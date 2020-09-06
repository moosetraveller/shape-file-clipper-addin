using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;
using Path = System.IO.Path;

namespace Shape_File_Clipper
{
    /// <summary>
    /// Interaction logic for ShapeFileClipper.xaml
    /// </summary>
    public partial class ShapeFileClipper : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        private readonly ObservableCollection<string> _selectedShapeFiles;
        
        public ShapeFileClipper()
        {
            InitializeComponent();

            this._selectedShapeFiles = new ObservableCollection<string>();
            this.SelectionList.ItemsSource = this._selectedShapeFiles;

            this.SubscribeEventHandlers();
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

            var cancelHandler = new CancelableProgressorSource(progressDialog);

            foreach (var shapeFile in _selectedShapeFiles)
            {

                var shapeFileName = Path.GetFileNameWithoutExtension(shapeFile) + "_Clipped.shp";
                var outputPath = Path.Combine(this.OutputDirectoryTextBox.Text, shapeFileName);
                var clipExtent = this.ClipExtentTextBox.Text;

                var parameters = await QueuedTask.Run(() => 
                {
                    return Geoprocessing.MakeValueArray(shapeFile, clipExtent, outputPath);
                });

                await Geoprocessing.ExecuteToolAsync("analysis.Clip", parameters, null, cancelHandler.Progressor, GPExecuteToolFlags.None | GPExecuteToolFlags.GPThread);

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
