namespace RuTracker.Client.Model.GetForumTopics.Request
{
    public sealed class GetForumTopicsRequest
    {
        public readonly int ForumId;
        public readonly GetForumTopicsSortBy SortBy;
        public readonly GetForumTopicsSortDirection SortDirection;
        public readonly int Page;

        public GetForumTopicsRequest(
            int forumId,
            GetForumTopicsSortBy sortBy = GetForumTopicsSortBy.LastMessage,
            GetForumTopicsSortDirection sortDirection = GetForumTopicsSortDirection.Descending,
            int page = 1
        ) {
            ForumId = forumId;
            SortBy = sortBy;
            SortDirection = sortDirection;
            Page = page;
        }

        public GetForumTopicsRequest WithPage(int page) => new GetForumTopicsRequest(
            ForumId,
            SortBy,
            SortDirection,
            page
        );
    }
}