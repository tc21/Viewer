using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private readonly ViewModel viewModel = new ViewModel();

        public MainWindow(string[] files) {
            InitializeComponent();

            this.DataContext = this.viewModel;

            Top = Properties.Settings.Default.Top;
            Left = Properties.Settings.Default.Left;
            Height = Properties.Settings.Default.Height;
            Width = Properties.Settings.Default.Width;
            CustomContextMenuItems = null;

            if (Top < 0) {
                Top = 0;
            }

            if (Left < 0) {
                Left = 0;
            }

            // args come from App, which is the command line arguments
            this.viewModel.LoadImages(files);
        }

        /* returns whether the index actually exists */
        public bool GoToIndex(int index) {
            if (!(0 <= index && index < viewModel.Images.Count)) {
                return false;
            }

            viewModel.CurrentImageIndex = index;
            return true;
        }

        public void Open(IEnumerable<string> paths) {
            if (paths.Count() == 1) {
                var path = paths.First();
                if (Directory.Exists(path)) {  // "file" is actually a directory!
                    this.viewModel.LoadImages(Directory.GetFiles(path));
                } else {
                    // we don't just load one file, but load the containing directory and navigate to this file.
                    var directoryName = Path.GetDirectoryName(path);
                    var directoryFiles = Directory.GetFiles(directoryName);
                    this.viewModel.LoadImages(directoryFiles);
                    this.viewModel.CurrentImageIndex = this.viewModel.Images.IndexOf(path);
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
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Open(files);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) { // This means scrolled up
                this.viewModel.CurrentImageIndex -= 1;
            } else {
                this.viewModel.CurrentImageIndex += 1;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (WindowState == WindowState.Maximized) {
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
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e) {
            this.viewModel.CurrentImageIndex += 1;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Right || e.Key == Key.Down) {
                this.viewModel.CurrentImageIndex += 1;
            }
            if (e.Key == Key.Left || e.Key == Key.Up) {
                this.viewModel.CurrentImageIndex -= 1;
            }
        }
    }
}
