using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomo.Util;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using Path = System.IO.Path;
using ArcGIS.Core.Geometry;
using System.Diagnostics;
using Geomo.ArcGisExtension;

namespace Geomo.ShapeFileClipper
{
    /// <summary>
    /// Interaction logic for ShapeFileClipper.xaml
    /// </summary>
    public partial class ShapeFileClipper : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        private ObservableCollection<string> _selectedShapeFiles;
        private ObservableCollection<ComboBoxValue<OverwriteMode>> _overwriteModes;
        private CoordinateSystemItem _selectedCoordinateSystem;

        private SelectCoordinateSystem selectReferenceSystemWindow;

        public ShapeFileClipper()
        {
            InitializeComponent();

            InitSelectionList();
            InitOverwriteModeComboBox();
            InitSelectReferenceSystemWindow();

            SubscribeEventHandlers();
        }

        private void InitSelectionList()
        {
            _selectedShapeFiles = new ObservableCollection<string>();
            SelectionList.ItemsSource = _selectedShapeFiles;

            SelectionList.Drop += OnSelectionListDragDrop;
            SelectionList.DragEnter += OnSelectionListDragEnter;
            SelectionList.DragOver += OnSelectionListDragOver;
        }

        private void InitSelectReferenceSystemWindow()
        {
            selectReferenceSystemWindow = new SelectCoordinateSystem();
            selectReferenceSystemWindow.CoordinateSystemChanged += OnCoordinateSystemChanged;
        }

        private void OnCoordinateSystemChanged(object source, CoordinateSystemWindowEventArgs args)
        {
            _selectedCoordinateSystem = args.CoordinateSystem;
            CoordinateSystemTextBox.Text = _selectedCoordinateSystem.Name;
        }

        private void InitOverwriteModeComboBox()
        {
            var defaultOverwriteMode = new ComboBoxValue<OverwriteMode>(OverwriteMode.Overwrite, "Overwrite existing files");
            _overwriteModes = new ObservableCollection<ComboBoxValue<OverwriteMode>>()
            {
                defaultOverwriteMode,
                new ComboBoxValue<OverwriteMode>(OverwriteMode.Backup, "Backup existing files"),
                new ComboBoxValue<OverwriteMode>(OverwriteMode.Skip, "Skip existing files")
            };
            OverwriteModeComboBox.SelectedItem = defaultOverwriteMode;
            OverwriteModeComboBox.ItemsSource = _overwriteModes;
        }

        private void SubscribeEventHandlers()
        {
            SelectionList.SelectionChanged += OnListBoxSelectionChanged;

            _selectedShapeFiles.CollectionChanged += OnListBoxContentChanged;
            _selectedShapeFiles.CollectionChanged += OnInputChanged;

            ClipExtentTextBox.SelectionChanged += OnInputChanged;
            OutputDirectoryTextBox.SelectionChanged += OnInputChanged;
        }

        private void OnSelectReferenceSystemClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            selectReferenceSystemWindow.ShowDialog();
        }

        private void OnAddFileClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                DefaultExt = ".shp",
                Filter = "Shape Files|*.shp"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            foreach (var fileName in dialog.FileNames.Except(_selectedShapeFiles))
            {
                _selectedShapeFiles.Add(fileName);
            }
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            RunButton.IsEnabled = _selectedShapeFiles.Count > 0
                                           && !string.IsNullOrWhiteSpace(ClipExtentTextBox.Text)
                                           && !string.IsNullOrWhiteSpace(OutputDirectoryTextBox.Text);
            OpenOutputDirectoryButton.IsEnabled = !string.IsNullOrWhiteSpace(OutputDirectoryTextBox.Text);
        }

        private void OnListBoxContentChanged(object sender, EventArgs e)
        {
            ClearSelectionButton.IsEnabled = _selectedShapeFiles.Count > 0;
        }

        private IEnumerable<string> GetShapeFilesFromDragEvent(DragEventArgs e)
        {
            return (e.Data.GetData(DataFormats.FileDrop) as string[]).Where(f => f.EndsWith(".shp"));
        }

        private void OnSelectionListDragEnter(object sender, DragEventArgs e)
        {
            var hasShapeFiles = e.Data.GetDataPresent(DataFormats.FileDrop) && GetShapeFilesFromDragEvent(e).Count() > 0;
            e.Effects = hasShapeFiles ? DragDropEffects.All : DragDropEffects.None;
        }

        private void OnSelectionListDragOver(object sender, DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop) && GetShapeFilesFromDragEvent(e).Count() > 0;
        }

        private void OnSelectionListDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var fileName in GetShapeFilesFromDragEvent(e).Except(_selectedShapeFiles))
                {
                    _selectedShapeFiles.Add(fileName);
                }
            }
        }

        private void OnListBoxSelectionChanged(object sender, EventArgs e)
        {
            RemoveFileButton.IsEnabled = SelectionList.SelectedItems.Count > 0;
        }

        private void OnRemoveFileClicked(object sender, RoutedEventArgs e)
        {
            var selection = (IList)SelectionList.SelectedItems;
            foreach (var selectedFile in new List<string>(selection.Cast<string>()))
            {
                _selectedShapeFiles.Remove(selectedFile);
            }
        }

        private void OnClearSelectionClicked(object sender, RoutedEventArgs e)
        {
            _selectedShapeFiles.Clear();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnOpenOutputDirectoryClicked(object sender, RoutedEventArgs e)
        {
            Process.Start(OutputDirectoryTextBox.Text);
        }

        private async void OnExecuteClicked(object sender, RoutedEventArgs e)
        {

            var progressDialog = new ProgressDialog("Running Shape File Clipper ...", "Cancel");
            progressDialog.Show();

            var ignoredShapeFiles = new HashSet<string>();

            var clipExtentShapeFile = ClipExtentTextBox.Text;
            var outputDirectory = OutputDirectoryTextBox.Text;
            var postfix = PostfixTextBox.Text;
            var backupFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
            var overwriteMode = ((ComboBoxValue<OverwriteMode>)OverwriteModeComboBox.SelectedItem).Value;
            var cancelHandler = new CancelableProgressorSource(progressDialog);

            var clipController = new ClipController(clipExtentShapeFile, outputDirectory, postfix, backupFolderName, overwriteMode, cancelHandler);

            foreach (var shapeFile in _selectedShapeFiles)
            {
                var hasFailed = !await clipController.ProcessShapeFile(shapeFile);
                if (hasFailed)
                {
                    ignoredShapeFiles.Add(shapeFile);
                }
            }

            if (ignoredShapeFiles.Count > 0)
            {
                var message = string.Join("\n", ignoredShapeFiles);
                MessageBox.Show(this, message, "Finished with ignored shape files", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show(this, "All shape files successfully clipped.", "Finished", MessageBoxButton.OK);
            }

            progressDialog.Hide();

        }

        private void OnSelectOutputDirectoryClicked(object sender, RoutedEventArgs e)
        {

            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OutputDirectoryTextBox.Text = dialog.FileName;
            }

        }

        private void OnSelectClipExtentClicked(object sender, RoutedEventArgs e)
        {

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".shp",
                Filter = "Shape Files|*.shp"
            };

            if (dialog.ShowDialog() == true)
            {
                ClipExtentTextBox.Text = dialog.FileName;
            }

        }
    }
}
