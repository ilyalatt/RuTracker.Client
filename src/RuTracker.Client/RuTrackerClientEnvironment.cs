using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuTracker.Client {
    public sealed record RuTrackerClientEnvironment(
        HttpClient HttpClient,
        Uri BaseUrl
    ) : IDisposable {
        public void Dispose() => HttpClient.Dispose();
        
        public static readonly Uri DefaultBaseUrl = new("https://rutracker.org");

        public static RuTrackerClientEnvironment CreateDefault(Uri? baseUrl = null) {
            var httpClientHandler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.GZip
            };
            var httpClient = new HttpClient(httpClientHandler) {
                DefaultRequestHeaders = {
                    { "User-Agent", $"RuTracker.Client/{VersionExtractor.Version}" }
                }
            };
            return new RuTrackerClientEnvironment(
                HttpClient: httpClient,
                BaseUrl: baseUrl ?? DefaultBaseUrl
            );
        }

        static readonly Encoding CP1251 = CodePagesEncodingProvider.Instance.GetEncoding(1251);
        
        static async Task<string> ReadResponseContent(HttpResponseMessage resp) {
            var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var content = CP1251.GetString(bytes);
            return content;
        }

        internal async Task<HttpResponseMessage> GetRaw(CancellationToken ct, string url) {
            return await HttpClient.GetAsync(new Uri(BaseUrl, url), ct).ConfigureAwait(false);
        }

        internal async Task<string> Get(CancellationToken ct, string url) {
            var resp = await GetRaw(ct, url).ConfigureAwait(false);
            return await ReadResponseContent(resp).ConfigureAwait(false);
        }
        
        internal async Task<HttpResponseMessage> PostRaw(
            CancellationToken ct,
            string url,
            params (string, string)[] form
        ) {
            var content = new FormUrlEncodedContent(form.Select(
                x => new KeyValuePair<string, string>(x.Item1, x.Item2)
            ));
            return await HttpClient.PostAsync(new Uri(BaseUrl, url), content, ct).ConfigureAwait(false);
        }
        
        internal async Task<string> Post(
            CancellationToken ct,
            string url,
            params (string, string)[] form
        ) {
            var resp = await PostRaw(ct, url, form).ConfigureAwait(false);
            return await ReadResponseContent(resp).ConfigureAwait(false);
        }
    }
}