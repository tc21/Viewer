using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Viewer {
    partial class ViewModel : INotifyPropertyChanged {

        public readonly string[] ImageExtensions = {
            ".bmp", ".gif", ".heic", ".heif", ".j2k", ".jfi", ".jfif", ".jif", ".jp2", ".jpe", ".jpeg", ".jpf",
            ".jpg", ".jpm", ".jpx", ".mj2", ".png", ".tif", ".tiff", ".webp"
        };

        // if you set this directly, make sure all items are valid images
        private List<string> images = new List<string>();
        public List<string> Images {
            get => images;
            set {
                if (images == value || value == null || value.Count == 0) {
                    return;
                }

                images = value;
                currentImageIndex = 0;  // has the side effect of the NotifyPropertyChanges in CurrentImage::get
                NotifyPropertyChanged(nameof(Images));
                NotifyPropertyChanged(nameof(CurrentImage));
                NotifyPropertyChanged(nameof(CurrentImageIndex));
                NotifyPropertyChanged(nameof(CurrentImageSource));
                NotifyPropertyChanged(nameof(CurrentImageMetadata));
                NotifyPropertyChanged(nameof(Title));
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
                NotifyPropertyChanged(nameof(CurrentImage));
                NotifyPropertyChanged(nameof(CurrentImageIndex));
                NotifyPropertyChanged(nameof(CurrentImageSource));
                NotifyPropertyChanged(nameof(CurrentImageMetadata));
                NotifyPropertyChanged(nameof(Title));
            }
        }

        private Visibility metadataVisibility;
        public Visibility MetadataVisibility {
            get => metadataVisibility;
            set {
                if (metadataVisibility == value) {
                    return;
                }

                metadataVisibility = value;
                NotifyPropertyChanged(nameof(MetadataVisibility));
            }
        }

        public string CurrentImage {
            get {
                if (CurrentImageIndex < Images.Count) {
                    return Images[CurrentImageIndex];
                }

                return null;
            }
        }

        public string Title {
            get {
                var title = "No files loaded";

                if (Images.Count > 0) {
                    title = string.Format("{0} ({1}/{2} Files)", Path.GetFileName(CurrentImage), CurrentImageIndex + 1, Images.Count);
                }

                return title + " - Viewer";
            }
        }

        public BitmapSource CurrentImageSource => GetBitmapSource(CurrentImageIndex);
        public BitmapMetadata CurrentImageMetadata => GetBitmapMetadata(CurrentImageIndex);

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // If you use this, it is your responsibility to ensure all paths are valid
        public void LoadImages(IEnumerable<string> files) {
            var images = new List<string>();

            foreach (var file in files) {
                if (IsImage(file)) {
                    images.Add(file);
                }
            }

            Images = images;
        }

        public void LoadImages(IEnumerable<FileInfo> files) {
            LoadImages(files.Select(e => e.FullName));
        }

        private int ActualIndex(int index) {
            return (index + images.Count) % images.Count;
        }

        // note: index must be valid!
        private BitmapSource GetBitmapSource(int index) {
            var file = images[index];

            // this is faster than using a BitmapFrame
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file);
            image.EndInit();
            return image;
        }
        
        private BitmapMetadata GetBitmapMetadata(int index) {
            var file = images[index];

            try {
                return (BitmapMetadata)BitmapFrame.Create(new Uri(file)).Metadata;
            } catch (NotSupportedException) {
                return null;
            }
        }

        private bool IsImage(string path) {
            return ImageExtensions.Contains(Path.GetExtension(path).ToLower());
        }
    }
}
