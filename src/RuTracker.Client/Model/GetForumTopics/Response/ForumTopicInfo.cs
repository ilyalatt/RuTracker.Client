using System;

namespace RuTracker.Client.Model.GetForumTopics.Response
{
    public record ForumTopicInfo(
        int Id,
        string Title,
        TopicStatus TopicStatus,
        User? Author,
        ApproximateSize? Size,
        int? SeedsCount,
        int? LeechesCount,
        int RepliesCount,
        int? DownloadsCount,
        DateTime LastMessageAt,
        User? LastMessageUser
    );
}