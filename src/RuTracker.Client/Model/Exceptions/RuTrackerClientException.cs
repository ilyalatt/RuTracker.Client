using System;

namespace RuTracker.Client.Model.Exceptions {
    public class RuTrackerClientException : Exception {
        public RuTrackerClientException(string message) : base(message) { }
    }
}