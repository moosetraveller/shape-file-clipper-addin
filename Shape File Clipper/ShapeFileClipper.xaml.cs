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
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

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

            this.SelectionList.SelectionChanged += OnSelectionChange;
            this._selectedShapeFiles.CollectionChanged += OnInputChange;
            this.ClipExtentTextBox.SelectionChanged += OnInputChange;
            this.OutputDirectoryTextBox.SelectionChanged += OnInputChange;
        }

        private void OnAddFileClick(object sender, RoutedEventArgs routedEventArgs)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.DefaultExt = ".shp";
            dialog.Filter = "Shape Files|*.shp";
            if (dialog.ShowDialog() == true)
            {
                foreach (var fileName in dialog.FileNames.Except(_selectedShapeFiles))
                {
                    this._selectedShapeFiles.Add(fileName);
                }
            }
        }

        private void OnInputChange(object sender, EventArgs e)
        {
            var doEnableExecuteButton = this._selectedShapeFiles.Count > 0
                && !string.IsNullOrWhiteSpace(this.ClipExtentTextBox.Text)
                && !string.IsNullOrWhiteSpace(this.OutputDirectoryTextBox.Text);
            this.ExecuteButton.IsEnabled = doEnableExecuteButton;

            var doEnableClearAllButton = this._selectedShapeFiles.Count > 0;
            this.ClearSelectionButton.IsEnabled = doEnableClearAllButton;
        }

        private void OnSelectionChange(object sender, EventArgs e)
        {
            var doEnableRemoveButton = this.SelectionList.SelectedItems.Count > 0;
            this.RemoveFileButton.IsEnabled = doEnableRemoveButton;
        }

        private void OnRemoveFileClick(object sender, RoutedEventArgs e)
        {
            var selection = (IList) this.SelectionList.SelectedItems;
            foreach (string selectedFile in new List<string>(selection.Cast<string>()))
            {
                this._selectedShapeFiles.Remove(selectedFile);
            }
        }

        private void OnClearSelectionClick(object sender, RoutedEventArgs e)
        {
            this._selectedShapeFiles.Clear();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnExecuteClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnSelectOutputDirectoryClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.OutputDirectoryTextBox.Text = dialog.FileName;
            }
        }

        private void OnSelectClipExtentClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".shp";
            dialog.Filter = "Shape Files|*.shp";
            if (dialog.ShowDialog() == true)
            {
                ClipExtentTextBox.Text = dialog.FileName;
            }
        }
    }
}
