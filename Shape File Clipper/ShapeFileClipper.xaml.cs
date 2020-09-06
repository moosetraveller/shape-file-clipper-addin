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

namespace Shape_File_Clipper
{
    /// <summary>
    /// Interaction logic for ShapeFileClipper.xaml
    /// </summary>
    public partial class ShapeFileClipper : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        private ObservableCollection<string> _selectedShapeFiles;
        private ObservableCollection<ComboBoxValue<OverwriteMode>> _overwriteModes;

        public ShapeFileClipper()
        {
            InitializeComponent();

            this.InitSelectionList();
            this.InitOverwriteModeComboBox();

            this.SubscribeEventHandlers();
        }

        private void InitSelectionList()
        {
            this._selectedShapeFiles = new ObservableCollection<string>();
            this.SelectionList.ItemsSource = this._selectedShapeFiles;
        }

        private void InitOverwriteModeComboBox()
        {
            var defaultOverwriteMode = 
                new ComboBoxValue<OverwriteMode>(OverwriteMode.Overwrite, "Overwrite existing files");
            this._overwriteModes = new ObservableCollection<ComboBoxValue<OverwriteMode>>()
            {
                defaultOverwriteMode,
                new ComboBoxValue<OverwriteMode>(OverwriteMode.Backup, "Backup existing files"),
                new ComboBoxValue<OverwriteMode>(OverwriteMode.Skip, "Skip existing files")
            };
            this.OverwriteModeComboBox.SelectedItem = defaultOverwriteMode;
            this.OverwriteModeComboBox.ItemsSource = this._overwriteModes;
        }

        private void SubscribeEventHandlers()
        {
            this.SelectionList.SelectionChanged += OnListBoxSelectionChanged;

            this._selectedShapeFiles.CollectionChanged += OnListBoxContentChanged;
            this._selectedShapeFiles.CollectionChanged += OnInputChanged;

            this.ClipExtentTextBox.SelectionChanged += OnInputChanged;
            this.OutputDirectoryTextBox.SelectionChanged += OnInputChanged;
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
                this._selectedShapeFiles.Add(fileName);
            }
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            this.ExecuteButton.IsEnabled = this._selectedShapeFiles.Count > 0
                                           && !string.IsNullOrWhiteSpace(this.ClipExtentTextBox.Text)
                                           && !string.IsNullOrWhiteSpace(this.OutputDirectoryTextBox.Text);
        }

        private void OnListBoxContentChanged(object sender, EventArgs e)
        {
            this.ClearSelectionButton.IsEnabled = this._selectedShapeFiles.Count > 0;
        }

        private void OnListBoxSelectionChanged(object sender, EventArgs e)
        {
            this.RemoveFileButton.IsEnabled = this.SelectionList.SelectedItems.Count > 0;
        }

        private void OnRemoveFileClicked(object sender, RoutedEventArgs e)
        {
            var selection = (IList) this.SelectionList.SelectedItems;
            foreach (var selectedFile in new List<string>(selection.Cast<string>()))
            {
                this._selectedShapeFiles.Remove(selectedFile);
            }
        }

        private void OnClearSelectionClicked(object sender, RoutedEventArgs e)
        {
            this._selectedShapeFiles.Clear();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            progressDialog.Hide();

            if (ignoredShapeFiles.Count > 0)
            {
                var message = string.Join("\n", ignoredShapeFiles);
                MessageBox.Show(this, message, "Finished with ignored shape files", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show(this, "All shape files successfully clipped.", "Finished", MessageBoxButton.OK);
            }

        }

        private void OnSelectOutputDirectoryClicked(object sender, RoutedEventArgs e)
        {

            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.OutputDirectoryTextBox.Text = dialog.FileName;
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
