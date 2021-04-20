using System.Collections.Generic;

namespace RuTracker.Client.Model.GetForumTopics.Response
{
    public record GetForumTopicsResponse(
        int CurrentPage,
        int PagesCount,
        IReadOnlyList<ForumTopicInfo> Topics
    );
}