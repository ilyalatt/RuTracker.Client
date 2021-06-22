using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        readonly string _session;

        public RuTrackerClient(RuTrackerClientEnvironment env, string session) {
            _env = env;
            _session = session;
        }
        
        public void Dispose() {
            _env.HttpClient.Dispose();
        }

        public static async Task<RuTrackerClient> Login(
            string login,
            string password,
            RuTrackerClientEnvironment? env = default,
            CancellationToken ct = default
        ) {
            env ??= RuTrackerClientEnvironment.CreateDefault();
            var session = await RuTrackerAuthorizer.Login(env, login, password, ct);
            return session == null
                ? throw new RuTrackerClientAuthException("Invalid login/password.")
                : new RuTrackerClient(env, session);
        }

        async Task<string> Get(CancellationToken ct, string url) {
            var httpReq = ApiUtil.CreateGetReq(
                baseUrl: _env.BaseUrl,
                url: url,
                session: _session
            );
            var resp = await _env.HttpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
            return await ApiUtil.ReadResponseContent(resp).ConfigureAwait(false);
        }

        async Task<string> Post(CancellationToken ct, string url, params (string, string)[] form) {
            var httpReq = ApiUtil.CreatePostReq(
                baseUrl: _env.BaseUrl,
                url: url,
                session: _session,
                form
            );
            var resp = await _env.HttpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
            return await ApiUtil.ReadResponseContent(resp).ConfigureAwait(false);
        }

        async Task<string> SearchImpl(SearchTopicsRequest req, CancellationToken ct = default) {
            return await Post(
                ct,
                "/forum/tracker.php",
                ("f", string.Join(",", req.Forums ?? new[] { -1 })),
                ("pn", req.Author),
                ("nm", req.Title),
                ("tm", "-1"),
                ("o", ((int) req.SortBy).ToString()),
                ("s", ((int) req.SortDirection).ToString())
            );
        }

        public async Task<IReadOnlyList<Forum>> GetForums(CancellationToken ct = default) {
            var req = new SearchTopicsRequest("");
            var html = await SearchImpl(req, ct).ConfigureAwait(false);
            return Parser.ParseForums(html);
        }

        public async Task<SearchResult> SearchTopics(SearchTopicsRequest req, CancellationToken ct = default) {
            var html = await SearchImpl(req, ct).ConfigureAwait(false);
            return Parser.ParseSearchTopicsResponse(html);
        }

        public async Task<SearchResult> SearchTopics(PaginatedSearchRequest req, CancellationToken ct = default) {
            var html = await Get(ct, $"/forum/tracker.php?search_id={req.SearchId}&start={req.Offset}");
            return Parser.ParseSearchTopicsResponse(html);
        }

        public async Task<Topic?> GetTorrentTopic(int topicId, CancellationToken ct = default) {
            var html = await Get(ct, $"/forum/viewtopic.php?t={topicId}");
            return Parser.ParseTorrentTopic(html);
        }

        public async Task<TorrentDirectoryInfo> GetTopicFileTree(int topicId, CancellationToken ct = default) {
            var html = await Post(
                ct,
                $"/forum/viewtorrent.php",
                ("t", topicId.ToString())
            );
            return FileTreeParser.Parse(html);
        }

        public async Task<GetForumTopicsResponse> GetForumTopics(GetForumTopicsRequest req, CancellationToken ct = default) {
            var queryParams = new[] {
                ("f", req.ForumId),
                ("sort", (int) req.SortBy),
                ("order", (int) req.SortDirection),
                ("start", (req.Page - 1) * 50) // can use multiples of 50 only
            };
            var queryStr = string.Join("&", queryParams.Where(x => x.Item2 != 0).Select(x => $"{x.Item1}={x.Item2}"));
            var httpReq = ApiUtil.CreateGetReq(
                baseUrl: _env.BaseUrl,
                url: $"/forum/viewforum.php?{queryStr}",
                session: _session
            );
            var resp = await _env.HttpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.Redirect) {
                throw new Exception("Looks like there is an unexpected redirect in method GetForumTopics.");
            }

            var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return Parser.ParseForumTopicsResponse(html);
        }

        public async Task<byte[]> GetTopicTorrent(int topicId, CancellationToken ct = default) {
            var httpReq = ApiUtil.CreateGetReq(
                baseUrl: _env.BaseUrl,
                url: $"/forum/dl.php?t={topicId}",
                session: _session
            );
            var resp = await _env.HttpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
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
    }
}