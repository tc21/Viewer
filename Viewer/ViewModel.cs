using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Viewer {
    class ViewModel : INotifyPropertyChanged {

        public readonly string[] ImageExtensions = {
            ".bmp", ".gif", ".heic", ".heif", ".j2k", ".jfi", ".jfif", ".jif", ".jp2", ".jpe", ".jpeg", ".jpf",
            ".jpg", ".jpm", ".jpx", ".mj2", ".png", ".tif", ".tiff", ".webp"
        };

        // if you set this directly, make sure all items are valid images
        private List<string> images = new List<string>();
        public List<string> Images {
            get => images;
            set {
                if (images == value) {
                    return;
                }

                images = value;
                CurrentImageIndex = 0;  // has the side effect of the NotifyPropertyChanges in CurrentImage::get
            }
        }


        private int currentImageIndex;
        public int CurrentImageIndex {
            get => currentImageIndex;
            set {
                if (Images.Count == 0 || currentImageIndex == value) {
                    return;
                }

                currentImageIndex = ActualIndex(value);
                NotifyPropertyChanged(nameof(CurrentImageSource));
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public string CurrentImage => Images[CurrentImageIndex];


        public string Title {
            get {
                var title = "No files loaded";

                if (Images.Count > 0) {
                    title = string.Format("{0} ({1}/{2} Files)", Path.GetFileName(CurrentImage), CurrentImageIndex + 1, Images.Count);
                }

                return title + " - Viewer";
            }
        }

        public BitmapImage CurrentImageSource => GetBitmapImage(CurrentImageIndex);


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadImages(IEnumerable<string> files) {
            var images = new List<string>();

            foreach (var file in files) {
                if (IsImage(file)) {
                    images.Add(file);
                }
            }

            Images = images;
        }

        private int ActualIndex(int index) {
            return (index + images.Count) % images.Count;
        }

        // note: index must be valid!
        private BitmapImage GetBitmapImage(int index) {
            var file = images[index];
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file);
            image.EndInit();
            return image;
        }

        private bool IsImage(string path) {
            return ImageExtensions.Contains(Path.GetExtension(path).ToLower());
        }
    }
}
