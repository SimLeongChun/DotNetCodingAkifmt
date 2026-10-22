using System.Text.Json.Serialization;

namespace BlazorAppFluentUINet10Charts.Models;

public class Issue
{
    [JsonPropertyName("html_url")]
    public string Url { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("labels")]
    public IEnumerable<Label> Labels { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("assignees")]
    public IEnumerable<User> Assignees { get; set; }

    [JsonPropertyName("state")]
    public IssueState State { get; set; }
}

public enum IssueState
{
    Open,
    Closed,
    All
}
public class Label
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }
}

public class User
{
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; }
}

