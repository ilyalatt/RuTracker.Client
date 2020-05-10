using System;

namespace RuTracker.Client.Model.GetForumTopics.Response
{
    public sealed class ForumTopicInfo
    {
        public readonly int Id;
        public readonly string Title;
        public readonly TopicStatus TopicStatus;
        public readonly User? Author;
        public readonly ApproximateSize? Size;
        public readonly int? SeedsCount;
        public readonly int? LeechesCount;
        public readonly int RepliesCount;
        public readonly int? DownloadsCount;
        public readonly DateTime LastMessageAt;
        public readonly User? LastMessageUser;

        public ForumTopicInfo(int id, string title, TopicStatus topicStatus, User? author, ApproximateSize? size, int? seedsCount, int? leechesCount, int repliesCount, int? downloadsCount, DateTime lastMessageAt, User? lastMessageUser)
        {
            Id = id;
            Title = title;
            TopicStatus = topicStatus;
            Author = author;
            Size = size;
            SeedsCount = seedsCount;
            LeechesCount = leechesCount;
            RepliesCount = repliesCount;
            DownloadsCount = downloadsCount;
            LastMessageAt = lastMessageAt;
            LastMessageUser = lastMessageUser;
        }
    }
}