using System.Collections.Generic;

namespace RuTracker.Client.Model.SearchTopics.Request
{
    public sealed class SearchRequest
    {
        public readonly string Title;
        public readonly string Author;
        public readonly IReadOnlyList<int> Forums;
        public readonly SearchTopicsSortBy SortBy;
        public readonly SearchTopicsSortDirection SortDirection;

        public SearchRequest(
            string title,
            string author = "",
            IReadOnlyList<int>? categories = null,
            SearchTopicsSortBy sortBy = SearchTopicsSortBy.Registered,
            SearchTopicsSortDirection sortDirection = SearchTopicsSortDirection.Descending
        )
        {
            Title = title;
            Author = author;
            Forums = categories ?? new[] { -1 };
            SortBy = sortBy;
            SortDirection = sortDirection;
        }
    }
}
