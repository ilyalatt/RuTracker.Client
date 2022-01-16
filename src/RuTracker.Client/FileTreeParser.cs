using System;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using RuTracker.Client.Model.Exceptions;
using RuTracker.Client.Model.GetTopicFileTree.Response;

namespace RuTracker.Client {
    static class FileTreeParser {
        static (string, long?) ParseInfo(IParentNode elm) {
            var name = elm.FirstElementChild!.TextContent;
            var sizeStr = elm.LastElementChild!.TextContent;
            var size = sizeStr.Length == 0 ? (long?) null : long.Parse(sizeStr);
            return (name, size);
        }

        static TorrentFileInfo ParseFile(IElement elm) {
            var (name, size) = ParseInfo(elm.FirstElementChild!);
            return new TorrentFileInfo(name, size!.Value);
        }

        static bool IsFile(IElement elm) => elm.ClassName == null;

        static bool IsDir(IElement elm) => elm.ClassName == "dir";

        static TorrentDirectoryInfo ParseDir(IElement elm) {
            var (name, _) = ParseInfo(elm.FirstElementChild!);
            var children = elm.LastElementChild!.Children;
            var dirs = children.Where(IsDir).Select(ParseDir).ToList();
            var files = children.Where(IsFile).Select(ParseFile).ToList();
            return new TorrentDirectoryInfo(
                Name: name,
                Size: dirs.Sum(x => x.Size) + files.Sum(x => x.Size),
                Directories: dirs,
                Files: files
            );
        }

        static TorrentDirectoryInfo ParseRoot(IElement elm) {
            var firstChild = elm.FirstElementChild!;
            if (IsDir(firstChild)) {
                return ParseDir(firstChild);
            }

            var file = ParseFile(firstChild);
            return new TorrentDirectoryInfo(
                Name: "./",
                Size: file.Size,
                Directories: Array.Empty<TorrentDirectoryInfo>(),
                Files: new[] { file }
            );
        }

        static readonly HtmlParser HtmlParser = new();

        public static TorrentDirectoryInfo Parse(string html) {
            if (html == "not logged in") {
                throw new RuTrackerClientAuthException("The client is not authorized.");
            }

            var doc = HtmlParser.ParseDocument(html);
            var treeRootElement = doc.QuerySelector(".ftree");
            return ParseRoot(treeRootElement!);
        }
    }
}