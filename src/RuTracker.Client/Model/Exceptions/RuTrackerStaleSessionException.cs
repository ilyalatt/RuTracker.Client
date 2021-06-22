namespace RuTracker.Client.Model.Exceptions {
    public sealed class RuTrackerStaleSessionException : RuTrackerClientException {
        public RuTrackerStaleSessionException() : base("Session is staled. You need to make a new search.") { }
    }
}