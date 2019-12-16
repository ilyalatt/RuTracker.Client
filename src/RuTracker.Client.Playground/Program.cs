using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BencodeNET.Torrents;
using RuTracker.Client.Model;
using RuTracker.Client.Model.Exceptions;
using RuTracker.Client.Model.Search.Request;
using RuTracker.Client.Model.Search.Response;

namespace RuTracker.Client.Playground
{
    static class Program
    {
        static async Task<RuTrackerClient> Login()
        {
            const string login = "LOGIN";
            const string password = "PASSWORD";
            var authClient = new RuTrackerAuthClient();
            var session = await authClient.Login(login, password);
            if (session == null)
            {
                throw new RuTrackerClientAuthException();
            }

            return authClient.WithSession(session);
        }

        static void PrintSearchResult(SearchResult searchResult) =>
            Console.WriteLine(string.Join(Environment.NewLine, searchResult.Topics.Select(x => x.Title)));

        // Use BencodeNET library to parse torrent
        static void ParseTorrent(byte[] torrentBytes)
        {
            var ms = new MemoryStream(torrentBytes);
            var torrentParser = new TorrentParser(TorrentParserMode.Strict);
            var torrent = torrentParser.Parse(ms);
            Console.WriteLine($"Here are '{torrent.DisplayName}' torrent files:");
            foreach (var file in torrent.Files)
            {
                Console.WriteLine(file.FullPath);
            }
        }

        static async Task<List<Category>> GetAudioCategories(RuTrackerClient client)
        {
            var categories = await client.GetCategories();
            return categories
                .Where(x =>
                    x.Path[0].EndsWith("музыка", StringComparison.OrdinalIgnoreCase) ||
                    x.Path[0] == "Hi-Res форматы, оцифровки"
                )
                .ToList();
        }

        static async Task Main()
        {
            using var client = await Login();
            var req = new SearchRequest(
                title: "Виктор Цой FLAC",
                sortBy: SortBy.Downloads,
                sortDirection: SortDirection.Descending
            );

            // Get first page of results
            var resp = await client.Search(req);
            Console.WriteLine($"Found {resp.Found} topics");
            PrintSearchResult(resp);
            
            // Download torrent
            // var torrentBytes = await client.GetTorrent(resp.Topics.First().Id);
            // ParseTorrent(torrentBytes);
            
            // Get a magnet link of the first topic
            // var topicId = resp.Topics.First().Id;
            // var topic = await client.GetTopic(topicId);
            // Console.WriteLine(topic.MagnetLink);

            // Get all other pages
            var nextPage = resp.NextPage;
            while (nextPage != null)
            {
                resp = await client.Search(nextPage);
                PrintSearchResult(resp);
                nextPage = resp.NextPage;
            }
        }
    }
}
