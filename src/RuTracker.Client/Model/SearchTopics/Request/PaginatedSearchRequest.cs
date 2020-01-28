namespace RuTracker.Client.Model.SearchTopics.Request
{
    public sealed class PaginatedSearchRequest
    {
        public readonly string SearchId;
        public readonly int Offset;

        public PaginatedSearchRequest(string searchId, int offset)
        {
            SearchId = searchId;
            Offset = offset;
        }
    }
}
