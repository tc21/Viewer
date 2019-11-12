using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Viewer {
    partial class ViewModel {
        public ICommand TestCommand { get; } = new Commands.TestCommand();

        private ICommand seekRelativeCommand;
        public ICommand SeekRelativeCommand {
            get {
                if (this.seekRelativeCommand == null) {
                    this.seekRelativeCommand = new Commands.RelayCommand(
                        o => this.CurrentImageIndex += (int)this.TryParseInt(o),
                        o => {
                            var i = this.TryParseInt(o);

                            if (i == null) {
                                return false;
                            }

                            return (i > -this.Images.Count && i < this.images.Count);
                        }
                    );
                }

                return this.seekRelativeCommand;
            }
        }

        private ICommand seekCommand;
        public ICommand SeekCommand {
            get {
                if (this.seekCommand == null) {
                    this.seekCommand = new Commands.RelayCommand(
                        o => this.CurrentImageIndex = (int)this.TryParseInt(o),
                        o => {
                            var i = this.TryParseInt(o);

                            if (i == null) {
                                return false;
                            }

                            var target = (int)i;

                            // see ActualIndex(): it is designed to only work if target > -Images.Count
                            // we could have made it work with any number, but you're probably doing something wrong if it
                            // goes out of this range
                            return (target > -this.Images.Count && target < this.Images.Count);
                        }
                    );
                }

                return this.seekCommand;
            }
        }

        private ICommand showInExplorerCommand;
        public ICommand ShowInExplorerCommand {
            get {
                if (this.showInExplorerCommand == null) {
                    this.showInExplorerCommand = new Commands.RelayCommand(
                        o => {
                            var proc = new Process();
                            proc.StartInfo.FileName = "explorer.exe";
                            proc.StartInfo.Arguments = string.Format("/select,\"{0}\"", this.CurrentImage);
                            proc.Start();
                        },
                        this.IsViewingOpenFile
                    );
                }

                return this.showInExplorerCommand;
            }
        }

        private ICommand toggleMetadataCommand;
        public ICommand ToggleMetadataCommand {
            get {
                if (this.toggleMetadataCommand == null) {
                    this.toggleMetadataCommand = new Commands.RelayCommand(
                        o => {
                            if (this.MetadataVisibility == Visibility.Visible) {
                                this.MetadataVisibility = Visibility.Hidden;
                            } else {
                                this.MetadataVisibility = Visibility.Visible;
                            }
                        }
                    );
                }

                return this.toggleMetadataCommand;
            }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand {
            get {
                if (this.deleteCommand == null) {
                    this.deleteCommand = new Commands.RelayCommand(
                        _ => {
                            // this doesn't work currently because the image is still loaded and the file handle is still held!
                            var response = MessageBox.Show("Are you sure you want to move this file to the Recycle Bin?", "Confirm file deletion", MessageBoxButton.YesNo);
                            if (response == MessageBoxResult.Yes) {
                                var currentImage = this.CurrentImage;
                                FileSystem.DeleteFile(currentImage, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                                this.Images.Remove(currentImage);
                                // this refreshes the view model; maybe we should have to call a ViewModel.RemoveImage instead?
                                this.CurrentImageIndex = this.CurrentImageIndex;
                            }
                        },
                        // this.IsViewingOpenFile // disabled for now
                        _ => false
                    );
                }

                return this.deleteCommand;
            }
        }

        private ICommand closeWindowCommand;
        public ICommand CloseWindowCommand {
            get {
                if (this.closeWindowCommand == null) {
                    this.closeWindowCommand = new Commands.RelayCommand(
                        _ => this.window.Close()
                    );
                }

                return this.closeWindowCommand;
            }
        }

        private int? TryParseInt(object o) {
            int? i = null;

            if (o is int i_) {
                i = i_;
            } else if (o is string s) {
                var success = int.TryParse(s, out var i__);
                if (success) {
                    i = i__;
                }
            }

            return i;
        }

        private bool IsViewingOpenFile(object _) => this.currentImageIndex < this.Images.Count;
    }
}
