using System.Collections.Generic;

namespace RuTracker.Client.Model.SearchTopics.Request {
    public record SearchTopicsRequest(
        string Title,
        string Author = "",
        IReadOnlyList<int>? Forums = null,
        SearchTopicsSortBy SortBy = SearchTopicsSortBy.Registered,
        SearchTopicsSortDirection SortDirection = SearchTopicsSortDirection.Descending
    );
}