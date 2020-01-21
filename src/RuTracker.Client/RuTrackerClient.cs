using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RuTracker.Client.Model;
using RuTracker.Client.Model.Exceptions;
using RuTracker.Client.Model.GetFileTree;
using RuTracker.Client.Model.GetTopic;
using RuTracker.Client.Model.Search.Request;
using RuTracker.Client.Model.Search.Response;

namespace RuTracker.Client
{
    public sealed class RuTrackerClient : IDisposable
    {
        readonly HttpClient _httpClient;
        readonly string _session;

        public RuTrackerClient(HttpClient httpClient, string session)
        {
            _httpClient = httpClient;
            _session = session;
        }
        public void Dispose() => _httpClient.Dispose();

        async Task<string> SearchImpl(SearchRequest req, CancellationToken ct = default)
        {
            var httpReq = ApiUtil.CreatePostReq(
                url: "/forum/tracker.php",
                session: _session,
                ("f", string.Join(",", req.Categories)),
                ("pn", req.Author),
                ("nm", req.Title),
                ("tm", "-1"),
                ("o", ((int) req.SortBy).ToString()),
                ("s", ((int) req.SortDirection).ToString())
            );
            var resp = await _httpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
            var html = await ApiUtil.ReadResponseContent(resp).ConfigureAwait(false);
            return html;
        }

        public async Task<IReadOnlyList<Category>> GetCategories(CancellationToken ct = default)
        {
            var req = new SearchRequest("");
            var html = await SearchImpl(req, ct).ConfigureAwait(false);
            return Parser.ParseCategories(html);
        }

        public async Task<SearchResult> Search(SearchRequest req, CancellationToken ct = default)
        {
            var html = await SearchImpl(req, ct).ConfigureAwait(false);
            return Parser.ParseSearchResult(html);
        }

        public async Task<SearchResult> Search(PaginatedSearchRequest req, CancellationToken ct = default)
        {
            var httpReq = ApiUtil.CreateGetReq(
                url: $"/forum/tracker.php?search_id={req.SearchId}&start={req.Offset}",
                session: _session
            );
            var resp = await _httpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
            var html = await ApiUtil.ReadResponseContent(resp).ConfigureAwait(false);
            return Parser.ParseSearchResult(html);
        }

        public async Task<Topic?> GetTorrentTopic(int topicId)
        {
            var httpReq = ApiUtil.CreateGetReq(
                url: $"/forum/viewtopic.php?t={topicId}",
                session: _session
            );
            var resp = await _httpClient.SendAsync(httpReq).ConfigureAwait(false);
            var html = await ApiUtil.ReadResponseContent(resp).ConfigureAwait(false);
            return Parser.ParseTorrentTopic(html);
        }

        public async Task<TorrentDirectoryInfo> GetFileTree(int topicId)
        {
            var httpReq = ApiUtil.CreatePostReq(
                url: $"/forum/viewtorrent.php",
                session: _session,
                ("t", topicId.ToString())
            );
            var resp = await _httpClient.SendAsync(httpReq).ConfigureAwait(false);
            var html = await ApiUtil.ReadResponseContent(resp).ConfigureAwait(false);
            return FileTreeParser.Parse(html);
        }

        public async Task<byte[]> GetTorrent(int topicId, CancellationToken ct = default)
        {
            var httpReq = ApiUtil.CreateGetReq(
                url: $"/forum/dl.php?t={topicId}",
                session: _session
            );
            var resp = await _httpClient.SendAsync(httpReq, ct).ConfigureAwait(false);
            var content = resp.Content;
            var contentType = content.Headers.ContentType.MediaType;
            const string torrentContentType = "application/x-bittorrent";
            if (contentType != torrentContentType)
            {
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
