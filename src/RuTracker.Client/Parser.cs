using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using RuTracker.Client.Model;
using RuTracker.Client.Model.Exceptions;
using RuTracker.Client.Model.GetTopic;
using RuTracker.Client.Model.Search.Request;
using RuTracker.Client.Model.Search.Response;

namespace RuTracker.Client
{
    static class Parser
    {
        static readonly HtmlParser HtmlParser = new HtmlParser();

        static void EnsureAuthorized(IHtmlDocument doc)
        {
            if (doc.QuerySelector("#logged-in-username") == null)
            {
                throw new RuTrackerClientAuthException();
            }
        }

        static void EnsureSessionIsNotStaled(string html)
        {
            if (html.Contains("Сессия устарела"))
            {
                throw new RuTrackerStaleSessionException();
            }
        }

        public static IReadOnlyList<Category> ParseCategories(string html)
        {
            var doc = HtmlParser.ParseDocument(html);
            EnsureAuthorized(doc);

            var categories = new List<Category>();
            var recPath = new Stack<string>();

            string? Text(IElement elm)
            {
                var text = (elm as IHtmlOptionsGroupElement)?.Label.Trim();
                text ??= (elm as IHtmlOptionElement)?.Label.Trim();
                const string marker = "|- ";
                text = text != null && text.StartsWith(marker) ? text.Substring(marker.Length) : text;
                return text;
            }

            void Traverse(IElement elm)
            {
                var text = Text(elm);
                var shouldPutTextInRecPath = text != null;
                if (shouldPutTextInRecPath) recPath.Push(text!);

                if (elm.NodeName == "OPTION")
                {
                    var id = int.Parse(elm.GetAttribute("value"));
                    var path = recPath.Reverse().ToList();
                    categories.Add(new Category(id, path));
                }

                var isOptGroupAdded = false;
                foreach (var x in elm.Children)
                {
                    var isGroup = x.ClassList.Contains("root_forum");
                    if (isGroup && isOptGroupAdded) recPath.Pop();
                    Traverse(x);
                    if (isGroup)
                    {
                        recPath.Push(Text(x)!);
                        isOptGroupAdded = true;
                    }
                }
                if (isOptGroupAdded) recPath.Pop();

                if (shouldPutTextInRecPath) recPath.Pop();
            }
            Traverse(doc.QuerySelector("#fs-main"));

            return categories;
        }

        static readonly Dictionary<string, TopicStatus> StatusMapping = new Dictionary<string, TopicStatus>
        {
            { "√", TopicStatus.Checked },
            { "∑", TopicStatus.Consumed },
            { "D", TopicStatus.Duplicate },
            { "#", TopicStatus.Suspicious },
            { "T", TopicStatus.Temporary },
            { "*", TopicStatus.Unchecked }
        };

        static readonly Dictionary<string, string> MonthToNumMap = new Dictionary<string, string>
        {
            { "Янв", "01" },
            { "Фев", "02" },
            { "Мар", "03" },
            { "Апр", "04" },
            { "Май", "05" },
            { "Июн", "06" },
            { "Июл", "07" },
            { "Авг", "08" },
            { "Сен", "09" },
            { "Окт", "10" },
            { "Ноя", "11" },
            { "Дек", "12" },
        };

