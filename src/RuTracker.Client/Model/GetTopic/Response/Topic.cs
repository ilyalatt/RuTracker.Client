namespace RuTracker.Client.Model.GetTopic.Response
{
    public sealed class Topic
    {
        public readonly string PostHtml;
        public readonly string MagnetLink;

        public Topic(string postHtml, string magnetLink)
        {
            PostHtml = postHtml;
            MagnetLink = magnetLink;
        }
    }
}