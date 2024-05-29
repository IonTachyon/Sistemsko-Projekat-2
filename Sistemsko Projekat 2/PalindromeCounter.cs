using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_Projekat_2
{
    internal static class PalindromeCounter
    {
        public static async Task<PalindromeResponse> SearchFiles(List<string> files)
        {
            List<Task<Tuple<string, int>>> tasks = new();
            foreach (string file in files)
            {
                tasks.Add(Task.Run(() => SearchFile(file)));
            }

            Tuple<string, int>[] results = await Task.WhenAll(tasks);

            return new PalindromeResponse(results);
        }

        private static Tuple<string, int> SearchFile(string file)
        {
            string fileContents = File.ReadAllText(file);
            string[] words = fileContents.Split(' ');
            int palindromeCount = 0;
            foreach (string word in words)
            {
                char[] letters = word.ToCharArray();
                char[] reverseLetters = new char[letters.Length];
                Array.Copy(letters, reverseLetters, letters.Length);
                Array.Reverse(reverseLetters);
                bool isPalindrome = reverseLetters.SequenceEqual(letters);
                if (isPalindrome)
                {
                    palindromeCount++;
                }
            }
            return Tuple.Create(file, palindromeCount);
        }
    }
}
