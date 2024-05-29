using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Sistemsko_Projekat_2
{
    internal class HttpServer
    {
        string url;
        int capacity;
        int concurrency;
        HttpListener listener;
        bool run;
        private ConcurrentDictionary<string, PalindromeResponse> _cache;
        private ConcurrentListDictionary _index;
        private IndexFileSystem idf; 
        public HttpServer(string inurl, int concurrency, int capacity, ConcurrentListDictionary index, IndexFileSystem idf)
        {
            this.capacity = capacity;
            this.concurrency = concurrency;
            url = inurl;
            listener = new HttpListener();
            run = true;
            _cache = new ConcurrentDictionary<string, PalindromeResponse>(concurrency, capacity);
            this._index = index;
            this.idf = idf;
        }

        public async Task Start()
        {
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine($"Listening to connections on {url}");
            while(run)
            {
                // podaci o http zahtevu i odgovoru
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // PalindromeResponse je struct koji sadrzi skup fajlova i broj palindroma u njima
                PalindromeResponse palindromeResponse;

                // fajl koji se pretrazuje
                string searchedFile = "";

                if(request.RawUrl != null)
                {
                    // uklanja / iz url
                    searchedFile = request.RawUrl.Substring(1);
                }

                // ako ima u kešu, onda pročitaj iz keša. ako ne, pretraži
                if(_cache.ContainsKey(searchedFile))
                {
                    _cache.TryGetValue(searchedFile, out palindromeResponse);
                    var oldResponse = new PalindromeResponse(palindromeResponse);
                    palindromeResponse.LastRequested = DateTime.Now;
                    _cache.TryUpdate(searchedFile, palindromeResponse, oldResponse);
                    Console.WriteLine("Value obtained from cache");
                }

                // ako nema u kešu, proveri indeks
                else
                {
                    List<string> files;

                    if(_index.TryGetValue(searchedFile, out files))
                    {
                        // prebrojava palindrome u fajlu
                        palindromeResponse = await PalindromeCounter.SearchFiles(files);

                        // ako je keš preko kapaciteta, onda zameniti 
                        // ako nije, samo dodaj
                        if (_cache.Count < capacity)
                        {
                            _cache.TryAdd(searchedFile, palindromeResponse);
                        }

                        else
                        {
                            ReplaceLeastRecentlyUsed(searchedFile, palindromeResponse);
                        }
                    }
                    else
                    {
                        palindromeResponse = new();
                    }
                }

                // validacija zahteva
                int filecount = palindromeResponse.Results.Length;
                int palindromes = 0;
                for(int i = 0; i < filecount; i++)
                {
                    palindromes += palindromeResponse.Results[i].Item2;
                }

                byte[] buffer;

                // tip odgovora
                if(filecount == 0)
                {
                    buffer = ResponseFileNotFound();
                }
                else if (palindromes == 0)
                {
                    buffer = ResponseNoPalindrome();
                }
                else
                {
                    buffer = ResponseNormal(palindromeResponse, filecount);
                }

                // odgovor klijentu
                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }
        // html odgovor kad je fajl nadjen i ima palndrome
        private byte[] ResponseNormal(PalindromeResponse palindromeResponse, int filecount)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<html><body><h1>");
            stringBuilder.Append("Broj fajlova u fajl sistemu je");
            stringBuilder.Append($" {filecount}. ");
            stringBuilder.Append("Ti fajlovi su:</h1>");
            stringBuilder.Append("<ul>");
            for (int i = 0; i < palindromeResponse.Results.Length; i++)
            {
                stringBuilder.Append($"<li>{palindromeResponse.Results[i].Item1}, sa {palindromeResponse.Results[i].Item2} palindroma.</li>");
            }
            stringBuilder.Append("</ul>");
            stringBuilder.Append("</body></html>");

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        // html odgovor kad fajl nije nadjen
        private byte[] ResponseFileNotFound()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<html><body><h1>");
            stringBuilder.Append("Dati fajl ne postoji.");
            stringBuilder.Append("</h1></body></html>");

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        // html odgovor kad nema palindroma
        private byte[] ResponseNoPalindrome()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<html><body><h1>");
            stringBuilder.Append("U trazenim fajlovima nema palindroma.");
            stringBuilder.Append("</h1></body></html>");

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        // least recently used algoritam za zamenu elemenata u kešu
        private void ReplaceLeastRecentlyUsed(string query, PalindromeResponse palindromeResponse)
        {

            var responses = _cache.GetEnumerator();
            responses.MoveNext();
            var leastRecentlyUsed = responses.Current;
            foreach(var response in _cache)
            {
                if(response.Value.LastRequested < leastRecentlyUsed.Value.LastRequested)
                {
                    leastRecentlyUsed = response;
                }
            }

            _cache.TryRemove(leastRecentlyUsed.Key, out var removedResponse);
            _cache.TryAdd(query, palindromeResponse);

            Console.WriteLine($"Removed query {leastRecentlyUsed.Key} in favor of {query}.");
        }
    }
}
