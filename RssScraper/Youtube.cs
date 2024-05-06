using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace RssScraper
{
    public class Youtube
    {
        public string Url { get; set; }
        public string ChannelID { get; set; }
        public string ChannelURL { get; set; }
        public string RssUrl { get; set; }
        public Boolean Error { get; set; }
        public string ErrorReason { get; set; }

        public static Youtube GetYoutubeRSS(string url)
        {
            Youtube result = new();
            try
            {
                result.Url = url;

                bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri resultUrl);

                if (!isValidUrl)
                {
                    result.Error = true;
                    result.ErrorReason = "The URL is not valid.";
                    return result;
                }

                if (!ValidYoutubeURL(url))
                {
                    result.Error = true;
                    result.ErrorReason = "Invalid Youtube URL.";
                    return result;
                }

                //check to see if its a playlist
                var list = GetListValueFromUrl(url);

                if (list != null)
                {
                    result.ChannelURL = result.ChannelID = "Not found";
                    result.RssUrl = "https://www.youtube.com/feeds/videos.xml?playlist_id=" + list;
                }
                else
                {
                    var web = new HtmlWeb();
                    var doc = web.Load(url);

                    var link = doc.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
                    if (link != null)
                    {
                        result.ChannelURL = link.GetAttributeValue("href", "");
                        result.ChannelID = GetChannelIdFromUrl(result.ChannelURL);
                    }
                    else
                    {
                        result.ChannelURL = "Not found";
                    }

                    var rssLink = doc.DocumentNode.SelectSingleNode("//link[@rel='alternate' and @type='application/rss+xml' and @title='RSS']");
                    if (rssLink != null)
                    {
                        result.RssUrl = rssLink.GetAttributeValue("href", ""); ;
                    }
                    else
                    {
                        result.RssUrl = "Not found";
                    }
                }

                result.Error = false;
            }
            catch
            {
                result.Error = true;
            }

            return result;
        }

        static string GetChannelIdFromUrl(string url)
        {
            Regex regex = new Regex(@"(?:\/channel\/|\/user\/)([a-zA-Z0-9_-]{24})");
            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "Channel ID not found";
            }
        }

        static bool ValidYoutubeURL(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return false;
            }

            return uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);

        }

        public static string GetListValueFromUrl(string url)
        {
            string pattern = @"list=([A-Za-z0-9_-]+)";
            Regex regex = new Regex(pattern);

            Match match = regex.Match(url);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null; // or throw an exception, depending on your requirements
            }
        }
    }
}
