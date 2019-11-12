using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

namespace Viewer {
    public partial class MainWindow : Window {

        // Expose view model properties to the public API here

        // When a list of objects is assigned to CustomContextMenuItems, it build a new context menu.
        // Assign null to CustomContextMenuItems in object initialization to ensure the menu does property get built.


        private IList<object> customContextMenuItems;
        public IList<object> CustomContextMenuItems {
            set {
                if (this.customContextMenuItems == value) {
                    return;
                }

                this.customContextMenuItems = value;
                BuildContextMenu(value);
            }
        }

        public string CurrentImage => this.viewModel.CurrentImage;
        public int CurrentImageIndex => this.viewModel.CurrentImageIndex;

        private readonly ViewModel viewModel;

        public MainWindow(string[] files) {
            InitializeComponent();

            this.viewModel = new ViewModel(this);
            this.DataContext = this.viewModel;
            this.viewModel.PropertyChanged += this.ViewModel_PropertyChanged;

            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            this.Metadata.Visibility = Properties.Settings.Default.MetadataVisible ? Visibility.Visible : Visibility.Hidden;
            BuildContextMenu(null);

            if (this.Top < 0) {
                this.Top = 0;
            }

            if (this.Left < 0) {
                this.Left = 0;
            }

            Open(files);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.viewModel.CurrentImage)
                || e.PropertyName == nameof(this.viewModel.MetadataVisibility)) {
                if (this.viewModel.MetadataVisibility == Visibility.Visible) {
                    UpdateMetadata();
                }
            }
        }

        /* returns whether the index actually exists */
        public bool GoToIndex(int index) {
            if (!(0 <= index && index < this.viewModel.Images.Count)) {
                return false;
            }

            this.viewModel.CurrentImageIndex = index;
            return true;
        }

        public void Open(IEnumerable<string> paths) {
            if (paths.Count() == 1) {
                var path = paths.First();
                if (Directory.Exists(path)) {  // "file" is actually a directory!
                    this.viewModel.LoadImages(new DirectoryInfo(path).GetFilesInNaturalOrder());
                } else {
                    // we don't just load one file, but load the containing directory and navigate to this file.
                    var fileInfo = new FileInfo(path);
                    var directoryName = fileInfo.DirectoryName;
                    var directoryFiles = new DirectoryInfo(directoryName).GetFilesInNaturalOrder();

                    this.viewModel.LoadImages(directoryFiles);

                    for (var i = 0; i < this.viewModel.Images.Count; i++) {
                        if (this.viewModel.Images[i] == fileInfo.FullName) {
                            this.viewModel.CurrentImageIndex = i;
                        }
                    }
                }
            } else {
                this.viewModel.LoadImages(paths);
            }
        }

        private void BuildContextMenu(IEnumerable<object> customContextMenuItems) {
            if (this.ContextMenu == null) {
                this.ContextMenu = new ContextMenu();
            }

            var menuItems = this.ContextMenu.Items;

            menuItems.Clear();

            var upper = (object[])this.FindResource("UpperContextMenu");
            var lower = (object[])this.FindResource("LowerContextMenu");

            foreach (var item in upper) {
                menuItems.Add(item);
            }
            menuItems.Add(new Separator());

            if (customContextMenuItems != null) {
                foreach (var item in customContextMenuItems) {
                    menuItems.Add(item);
                }
                menuItems.Add(new Separator());
            }

            foreach (var item in lower) {
                menuItems.Add(item);
            }

            this.ContextMenu.DataContext = this.viewModel;
        }

        private void UpdateMetadata() {
            // TODO: eventually this should be moved to the xaml
            if (this.viewModel.CurrentImage == null) {
                return;
            }

            var info = new FileInfo(this.viewModel.CurrentImage);
            var metadata = this.viewModel.CurrentImageMetadata;

            this.ImageSizeIndicator.Text = string.Format("File size: {0} bytes", info.Length);
            this.ImageLastModifiedIndicator.Text = string.Format("Last modified: {0}", info.LastWriteTime);

            try {
                this.ImageDimensionsIndicator.Text = string.Format("Dimensions: {0}x{1}", this.viewModel.CurrentImageSource.PixelWidth, this.viewModel.CurrentImageSource.PixelHeight);
            } catch (NotSupportedException) {
                this.ImageDimensionsIndicator.Text = "This image could not be loaded";
            }

            if (metadata != null) {
                try {
                    this.ImageMetadataIndicator.Text = string.Format(
                        "Metadata: {0}, {1}, {2} {3}, {4}",
                        metadata.Format, metadata.DateTaken, metadata.CameraManufacturer, metadata.CameraModel, metadata.ApplicationName
                    );
                } catch (NotSupportedException) {
                    this.ImageMetadataIndicator.Text = string.Format("Metadata: {0}", metadata.Format);
                }
            } else {
                this.ImageMetadataIndicator.Text = "";
            }
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Open(files);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) { // This means scrolled up
                this.viewModel.SeekRelativeCommand.Execute(-1);
            } else {
                this.viewModel.SeekRelativeCommand.Execute(1);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (this.WindowState == WindowState.Maximized) {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
            } else {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
            }

            Properties.Settings.Default.MetadataVisible = this.Metadata.Visibility == Visibility.Visible;
        }
    }
}
