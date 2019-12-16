using System;
using System.Collections.Generic;

namespace RuTracker.Client.Model.Search.Response
{
    public sealed class TopicBriefInfo
    {
        public readonly int Id;
        public readonly string Title;
        public readonly TopicStatus TopicStatus;
        public readonly Category Category;
        public readonly IReadOnlyList<string> Tags;
        public readonly User Author;
        public readonly long SizeInBytes;
        public readonly int SeedsCount;
        public readonly int LeechsCount;
        public readonly int DownloadsCount;
        public readonly DateTime CreatedAt;

        public TopicBriefInfo(int id, string title, TopicStatus topicStatus, Category category, IReadOnlyList<string> tags, User author, long sizeInBytes, int seedsCount, int leechsCount, int downloadsCount, DateTime createdAt)
        {
            Id = id;
            Title = title;
            TopicStatus = topicStatus;
            Category = category;
            Tags = tags;
            Author = author;
            SizeInBytes = sizeInBytes;
            SeedsCount = seedsCount;
            LeechsCount = leechsCount;
            DownloadsCount = downloadsCount;
            CreatedAt = createdAt;
        }
    }
}