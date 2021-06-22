using System;
using System.Collections.Generic;

namespace RuTracker.Client.Model.SearchTopics.Response {
    public record SearchTopicInfo(
        int Id,
        string Title,
        TopicStatus TopicStatus,
        Forum Forum,
        IReadOnlyList<string> Tags,
        User? Author,
        long SizeInBytes,
        int SeedsCount,
        int LeechesCount,
        int DownloadsCount,
        DateTime CreatedAt
    );
}