using Sistemsko_Projekat_2;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;

class Program
{
    static async Task Main()
    {
        const int capacity = 1000;
        const int concurrency = 32;
        const string url = "http://localhost:5050/";

        ThreadPool.SetMaxThreads(16, 16);
        ConcurrentListDictionary index = new(32, 100000);
        IndexFileSystem idf = new(index);

        await idf.Start();
        var files = index.LogAllFiles();

        PalindromeResponse response = await PalindromeCounter.SearchFiles(files);

        HttpServer server = new HttpServer(url, concurrency, capacity, index, idf);
        await server.Start();

        Console.WriteLine("Done!");
    }
}