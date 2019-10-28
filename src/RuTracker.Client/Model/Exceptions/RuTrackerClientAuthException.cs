namespace RuTracker.Client.Model.Exceptions
{
    public sealed class RuTrackerClientAuthException : RuTrackerClientException
    {
        public RuTrackerClientAuthException() : base("You need to login.") { }
    }
}
