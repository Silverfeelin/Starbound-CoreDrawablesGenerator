using System;
using System.IO;

namespace CoreDrawablesGenerator
{
    /// <summary>
    /// Simple wrapper to keep track of one file, initially identified by its file path.
    /// This watcher detects changes to the file name, to the contents of the file and the parent directory name.
    /// If any 'higher' directory changes, these changes are still detected but the file path will no longer work; this makes it important to check if the file still exists.
    /// </summary>
    public class FileWatcher : IDisposable
    {
        public event Action FileDeleted;
        public event Action FileChanged;

        public bool IsDisposed { get; private set; } = false;

        public string FilePath {
            get
            {
                return Path.Combine(fileSystemWatcher.Path, fileSystemWatcher.Filter);
            }
        }

        FileSystemWatcher folderSystemWatcher;
        FileSystemWatcher fileSystemWatcher;

        public FileWatcher(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The FileWatcher can only watch existing files.");

            fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            fileSystemWatcher.Path = Path.GetDirectoryName(path);
            fileSystemWatcher.Filter = Path.GetFileName(path);

            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            // Keeps track of renamed folder. If any 'higher' folder is renamed, this method will still fail.
            folderSystemWatcher = new FileSystemWatcher();

            folderSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
            folderSystemWatcher.Path = Path.GetDirectoryName(fileSystemWatcher.Path);
            folderSystemWatcher.Filter = Path.GetFileName(fileSystemWatcher.Path);

            folderSystemWatcher.Renamed += FolderSystemWatcher_Renamed;

            fileSystemWatcher.EnableRaisingEvents = true;
            folderSystemWatcher.EnableRaisingEvents = true;
        }

        private void FolderSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            fileSystemWatcher.Path = e.FullPath;
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            // Catch changed directories
            if (Path.GetDirectoryName(e.FullPath) != Path.GetDirectoryName(e.OldFullPath))
            {
                fileSystemWatcher.Path = Path.GetDirectoryName(e.FullPath);
            }

            // Catch changed name
            if (e.Name != e.OldName)
            {
                fileSystemWatcher.Filter = e.Name;
            }
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (FileDeleted != null)
                FileDeleted.Invoke();

            Dispose();
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (FileChanged != null)
                FileChanged.Invoke();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                foreach (var item in FileDeleted.GetInvocationList())
                {
                    FileDeleted -= item as Action;
                }

                foreach (var item in FileChanged.GetInvocationList())
                {
                    FileChanged -= item as Action;
                }

                if (fileSystemWatcher != null)
                {
                    fileSystemWatcher.Dispose();
                    fileSystemWatcher = null;
                }
            }
        }
    }
}
