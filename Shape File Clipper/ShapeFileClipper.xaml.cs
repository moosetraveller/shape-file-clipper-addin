using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using System.Diagnostics;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Internal.Core.BrowseFilter;
using ESRI.ArcGIS.ItemIndex;
using Geomo.ShapeFileClipper.Utils;
using Geomo.ShapeFileClipper.CustomCoordinateSystemTree;
using Geomo.ShapeFileClipper.GeoProcessing;

namespace Geomo.ShapeFileClipper
{
    /// <summary>
    /// Interaction logic for ShapeFileClipper.xaml
    /// </summary>
    public partial class ShapeFileClipper : ProWindow
    {

        private ICoordinateSystemSelectionWindow _selectCoordinateSystemWindow;

        private BrowseProjectFilter _browseFilter;

        private ObservableCollection<string> _selectedShapeFiles;
        private ObservableCollection<ComboBoxValue<OverwriteMode>> _overwriteModes;
        private object _selectedCoordinateSystem;

        public ShapeFileClipper()
        {
            InitializeComponent();

            InitSelectionList();
            InitOverwriteModeComboBox();
            InitSelectReferenceSystemWindow();

            InitBrowseFilter();

            SubscribeEventHandlers();
        }

        private void InitBrowseFilter()
        {
            // C:\Program Files\ArcGIS\Pro\Resources\SearchResources\Schema
            _browseFilter = new BrowseProjectFilter("esri_browseDialogFilters_featureClasses_all")
            {
                Name = "Feature Classes"
            };
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
            //_selectCoordinateSystemWindow = new CoordinateSystemSelectionWindow();
            _selectCoordinateSystemWindow = new ArcCoordinateSystemSelectionWindow();
            _selectCoordinateSystemWindow.CoordinateSystemChanged += OnCoordinateSystemChanged;
        }

        private void OnCoordinateSystemChanged(object source, CoordinateSystemSelectorWindowEventArgs e)
        {
            _selectedCoordinateSystem = e.Selection;
            CoordinateSystemTextBox.Text = _selectedCoordinateSystem.ToString();
            OnInputChanged(source, EventArgs.Empty);
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
            _selectCoordinateSystemWindow.ShowDialog();
        }

        private void OnAddFileClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            //var dialog = new OpenFileDialog { Multiselect = true, DefaultExt = ".shp", Filter = "Shape Files|*.shp" };
            var dialog = new OpenItemDialog()
            {
                Title = "Add Feature Classes",
                MultiSelect = true,
                BrowseFilter = _browseFilter
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            //foreach (var fileName in dialog.FileNames.Except(_selectedShapeFiles)) { _selectedShapeFiles.Add(fileName); }
            foreach (var fileName in dialog.Items
                .Select(i => i.Path)
                .Except(_selectedShapeFiles))
            {
                _selectedShapeFiles.Add(fileName);
            }
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            RunButton.IsEnabled = _selectedShapeFiles.Count > 0
                                           && !string.IsNullOrWhiteSpace(ClipExtentTextBox.Text)
                                           && !string.IsNullOrWhiteSpace(OutputDirectoryTextBox.Text)
                                           && (!(DoProjectCheckBox.IsChecked ?? true) || _selectedCoordinateSystem != null) ;
            OpenOutputDirectoryButton.IsEnabled = !string.IsNullOrWhiteSpace(OutputDirectoryTextBox.Text);
        }

        private void OnListBoxContentChanged(object sender, EventArgs e)
        {
            ClearSelectionButton.IsEnabled = _selectedShapeFiles.Count > 0;
        }

        private IEnumerable<string> GetShapeFilesFromDragEvent(DragEventArgs e)
        {
            return (e.Data.GetData(DataFormats.FileDrop) as string[])?.Where(f => f.EndsWith(".shp")) ?? new string[0];
        }

        private void OnSelectionListDragEnter(object sender, DragEventArgs e)
        {
            var hasShapeFiles = e.Data.GetDataPresent(DataFormats.FileDrop) && GetShapeFilesFromDragEvent(e).Any();
            e.Effects = hasShapeFiles ? DragDropEffects.All : DragDropEffects.None;
        }

        private void OnSelectionListDragOver(object sender, DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop) && GetShapeFilesFromDragEvent(e).Any();
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
            var selection = SelectionList.SelectedItems;
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

            var clipController = new ClipTool(clipExtentShapeFile, outputDirectory, postfix, backupFolderName, overwriteMode, cancelHandler);

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

            //var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var dialog = new OpenItemDialog()
            {
                Title = "Select Output Location",
                Filter = ItemFilters.folders
            };

            if (dialog.ShowDialog() == true)
            {
                OutputDirectoryTextBox.Text = dialog.Items
                    .Select(i => i.Path)
                    .FirstOrDefault() ?? "";
            }

        }

        private void OnSelectClipExtentClicked(object sender, RoutedEventArgs e)
        {

            //var dialog = new OpenFileDialog { DefaultExt = ".shp",  Filter = "Shape Files|*.shp" };
            var dialog = new OpenItemDialog();

            if (dialog.ShowDialog() == true)
            {
                ClipExtentTextBox.Text = dialog.Items
                    .Select(i => i.Path)
                    .FirstOrDefault() ?? "";
            }

        }

        private void HandleDoProjectCheckBoxState(bool isChecked)
        {
            CoordinateSystemLabel.IsEnabled = isChecked;
            SelectCoordinateSystemButton.IsEnabled = isChecked;
            CoordinateSystemTextBox.IsEnabled = isChecked;
            CoordinateSystemLabel.FontWeight = isChecked ? FontWeights.Bold : FontWeights.Normal;
            OnInputChanged(this, EventArgs.Empty);
        }

        private void DoProjectCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HandleDoProjectCheckBoxState(true);
        }

        private void DoProjectCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleDoProjectCheckBoxState(false);
        }
    }
}
