namespace RuTracker.Client.Model.SearchTopics.Request
{
    public record PaginatedSearchRequest(
        string SearchId,
        int Offset
    );
}
