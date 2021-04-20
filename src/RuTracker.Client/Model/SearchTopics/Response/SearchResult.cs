using System.Collections.Generic;
using RuTracker.Client.Model.SearchTopics.Request;

namespace RuTracker.Client.Model.SearchTopics.Response
{
    public record SearchResult(
        int Found,
        PaginatedSearchRequest? NextPage,
        IReadOnlyList<Forum> AllForums,
        IReadOnlyList<SearchTopicInfo> Topics
    );
}
