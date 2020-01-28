using System.Collections.Generic;

namespace RuTracker.Client.Model.GetForumTopics.Response
{
    public sealed class GetForumTopicsResponse
    {
        public readonly int CurrentPage;
        public readonly int PagesCount;
        public readonly IReadOnlyList<ForumTopicInfo> Topics;

        public GetForumTopicsResponse(int currentPage, int pagesCount, IReadOnlyList<ForumTopicInfo> topics)
        {
            CurrentPage = currentPage;
            PagesCount = pagesCount;
            Topics = topics;
        }
    }
}