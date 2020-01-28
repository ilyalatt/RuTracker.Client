using System;
using System.Collections.Generic;
using System.Linq;

namespace RuTracker.Client.Model.GetTopicFileTree.Response
{
    public sealed class TorrentDirectoryInfo
    {
        public readonly string Name;
        public readonly long Size;
        public readonly IReadOnlyList<TorrentDirectoryInfo> Directories;
        public readonly IReadOnlyList<TorrentFileInfo> Files;

        public TorrentDirectoryInfo(string name, long size, IReadOnlyList<TorrentDirectoryInfo> directories, IReadOnlyList<TorrentFileInfo> files)
        {
            Name = name;
            Size = size;
            Directories = directories;
            Files = files;
        }

        static string Indent(int depth) => new string(' ', depth * 2);

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