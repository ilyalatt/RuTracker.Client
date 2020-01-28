namespace RuTracker.Client.Model.GetTopicFileTree.Response
{
    public sealed class TorrentFileInfo
    {
        public readonly string Name;
        public readonly long Size;

        public TorrentFileInfo(string name, long size)
        {
            Name = name;
            Size = size;
        }

        public override string ToString() => $"{Name} ({Size})";
    }
}