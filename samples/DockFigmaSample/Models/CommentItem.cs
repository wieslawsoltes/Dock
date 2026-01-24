using System;

namespace DockFigmaSample.Models;

public class CommentItem
{
    public CommentItem(string author, string body, string time)
    {
        Author = author;
        Body = body;
        Time = time;
        Initials = GetInitials(author);
    }

    public string Author { get; }
    public string Body { get; }
    public string Time { get; }
    public string Initials { get; }

    private static string GetInitials(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return "?";
        }

        var parts = author.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0].Substring(0, 1).ToUpperInvariant();
        }

        var first = parts[0].Substring(0, 1).ToUpperInvariant();
        var last = parts[^1].Substring(0, 1).ToUpperInvariant();
        return first + last;
    }
}
