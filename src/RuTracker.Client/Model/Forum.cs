using System.Collections.Generic;

namespace RuTracker.Client.Model
{
    public record Forum(
        int Id,
        IReadOnlyList<string> Path
    ) {
        public override string ToString() => string.Join(" - ", Path);
    }
}
