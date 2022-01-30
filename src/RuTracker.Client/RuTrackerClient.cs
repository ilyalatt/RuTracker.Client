using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuTracker.Client.Model;
using RuTracker.Client.Model.Exceptions;
using RuTracker.Client.Model.GetForumTopics.Request;
using RuTracker.Client.Model.GetForumTopics.Response;
using RuTracker.Client.Model.GetTopic.Response;
using RuTracker.Client.Model.GetTopicFileTree.Response;
using RuTracker.Client.Model.SearchTopics.Request;
using RuTracker.Client.Model.SearchTopics.Response;

namespace RuTracker.Client {
    public sealed class RuTrackerClient : IDisposable {
        readonly RuTrackerClientEnvironment _env;

        public RuTrackerClient(RuTrackerClientEnvironment? env = null) {
            _env = env ?? RuTrackerClientEnvironment.CreateDefault();
        }
        
        public void Dispose() {
            _env.Dispose();
        }

        public async Task Login(
            string login,
            string password,
            CancellationToken ct = default
        ) {
            var html = await _env.Post(
                ct,
                "/forum/login.php",
                ("login_username", login),
                ("login_password", password),
                ("login", "вход")
            ).ConfigureAwait(false);
            Parser.CheckAuthorization(html);
        }

        async Task<string> SearchImpl(SearchTopicsRequest req, CancellationToken ct = default) {
            return await _env.Post(
                ct,
                "/forum/tracker.php",
                ("f", string.Join(",", req.Forums ?? new[] { -1 })),
                ("pn", req.Author),
                ("nm", req.Title),
                ("tm", "-1"),
                ("o", ((int) req.SortBy).ToString()),
                ("s", ((int) req.SortDirection).ToString())
            ).ConfigureAwait(false);
        }

        public async Task<SearchResult> SearchTopics(SearchTopicsRequest req, CancellationToken ct = default) {
            var html = await SearchImpl(req, ct).ConfigureAwait(false);
            return Parser.ParseSearchTopicsResponse(html);
        }

        public async Task<SearchResult> SearchTopics(PaginatedSearchTopicsRequest req, CancellationToken ct = default) {
            var html = await _env.Get(ct, $"/forum/tracker.php?search_id={req.SearchId}&start={req.Offset}").ConfigureAwait(false);
            return Parser.ParseSearchTopicsResponse(html);
        }

        public async Task<Topic?> GetTopic(int topicId, CancellationToken ct = default) {
            var html = await _env.Get(ct, $"/forum/viewtopic.php?t={topicId}").ConfigureAwait(false);
            return Parser.ParseTorrentTopic(html);
        }

        public async Task<TorrentDirectoryInfo> GetTopicFileTree(int topicId, CancellationToken ct = default) {
            var html = await _env.Post(
                ct,
                $"/forum/viewtorrent.php",
                ("t", topicId.ToString())
            ).ConfigureAwait(false);
            return FileTreeParser.Parse(html);
        }
        
        public async Task<byte[]> GetTopicTorrentFile(int topicId, CancellationToken ct = default) {
            var resp = await _env.GetRaw(ct, $"/forum/dl.php?t={topicId}");
            var content = resp.Content;
            var contentType = content.Headers.ContentType.MediaType;
            const string torrentContentType = "application/x-bittorrent";
            if (contentType != torrentContentType) {
                throw new RuTrackerClientException($"Expected Content-Type is '{torrentContentType}' but got '{contentType}'.");
            }

            using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            var ms = new MemoryStream();
            const int defaultBufferSize = 81920;
            await stream.CopyToAsync(ms, defaultBufferSize, ct).ConfigureAwait(false);
            return ms.ToArray();
        }

        public async Task<IReadOnlyList<Forum>> GetForums(CancellationToken ct = default) {
            var req = new SearchTopicsRequest("");
            var html = await SearchImpl(req, ct).ConfigureAwait(false);
            return Parser.ParseForums(html);
        }

        public async Task<GetForumTopicsResponse> GetForumTopics(GetForumTopicsRequest req, CancellationToken ct = default) {
            var queryParams = new[] {
                ("f", req.ForumId),
                ("sort", (int) req.SortBy),
                ("order", (int) req.SortDirection),
                ("start", (req.Page - 1) * 50) // can use multiples of 50 only
            };
            var queryStr = string.Join("&", queryParams.Where(x => x.Item2 != 0).Select(x => $"{x.Item1}={x.Item2}"));
            var html = await _env.Get(
                ct,
                url: $"/forum/viewforum.php?{queryStr}"
            ).ConfigureAwait(false);
            return Parser.ParseForumTopicsResponse(html);
        }
    }
}