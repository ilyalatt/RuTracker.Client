using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuTracker.Client
{
    static class ApiUtil
    {
        public static Uri BaseUri = new("https://rutracker.org/");

        static void AddSession(HttpRequestMessage req, string session)
        {
            req.Headers.Add("Cookie", $"bb_session={session};");
        }

        public static HttpRequestMessage CreateGetReq(
            string url,
            string? session
        )
        {
            var fullUri = new Uri(BaseUri, url);
            var req = new HttpRequestMessage(HttpMethod.Get, fullUri);
            if (session != null) AddSession(req, session);
            return req;
        }

        public static HttpRequestMessage CreatePostReq(
            string url,
            string? session,
            params (string, string)[] form
        )
        {
            var fullUri = new Uri(BaseUri, url);
            var content = new FormUrlEncodedContent(form.Select(
                x => new KeyValuePair<string, string>(x.Item1, x.Item2)
            ));
            var req = new HttpRequestMessage(HttpMethod.Post, fullUri)
            {
                Content = content
            };
            if (session != null) AddSession(req, session);
            return req;
        }

        static readonly Encoding CP1251 = CodePagesEncodingProvider.Instance.GetEncoding(1251);

        public static async Task<string> ReadResponseContent(HttpResponseMessage resp)
        {
            var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var content = CP1251.GetString(bytes);
            return content;
        }
    }
}
