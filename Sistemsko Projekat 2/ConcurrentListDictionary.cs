using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_Projekat_2
{
    public class ConcurrentListDictionary
    {
        private ConcurrentDictionary<string, List<string>> internalDictionary;

        public ConcurrentListDictionary(int concurrency, int capacity)
        {
            internalDictionary = new ConcurrentDictionary<string, List<string>>(concurrency, capacity);
        }

        public bool TryAdd(string key, string value)
        {
            bool added = false;

            if (internalDictionary.ContainsKey(key))
            {
                List<string> list = this.internalDictionary[key];
                if (list.Contains(value) == false)
                {
                    list.Add(value);
                    added = true;
                }
            }
            else
            {
                List<string> list = new List<string>();
                list.Add(value);
                added = this.internalDictionary.TryAdd(key, list);
            }

            return added;
        }

        public bool ContainsKey(string key) { return internalDictionary.ContainsKey(key); }

        public List<string> LogAllFiles()
        {
            List<string> allFiles = new();

            foreach(var item in internalDictionary)
            {
                Console.WriteLine(item.Key);
                foreach(var value in item.Value)
                {
                    Console.WriteLine(item.Key + " " + value);
                    allFiles.Add(value);
                }
            }

            return allFiles;
        }

        public bool TryGetValue(string key, out List<string> value)
        {
            var internalValue = new List<string>();
            var getSuccess = internalDictionary.TryGetValue(key, out internalValue);
            value = internalValue;
            return getSuccess;
        }
    }
}
