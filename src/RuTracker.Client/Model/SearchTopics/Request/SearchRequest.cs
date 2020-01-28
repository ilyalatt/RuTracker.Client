using System.Collections.Generic;

namespace RuTracker.Client.Model.SearchTopics.Request
{
    public sealed class SearchRequest
    {
        public readonly string Title;
        public readonly string Author;
        public readonly IReadOnlyList<int> Forums;
        public readonly SearchTopicSortBy SearchTopicSortBy;
        public readonly SearchTopicSortDirection SearchTopicSortDirection;

        public SearchRequest(
            string title,
            string author = "",
            IReadOnlyList<int>? categories = null,
            SearchTopicSortBy searchTopicSortBy = SearchTopicSortBy.Registered,
            SearchTopicSortDirection searchTopicSortDirection = SearchTopicSortDirection.Descending
        )
        {
            Title = title;
            Author = author;
            Forums = categories ?? new[] { -1 };
            SearchTopicSortBy = searchTopicSortBy;
            SearchTopicSortDirection = searchTopicSortDirection;
        }
    }
}
