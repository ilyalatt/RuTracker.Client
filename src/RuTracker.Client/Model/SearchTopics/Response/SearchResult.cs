using System.Collections.Generic;
using RuTracker.Client.Model.SearchTopics.Request;

namespace RuTracker.Client.Model.SearchTopics.Response
{
    public sealed class SearchResult
    {
        public readonly int Found;
        public readonly PaginatedSearchRequest? NextPage;
        public readonly IReadOnlyList<Forum> AllForums;
        public readonly IReadOnlyList<SearchTopicInfo> Topics;

        public SearchResult(int found, PaginatedSearchRequest? nextPage, IReadOnlyList<Forum> allForums, IReadOnlyList<SearchTopicInfo> topics)
        {
            Found = found;
            NextPage = nextPage;
            AllForums = allForums;
            Topics = topics;
        }
    }
}
