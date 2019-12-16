using System.Collections.Generic;
using RuTracker.Client.Model.Search.Request;

namespace RuTracker.Client.Model.Search.Response
{
    public sealed class SearchResult
    {
        public readonly int Found;
        public readonly PaginatedSearchRequest? NextPage;
        public readonly IReadOnlyList<Category> AllCategories;
        public readonly IReadOnlyList<TopicBriefInfo> Topics;

        public SearchResult(int found, PaginatedSearchRequest? nextPage, IReadOnlyList<Category> allCategories, IReadOnlyList<TopicBriefInfo> topics)
        {
            Found = found;
            NextPage = nextPage;
            AllCategories = allCategories;
            Topics = topics;
        }
    }
}
