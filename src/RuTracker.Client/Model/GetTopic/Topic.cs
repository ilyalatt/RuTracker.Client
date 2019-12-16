namespace RuTracker.Client.Model.GetTopic
{
    public sealed class Topic
    {
        public readonly string MagnetLink;

        public Topic(string magnetLink)
        {
            MagnetLink = magnetLink;
        }
    }
}