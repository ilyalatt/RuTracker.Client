using System;
using System.Net;
using System.Net.Http;

namespace RuTracker.Client {
    public record RuTrackerClientEnvironment(
        HttpClient HttpClient,
        Uri BaseUrl
    ) {
        public static RuTrackerClientEnvironment CreateDefault(
            string baseUrl = "https://rutracker.org/"
        ) {
            var baseUri = new Uri(baseUrl, UriKind.Absolute);
            var httpClientHandler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = false,
                AllowAutoRedirect = false
            };
            var httpClient = new HttpClient(httpClientHandler);
            return new RuTrackerClientEnvironment(
                HttpClient: httpClient,
                BaseUrl: baseUri
            );
        }
    }
}