using System;
using System.Collections.Generic;
using System.Linq;

namespace RuTracker.Client.Model.GetTopicFileTree.Response
{
    public record TorrentDirectoryInfo(
        string Name,
        long Size,
        IReadOnlyList<TorrentDirectoryInfo> Directories,
        IReadOnlyList<TorrentFileInfo> Files
    ) {
        static string Indent(int depth) => new(' ', depth * 2);

        string ToStringHelper(int depth) =>
            $"{Indent(depth)}{Name} ({Size}){Environment.NewLine}" +
            string.Join(Environment.NewLine,
                Directories.OrderBy(x => x.Name).Select(x => x.ToStringHelper(depth + 1)).Concat(
                    Files.OrderBy(x => x.Name).Select(x => Indent(depth + 1) + x)
                )
            );

        public override string ToString() => ToStringHelper(0);
    }
}