using System.Collections.Generic;

namespace RuTracker.Client.Model
{
    public sealed class Forum
    {
        public readonly int Id;
        public readonly IReadOnlyList<string> Path;

        public Forum(int id, IReadOnlyList<string> path)
        {
            Id = id;
            Path = path;
        }

        public override string ToString() => string.Join(" - ", Path);
    }
}
