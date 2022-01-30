namespace RuTracker.Client.Model.SearchTopics.Request {
    public record PaginatedSearchTopicsRequest(
        string SearchId,
        int Offset
    );
}