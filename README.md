# RuTracker.Client

[![NuGet version](https://badge.fury.io/nu/RuTracker.Client.svg)](https://www.nuget.org/packages/RuTracker.Client)

## Quick start

```C#
var client = new RuTrackerClient();
await client.Login("test_account", "qwerty12345");
var res = await client.SearchTopics(new SearchTopicsRequest(
    Title: "Виктор Цой FLAC",
    SortBy: SearchTopicsSortBy.Downloads,
    SortDirection: SearchTopicsSortDirection.Descending
));
var topic = res.Topics.First();
var torrentFileBytes = await client.GetTopicTorrentFile(topic.Id);
```

For the complete example check out [Playground](https://github.com/ilyalatt/RuTracker.Client/blob/master/src/RuTracker.Client.Playground/Program.cs).

## Implemented methods

* SearchTopics
* GetTopic
* GetTopicFileTree
* GetTopicTorrentFile
* GetForums
* GetForumTopics
