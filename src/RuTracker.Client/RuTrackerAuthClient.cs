using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuTracker.Client
{
    public sealed class RuTrackerAuthClient : IDisposable
    {
        readonly HttpClient _httpClient;

        public RuTrackerAuthClient(HttpClient httpClient) => _httpClient = httpClient;
        public RuTrackerAuthClient()
        {
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = false,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(httpClientHandler)
            {
                DefaultRequestHeaders = { { "User-Agent", "RuTracker.Client/0.1.1" } }
            };
        }
        public void Dispose() => _httpClient.Dispose();

        // TODO: handle captcha
        public async Task<string?> Login(
            string login,
            string password,
            CancellationToken ct = default
        )
        {
            var req = ApiUtil.CreatePostReq(
                url: "/forum/login.php",
                session: null,
                ("login_username", login),
                ("login_password", password),
                ("login", "вход")
            );
            var resp = await _httpClient.SendAsync(req, ct).ConfigureAwait(false);
            var cookies = new CookieContainer();
            var uri = new Uri(ApiUtil.BaseUri, "/forum/");
            var cookieHeaders = resp.Headers.TryGetValues("Set-Cookie", out var res) ? res : new string[0];
            foreach (var x in cookieHeaders) cookies.SetCookies(uri, x);
            var sessionCookie = cookies.GetCookies(uri)["bb_session"];
            return sessionCookie?.Value;
        }

        public RuTrackerClient WithSession(string session) =>
            new RuTrackerClient(_httpClient, session);
    }
}
