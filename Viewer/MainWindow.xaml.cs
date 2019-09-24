using System;
using System.Collections.Generic;
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
        private readonly List<string> images = new List<string>();

        private int _currentImageIndex;
        public int CurrentImageIndex {
            get => _currentImageIndex;
            private set {
                if (images.Count == 0) {
                    return;
                }

                _currentImageIndex = ActualIndex(value);
                MainImage.Source = LoadImage(_currentImageIndex);
                UpdateTitle();
            }
        }

        // When a list of objects is assigned to CustomContextMenuItems, it build a new context menu.
        // Assign null to CustomContextMenuItems in object initialization to ensure the menu does property get built.
        private IList<object> _customContextMenuItems;
        public IList<object> CustomContextMenuItems {
            private get => _customContextMenuItems;
            set {
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

                if (value != null) {
                    foreach (var item in value) {
                        menuItems.Add(item);
                    }
                    menuItems.Add(new Separator());
                }

                foreach (var item in lower) {
                    menuItems.Add(item);
                }

                this._customContextMenuItems = value;
            }
        }

        // First of many apis for embedding in other programs
        public string CurrentImage => images[CurrentImageIndex];

        private string title = "Viewer";
        private readonly string[] imageExtensions = { ".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".tiff", ".tif" };

        public MainWindow(string[] files) {
            InitializeComponent();

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
            if (files != null && files.Length > 0) {
                ProcessFiles(files);
            }
        }

        /* returns whether the index actually exists */
        public bool GoToIndex(int index) {
            if (ActualIndex(index) != index) {
                return false;
            }

            CurrentImageIndex = index;
            return true;
        }

        private BitmapImage LoadImage(int index) {
            var file = images[ActualIndex(index)];
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file);
            image.EndInit();
            return image;
        }

        private int ActualIndex(int index) {
            return (index + images.Count) % images.Count;
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                images.Clear();
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ProcessFiles(files);
            }
        }

        private void ProcessFiles(string[] files) {
            if (files.Length == 1) {
                var file = files[0];
                if (Directory.Exists(file)) {
                    OpenFolder(file);
                } else {
                    OpenContainingFolder(file);
                }
            } else {
                OpenFiles(files);
            }
        }

        private void OpenContainingFolder(string path) {
            var directoryName = Path.GetDirectoryName(path);
            var files = Directory.GetFiles(directoryName);
            LoadFiles(files);

            if (images.Count > 0) {
                title = " (" + directoryName + ")";
                CurrentImageIndex = images.IndexOf(path);
            }
        }

        private void OpenFiles(string[] files) {
            LoadFiles(files);

            if (images.Count > 0) {
                title = " (" + images.Count.ToString() + " files loaded)";
                CurrentImageIndex = 0;
            }
        }

        private void OpenFolder(string path) {
            var files = Directory.GetFiles(path);
            LoadFiles(files);

            if (images.Count > 0) {
                title = " (" + path + ")";
                CurrentImageIndex = 0;
            }

            // This is very dangerous!!! For personal use only, I guess.
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs) {
                Properties.Settings.Default.Top += 15;
                Properties.Settings.Default.Left += 15;
                var newWindow = new MainWindow(new string[] { dir });
                newWindow.Show();
            }
        }

        private bool IsImage(string path) {
            return imageExtensions.Contains(Path.GetExtension(path).ToLower());
        }

        private void LoadFiles(IEnumerable<string> files) {
            foreach (var file in files) {
                if (!IsImage(file)) {
                    continue;
                }

                images.Add(file);
            }

            if (images.Count == 0) {
                Title = "No files loaded";
                MainImage.Source = null;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0)  // This means scrolled up
               {
                CurrentImageIndex -= 1;
            } else {
                CurrentImageIndex += 1;
            }
        }

        private void UpdateTitle() {
            var filename = Path.GetFileName(images[CurrentImageIndex]);
            Title = filename + title;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
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
            CurrentImageIndex += 1;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Right || e.Key == Key.Down) {
                CurrentImageIndex += 1;
            }
            if (e.Key == Key.Left || e.Key == Key.Up) {
                CurrentImageIndex -= 1;
            }
        }
    }
}
