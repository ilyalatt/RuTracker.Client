using System.Collections.Generic;

namespace RuTracker.Client.Model.Search.Request
{
    public sealed class SearchRequest
    {
        public readonly string Title;
        public readonly string Author;
        public readonly IReadOnlyList<int> Categories;
        public readonly SortBy SortBy;
        public readonly SortDirection SortDirection;

        public SearchRequest(
            string title,
            string author = "",
            IReadOnlyList<int>? categories = null,
            SortBy sortBy = SortBy.Registered,
            SortDirection sortDirection = SortDirection.Descending
        )
        {
            Title = title;
            Author = author;
            Categories = categories ?? new[] { -1 };
            SortBy = sortBy;
            SortDirection = sortDirection;
        }
    }
}
