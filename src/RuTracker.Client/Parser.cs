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
using RuTracker.Client.Model.GetForumTopics.Response;
using RuTracker.Client.Model.GetTopic;
using RuTracker.Client.Model.GetTopic.Response;
using RuTracker.Client.Model.SearchTopics.Request;
using RuTracker.Client.Model.SearchTopics.Response;

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

        public static IReadOnlyList<Forum> ParseForums(string html)
        {
            var doc = HtmlParser.ParseDocument(html);
            EnsureAuthorized(doc);

            var forums = new List<Forum>();
            var recPath = new Stack<string>();

            static string? Text(IElement elm)
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
                    forums.Add(new Forum(id, path));
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

            return forums;
        }

        static readonly Dictionary<string, TopicStatus> StatusMapping = new Dictionary<string, TopicStatus>
        {
            { "√", TopicStatus.Checked },
            { "x", TopicStatus.Closed },
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

        static User? ParseUserLink(IHtmlAnchorElement? elm)
        {
            if (elm == null) return null;
            
            var title = elm.Text();
            var id = int.Parse(elm.Href.Split('=').Last());
            return id == -1 ? null : new User(id, title);
        }

        static DateTime ParseShortTimestamp(string s)
        {
            var createdDateStr = string.Join(" ", s
                .Split(new[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries)
            );
            var splCreatedDateStr = createdDateStr.Split('-');
            splCreatedDateStr[1] = MonthToNumMap[splCreatedDateStr[1]];
            createdDateStr = string.Join("-", splCreatedDateStr);
            var format = createdDateStr.Contains(' ') ? "d-MM-yy HH:mm" : "d-MM-yy";
            return DateTime.ParseExact(createdDateStr, format, CultureInfo.InvariantCulture);
        }

        public static SearchResult ParseSearchTopicsResponse(string html)
        {
            EnsureSessionIsNotStaled(html);
            var doc = HtmlParser.ParseDocument(html);

            var forums = ParseForums(html);
            var forumMap = forums.ToDictionary(x => x.Id);

            SearchTopicInfo ParseTopicBriefInfo(IElement elm)
            {
                var id = int.Parse(elm.Id.Split('-').Last());

                var statusIconText = elm.QuerySelector(".tor-icon").Text();
                var status = StatusMapping.TryGetValue(statusIconText, out var res) ? res : TopicStatus.Unknown;

                var forumUrl = ((IHtmlAnchorElement) elm.QuerySelector(".f-name a")).Href;
                var forumId = int.Parse(forumUrl.Split('=').Last());
                var forum = forumMap[forumId];

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

                var authorElm = (IHtmlAnchorElement?) elm.QuerySelector(".u-name a");
                var author = ParseUserLink(authorElm);

                var sizeElm = elm.QuerySelector(".tor-size");
                var sizeInBytes = long.Parse(sizeElm.GetAttribute("data-ts_text"));

                var seedCountElm = sizeElm.NextElementSibling;
                var seedCountValue = int.Parse(seedCountElm.GetAttribute("data-ts_text"));
                var seedsCount = Math.Max(0, seedCountValue); // TODO: add info about past (like -4 days)

                var leechCountElm = elm.QuerySelector(".leechmed");
                var leechesCount = int.Parse(leechCountElm.TextContent);

                var downloadsCountElm = leechCountElm.NextElementSibling;
                var downloadsCount = int.Parse(downloadsCountElm.TextContent);

                var createdDateElm = downloadsCountElm.NextElementSibling;
                var createdAt = ParseShortTimestamp(createdDateElm.TextContent);

                return new SearchTopicInfo(
                    id,
                    title,
                    status,
                    forum,
                    tags,
                    author,
                    sizeInBytes,
                    seedsCount,
                    leechesCount,
                    downloadsCount,
                    createdAt
                );
            }

            var foundRegex = new Regex(@"Результатов поиска: (\d+)");
            var found = int.Parse(foundRegex.Match(html).Groups[1].Value);
            
            var table = doc.QuerySelector("#search-results table tbody");
            var topics = found == 0 ? new List<SearchTopicInfo>() : table.QuerySelectorAll("tr").Select(ParseTopicBriefInfo).ToList();

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
                forums,
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

        // TODO: parse pinned forums
        public static GetForumTopicsResponse ParseForumTopicsResponse(string html)
        {
            EnsureSessionIsNotStaled(html);
            var doc = HtmlParser.ParseDocument(html);

            var tableElm = doc.QuerySelector("table.forum");
            if (tableElm == null) return new GetForumTopicsResponse(
                currentPage: 0,
                pagesCount: 0,
                topics: new ForumTopicInfo[0]
            );
            var rowElms = tableElm.QuerySelectorAll("tr").ToList();
            var topicSeparatorElmIndex = rowElms.FindLastIndex(x => x.FirstElementChild.ClassList.Contains("topicSep"));
            var topicRows = rowElms.Skip(topicSeparatorElmIndex + 1).Where(x => x.ClassList.Contains("hl-tr"));

            static ForumTopicInfo? ParseTopic(IElement elm)
            {
                var id = int.Parse(elm.Id.Split('-').Last());

                var titleSection = elm.QuerySelector("td.vf-col-t-title");
                var title = titleSection.QuerySelector("a").Text();
                var topicAuthorElm = (IHtmlAnchorElement?) titleSection.QuerySelector("a.topicAuthor");
                var author = ParseUserLink(topicAuthorElm);
                // status can be not set!
                var statusIconText = titleSection.QuerySelector(".tor-icon")?.Text();
                var topicStatus = statusIconText != null && StatusMapping.TryGetValue(statusIconText, out var res) ? res : TopicStatus.Unknown;

                var torrentSection = elm.QuerySelector("td.vf-col-tor");
                var seedsCountStr = torrentSection.QuerySelector("span.seedmed")?.Text();
                var seedsCount = seedsCountStr == null ? (int?) null : int.Parse(seedsCountStr);
                var leechesCountStr = torrentSection.QuerySelector("span.leechmed")?.Text();
                var leechesCount = leechesCountStr == null ? (int?) null : int.Parse(leechesCountStr);
                var size = torrentSection.QuerySelector("a.f-dl")?.Text();

                var repliesSection = elm.QuerySelector(".vf-col-replies");
                var repliesCountElm = repliesSection.FirstElementChild;
                var downloadsCountElm = repliesCountElm.NextElementSibling;
                var repliesCount = int.Parse(repliesCountElm.Text());
                var downloadsCount = downloadsCountElm == null ? (int?) null : int.Parse(downloadsCountElm.Text().Replace(",", ""));

                var lastPostSection = elm.QuerySelector(".vf-col-last-post");
                var lastMessageAtStr = lastPostSection.FirstElementChild.Text();
                var lastMessageAt = DateTime.ParseExact(lastMessageAtStr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                
                var lastMessageUser = ParseUserLink((IHtmlAnchorElement?) lastPostSection.QuerySelector("a[href^=profile]"));

                return new ForumTopicInfo(
                    id,
                    title,
                    topicStatus,
                    author,
                    size,
                    seedsCount,
                    leechesCount,
                    repliesCount,
                    downloadsCount,
                    lastMessageAt,
                    lastMessageUser
                );
            }

            var topics = topicRows.Select(ParseTopic).Where(x => x != null).Select(x => x!).ToList();

            var paginationElm = doc.QuerySelector("#pagination");
            var paginationLabelText = paginationElm.QuerySelector("p").Text();
            var paginationRegex = new Regex(@"Страница (?<currentPage>\d+) из (?<pagesCount>\d+)");
            var paginationMatch = paginationRegex.Match(paginationLabelText);
            var currentPage = int.Parse(paginationMatch.Groups["currentPage"].Value);
            var pagesCount = int.Parse(paginationMatch.Groups["pagesCount"].Value);

            return new GetForumTopicsResponse(
                currentPage: currentPage,
                pagesCount: pagesCount,
                topics: topics
            );
        }
    }
}
