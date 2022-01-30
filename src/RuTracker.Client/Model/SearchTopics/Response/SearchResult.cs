using System.Collections.Generic;
using RuTracker.Client.Model.SearchTopics.Request;

namespace RuTracker.Client.Model.SearchTopics.Response {
    public record SearchResult(
        int Found,
        PaginatedSearchTopicsRequest? NextPage,
        IReadOnlyList<Forum> AllForums,
        IReadOnlyList<SearchTopicInfo> Topics
    );
}