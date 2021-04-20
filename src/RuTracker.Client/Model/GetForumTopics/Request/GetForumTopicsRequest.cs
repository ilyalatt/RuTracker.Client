namespace RuTracker.Client.Model.GetForumTopics.Request
{
    public record GetForumTopicsRequest(
        int ForumId,
        GetForumTopicsSortBy SortBy = GetForumTopicsSortBy.LastMessage,
        GetForumTopicsSortDirection SortDirection = GetForumTopicsSortDirection.Descending,
        int Page = 1
    );
}