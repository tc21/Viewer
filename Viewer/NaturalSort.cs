using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Viewer {

    // note: this and Comics.Support.DirectoryInfoExtension should probaby be moved to a better (singular) place
    public static class NaturalSort {
        private static readonly FileUtilsInterop.NaturalFileSystemInfoComparer naturalComparer = new FileUtilsInterop.NaturalFileSystemInfoComparer();

        public static DirectoryInfo[] GetDirectoriesInNaturalOrder(this DirectoryInfo dir) {
            DirectoryInfo[] dirs = dir.GetDirectories();
            SortInNaturalOrder(dirs);
            return dirs;
        }

        public static DirectoryInfo[] GetDirectoriesInNaturalOrder(this DirectoryInfo dir, string searchPattern) {
            DirectoryInfo[] dirs = dir.GetDirectories(searchPattern);
            SortInNaturalOrder(dirs);
            return dirs;
        }

        public static FileInfo[] GetFilesInNaturalOrder(this DirectoryInfo dir) {
            FileInfo[] files = dir.GetFiles();
            SortInNaturalOrder(files);
            return files;
        }

        public static FileInfo[] GetFilesInNaturalOrder(this DirectoryInfo dir, string searchPattern) {
            FileInfo[] files = dir.GetFiles(searchPattern);
            SortInNaturalOrder(files);
            return files;
        }

        private static void SortInNaturalOrder<T>(T[] input) where T : FileSystemInfo {
            Array.Sort(input, naturalComparer);
        }
    }

    class FileUtilsInterop {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        public class NaturalFileSystemInfoComparer : IComparer<FileSystemInfo> {
            public int Compare(FileSystemInfo x, FileSystemInfo y) {
                return StrCmpLogicalW(x.Name, y.Name);
            }
        }
    }
}
