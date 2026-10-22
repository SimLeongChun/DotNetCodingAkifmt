using BlazorAppFluentUINet10Charts.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BlazorAppFluentUINet10Charts.Services;

class Link
{
    public string Next { get; set; }
    public string Last { get; set; }

    public int NextPage { get; set; }
    public int LastPage { get; set; }

    private static int ExtractPage(string value)
    {
        var match = Regex.Match(value, "&page=(?<page>\\d+)");

        if (match != null)
        {
            return Convert.ToInt32(match.Groups["page"].Value);
        }

        return 0;
    }

    public static Link FromHeader(IEnumerable<string> header)
    {
        var result = new Link();

        var links = String.Join("", header).Split(',');

        foreach (var link in links)
        {
            var rel = Regex.Match(link, "(?<=rel=\").+?(?=\")", RegexOptions.IgnoreCase);
            var value = Regex.Match(link, "(?<=<).+?(?=>)", RegexOptions.IgnoreCase);

            if (rel.Success && value.Success)
            {
                if (rel.Value == "next")
                {
                    result.Next = value.Value;
                    result.NextPage = ExtractPage(result.Next);
                }
                if (rel.Value == "last")
                {
                    result.Last = value.Value;
                    result.LastPage = ExtractPage(result.Last);
                }
            }
        }

        return result;
    }
}

class IssueCache
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("issues")]
    public IEnumerable<Issue> Issues { get; set; }
}

public class FetchProgressEventArgs
{
    public int Total { get; set; }
    public int Current { get; set; }
}

public class GitHubService
{
    private readonly JsonSerializerOptions options;

    private IEnumerable<Issue> issues;

    public GitHubService()
    {
        options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public async Task<IEnumerable<Issue>> GetIssues(DateTime date)
    {
        var target = date.AddMonths(-1);

        if (issues == null)
        {
            issues = await FetchIssues(target);
            issues = issues.Where(issue => issue.CreatedAt >= target).OrderBy(issue => issue.CreatedAt);
        }

        return issues;
    }

    private async Task<IEnumerable<Issue>> FetchIssues(DateTime since)
    {
        var issues = new List<Issue>();

        using (var http = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/dotnet/aspnetcore/issues?state=all&per_page=100&since={since:yyyy-MM-ddThh:mm:ssZ}");
            request.Headers.Add("User-Agent", "testapplication");

            var response = await http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var page = await JsonSerializer.DeserializeAsync<IEnumerable<Issue>>(responseStream, options);
                issues.AddRange(page);
                var link = Link.FromHeader(response.Headers.GetValues("Link"));

                while (link.Next != null)
                {
                    request = new HttpRequestMessage(HttpMethod.Get, link.Next);
                    request.Headers.Add("User-Agent", "testapplication");

                    response = await http.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            page = await JsonSerializer.DeserializeAsync<IEnumerable<Issue>>(stream, options);
                            issues.AddRange(page);
                        }

                        link = Link.FromHeader(response.Headers.GetValues("Link"));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new ApplicationException(response.ReasonPhrase);
            }
        }

        return issues;
    }

}
