﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BencodeNET.Torrents;
using RuTracker.Client;
using RuTracker.Client.Model;
using RuTracker.Client.Model.GetForumTopics.Request;
using RuTracker.Client.Model.SearchTopics.Request;
using RuTracker.Client.Model.SearchTopics.Response;

static void PrintSearchResult(SearchResult searchResult) =>
    Console.WriteLine(string.Join(Environment.NewLine, searchResult.Topics.Select(x => x.Title)));

static void ParseTorrent(byte[] torrentBytes) {
    var ms = new MemoryStream(torrentBytes);
    var torrentParser = new TorrentParser(TorrentParserMode.Strict);
    var torrent = torrentParser.Parse(ms);
    Console.WriteLine($"Here are '{torrent.DisplayName}' torrent files:");
    foreach (var file in torrent.Files) {
        Console.WriteLine(file.FullPath);
    }
}

static async Task<List<Forum>> GetAudioForums(RuTrackerClient client) {
    var forums = await client.GetForums();
    return forums
        .Where(x =>
            x.Path[0].EndsWith("музыка", StringComparison.OrdinalIgnoreCase) ||
            x.Path[0] == "Hi-Res форматы, оцифровки"
        )
        .ToList();
}

static async Task TestForumTopicsScraping(RuTrackerClient client) {
    var forums = await client.GetForums();
    var forum = forums.Single(x => x.Path.Last() == "Punk (lossless)");

    var getForumTopicsRequest = new GetForumTopicsRequest(
        ForumId: forum.Id,
        SortBy: GetForumTopicsSortBy.Registered,
        SortDirection: GetForumTopicsSortDirection.Ascending);
    var firstPage = await client.GetForumTopics(getForumTopicsRequest);
    for (var i = 2; i <= Math.Min(5, firstPage.PagesCount); i++) {
        var page = await client.GetForumTopics(getForumTopicsRequest with { Page = i });
    }
}

using var client = new RuTrackerClient();
await client.Login("cyberpunk777", "cyberpunk");

var req = new SearchTopicsRequest(
    Title: "Виктор Цой FLAC",
    SortBy: SearchTopicsSortBy.Downloads,
    SortDirection: SearchTopicsSortDirection.Descending
);

// Get first page of results
var resp = await client.SearchTopics(req);
Console.WriteLine($"Found {resp.Found} topics");
PrintSearchResult(resp);

// Download torrent file
// var torrentBytes = await client.GetTopicTorrentFile(resp.Topics.First().Id);
// ParseTorrent(torrentBytes);

// Get a magnet link of the first topic
// var topicId = resp.Topics.First().Id;
// var topic = await client.GetTopic(topicId);
// Console.WriteLine(topic.MagnetLink);

// Get all other pages
var nextPage = resp.NextPage;
while (nextPage != null) {
    resp = await client.SearchTopics(nextPage);
    PrintSearchResult(resp);
    nextPage = resp.NextPage;
}