        public static SearchResult ParseSearchResult(string html)
        {
            EnsureSessionIsNotStaled(html);
            var doc = HtmlParser.ParseDocument(html);

            var categories = ParseCategories(html);
            var categoryMap = categories.ToDictionary(x => x.Id);

            TopicBriefInfo ParseTopicBriefInfo(IElement elm)
            {
                var id = int.Parse(elm.Id.Split('-').Last());

                var statusIconText = elm.QuerySelector(".tor-icon").Text();
                var status = StatusMapping.TryGetValue(statusIconText, out var res) ? res : TopicStatus.Unknown;

                var categoryUrl = ((IHtmlAnchorElement) elm.QuerySelector(".f-name a")).Href;
                var categoryId = int.Parse(categoryUrl.Split('=').Last());
                var category = categoryMap[categoryId];

                var titleElm = elm.QuerySelector(".t-title");
                var title = titleElm.QuerySelector("a").Text().Trim();
                var tagsRegex = new Regex(@"^(?<tags>\(.+?\))|^(?<tags>\[.+?\])");
                var tags = new List<string>();
                // TODO: optimize it to O(N) instead of O(N*N)
                while (true)
                {
                    var tagsMatch = tagsRegex.Match(title);
                    if (!tagsMatch.Success) break;

                    var tagsStr = tagsMatch.Groups["tags"].Value;
                    tags.AddRange(tagsStr.Substring(1, tagsStr.Length - 2).Split(',').Select(x => x.Trim()));
                    title = title.Substring(tagsStr.Length).Trim();
                }

                var authorElm = (IHtmlAnchorElement) elm.QuerySelector(".u-name a");
                var authorTitle = authorElm.Text();
                var authorId = int.Parse(authorElm.Href.Split('=').Last());
                var author = new User(authorId, authorTitle);

                var sizeElm = elm.QuerySelector(".tor-size");
                var sizeInBytes = long.Parse(sizeElm.GetAttribute("data-ts_text"));

                var seedCountElm = sizeElm.NextElementSibling;
                var seedCountValue = int.Parse(seedCountElm.GetAttribute("data-ts_text"));
                var seedsCount = Math.Max(0, seedCountValue); // TODO: add info about past (like -4 days)

                var leechCountElm = elm.QuerySelector(".leechmed");
                var leechsCount = int.Parse(leechCountElm.TextContent);

                var downloadsCountElm = leechCountElm.NextElementSibling;
                var downloadsCount = int.Parse(downloadsCountElm.TextContent);

                var createdDateElm = downloadsCountElm.NextElementSibling;
                var createdDateStr = string.Join(" ", createdDateElm.TextContent
                    .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                );
                var splCreatedDateStr = createdDateStr.Split('-');
                splCreatedDateStr[1] = MonthToNumMap[splCreatedDateStr[1]];
                createdDateStr = string.Join("-", splCreatedDateStr);
                var format = createdDateStr.Contains(' ') ? "d-MM-yy hh:mm" : "d-MM-yy";
                var createdAt = DateTime.ParseExact(createdDateStr, format, CultureInfo.InvariantCulture);

                return new TopicBriefInfo(
                    id,
                    title,
                    status,
                    category,
                    tags,
                    author,
                    sizeInBytes,
                    seedsCount,
                    leechsCount,
                    downloadsCount,
                    createdAt
                );
            }

            var foundRegex = new Regex(@"Результатов поиска: (\d+)");
            var found = int.Parse(foundRegex.Match(html).Groups[1].Value);
            
            var table = doc.QuerySelector("#search-results table tbody");
            var topics = found == 0 ? new List<TopicBriefInfo>() : table.QuerySelectorAll("tr").Select(ParseTopicBriefInfo).ToList();

            PaginatedSearchRequest? GetNextPage()
            {
                var nextPageElm = (IHtmlAnchorElement) doc.QuerySelectorAll(".bottom_info a.pg").FirstOrDefault(x => x.Text().StartsWith("След"));
                if (nextPageElm == null) return null;
                var nextPageUrl = nextPageElm.Href;
                var query = nextPageUrl.Substring(nextPageUrl.IndexOf('?') + 1);
                var queryDict = query.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
                var searchId = queryDict["search_id"];
                var offset = int.Parse(queryDict["start"]);
                return new PaginatedSearchRequest(searchId, offset);
            }
            var nextPage = GetNextPage();

            return new SearchResult(
                found,
                nextPage,
                categories,
                topics
            );
        }

        public static Topic? ParseTorrentTopic(string html)
        {
            EnsureSessionIsNotStaled(html);
            var doc = HtmlParser.ParseDocument(html);

            var postHtml = doc.QuerySelector(".post_body")?.InnerHtml;
            if (postHtml == null) return null;
            
            var magnetLinkElm = (IHtmlAnchorElement?) doc.QuerySelector("a[href^=\"magnet\"]");
            var magnetLink = magnetLinkElm?.Href;
            if (magnetLink == null) return null;
            
            return new Topic(
                postHtml,
                magnetLink
            );
        }
    }
}
