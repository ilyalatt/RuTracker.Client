using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RuTracker.Client {
    public static class RuTrackerAuthorizer {
        // TODO: handle captcha
        public static async Task<string?> Login(
            RuTrackerClientEnvironment env,
            string login,
            string password,
            CancellationToken ct = default
        ) {
            var req = ApiUtil.CreatePostReq(
                baseUrl: env.BaseUrl,
                url: "/forum/login.php",
                session: null,
                ("login_username", login),
                ("login_password", password),
                ("login", "вход")
            );
            var resp = await env.HttpClient.SendAsync(req, ct).ConfigureAwait(false);
            var cookies = new CookieContainer();
            var uri = new Uri(env.BaseUrl, "/forum/");
            var cookieHeaders = resp.Headers.TryGetValues("Set-Cookie", out var res) ? res : new string[0];
            foreach (var x in cookieHeaders) {
                cookies.SetCookies(uri, x);
            }

            var sessionCookie = cookies.GetCookies(uri)["bb_session"];
            return sessionCookie?.Value;
        }
    }
}