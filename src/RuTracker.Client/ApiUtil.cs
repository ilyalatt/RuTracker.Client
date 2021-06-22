using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuTracker.Client {
    static class ApiUtil {
        static void AddUserAgentHeader(HttpRequestMessage req) {
            req.Headers.UserAgent.Clear();
            req.Headers.Add("User-Agent", $"RuTracker.Client/{VersionExtractor.Version}");
        }

        static void AddSessionHeader(HttpRequestMessage req, string session) {
            req.Headers.Add("Cookie", $"bb_session={session};");
        }

        public static HttpRequestMessage CreateGetReq(
            Uri baseUrl,
            string url,
            string? session
        ) {
            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl, url));
            AddUserAgentHeader(req);
            if (session != null) {
                AddSessionHeader(req, session);
            }

            return req;
        }

        public static HttpRequestMessage CreatePostReq(
            Uri baseUrl,
            string url,
            string? session,
            params (string, string)[] form
        ) {
            var content = new FormUrlEncodedContent(form.Select(
                x => new KeyValuePair<string, string>(x.Item1, x.Item2)
            ));
            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUrl, url)) {
                Content = content
            };
            AddUserAgentHeader(req);
            if (session != null) {
                AddSessionHeader(req, session);
            }

            return req;
        }

        static readonly Encoding CP1251 = CodePagesEncodingProvider.Instance.GetEncoding(1251);

        public static async Task<string> ReadResponseContent(HttpResponseMessage resp) {
            var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var content = CP1251.GetString(bytes);
            return content;
        }
    }
}