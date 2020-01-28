# RuTracker.Client

[![NuGet version](https://badge.fury.io/nu/RuTracker.Client.svg)](https://www.nuget.org/packages/RuTracker.Client)

## Quick start

Create a client:

```C#
var authClient = new RuTrackerAuthClient();
var session = await authClient.Login("login", "password");
var client = authClient.WithSession(session);
```

Use it:

```C#
var res = await client.SearchTopics(new SearchTopicsRequest(
    title: "Виктор Цой FLAC",
    sortBy: SearchTopicsSortBy.Downloads,
    sortDirection: SearchTopicsSortDirection.Descending
));
var topic = res.Topics.First();
var torrentBytes = await client.GetTorrentTorrent(topic.Id);
```

For the complete example check out [Playground](https://github.com/ilyalatt/RuTracker.Client/blob/master/src/RuTracker.Client.Playground/Program.cs).

## Implemented methods

* GetForums
* GetForumTopics
* SearchTopics
* GetTopic
* GetTopicTorrent
* GetTopicFileTree
