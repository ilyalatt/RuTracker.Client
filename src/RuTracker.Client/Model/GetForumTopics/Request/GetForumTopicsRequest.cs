namespace RuTracker.Client.Model.GetForumTopics.Request
{
    public sealed class GetForumTopicsRequest
    {
        public readonly int ForumId;
        public readonly GetForumTopicsSortBy GetForumTopicsSortBy;
        public readonly GetForumsTopicsSortDirection GetForumsTopicsSortDirection;
        public readonly int Page;

        public GetForumTopicsRequest(
            int forumId,
            GetForumTopicsSortBy getForumTopicsSortBy = GetForumTopicsSortBy.LastMessage,
            GetForumsTopicsSortDirection getForumsTopicsSortDirection = GetForumsTopicsSortDirection.Descending,
            int page = 1
        ) {
            ForumId = forumId;
            GetForumTopicsSortBy = getForumTopicsSortBy;
            GetForumsTopicsSortDirection = getForumsTopicsSortDirection;
            Page = page;
        }

        public GetForumTopicsRequest WithPage(int page) => new GetForumTopicsRequest(
            ForumId,
            GetForumTopicsSortBy,
            GetForumsTopicsSortDirection,
            page
        );
    }
}