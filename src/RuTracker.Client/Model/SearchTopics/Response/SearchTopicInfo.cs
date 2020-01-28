using System;
using System.Collections.Generic;

namespace RuTracker.Client.Model.SearchTopics.Response
{
    public sealed class SearchTopicInfo
    {
        public readonly int Id;
        public readonly string Title;
        public readonly TopicStatus TopicStatus;
        public readonly Forum Forum;
        public readonly IReadOnlyList<string> Tags;
        public readonly User? Author;
        public readonly long SizeInBytes;
        public readonly int SeedsCount;
        public readonly int LeechesCount;
        public readonly int DownloadsCount;
        public readonly DateTime CreatedAt;

        public SearchTopicInfo(int id, string title, TopicStatus topicStatus, Forum forum, IReadOnlyList<string> tags, User? author, long sizeInBytes, int seedsCount, int leechesCount, int downloadsCount, DateTime createdAt)
        {
            Id = id;
            Title = title;
            TopicStatus = topicStatus;
            Forum = forum;
            Tags = tags;
            Author = author;
            SizeInBytes = sizeInBytes;
            SeedsCount = seedsCount;
            LeechesCount = leechesCount;
            DownloadsCount = downloadsCount;
            CreatedAt = createdAt;
        }
    }
}