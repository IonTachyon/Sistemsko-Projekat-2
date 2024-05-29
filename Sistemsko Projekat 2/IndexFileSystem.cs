using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_Projekat_2
{
    public class IndexFileSystem
    {
        private ConcurrentListDictionary index;

        public IndexFileSystem(ConcurrentListDictionary index)
        {
            this.index = index; 
        }

        public async Task Start()
        {
            await SearchDirectory(Directory.GetCurrentDirectory());
        }

        public async Task SearchDirectory(string currentDirectory)
        {
            if (currentDirectory == null)
            {
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(currentDirectory);
            var files = dirInfo.EnumerateFiles();
            var directories = dirInfo.EnumerateDirectories();

            // Dodaj svaki .txt u indeks
            foreach (FileInfo file in files)
            {
                var fileName = file.Name;
                var fileExtension = file.Extension;

                if (fileExtension == ".txt")
                {
                    index.TryAdd(file.Name, file.FullName);
                }
            }

            List<Task> tasks = new();

            // Za svaki folder, uradi ovo ponovo.
            foreach (DirectoryInfo directory in directories)
            {
                if(directory.FullName != null)
                {
                    tasks.Add(SearchDirectory(directory.FullName));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
