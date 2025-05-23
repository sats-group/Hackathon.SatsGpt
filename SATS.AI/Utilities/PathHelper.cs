using System.Text.RegularExpressions;

namespace SATS.AI.Utilities;

public static class PathHelper
{
    /// <summary>
    /// Converts a file-like path (using '/' or '\\') into an LTree path (dot-separated).
    /// </summary>
    public static string ToLTree(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var segments = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);

        var sanitized = segments
            .SelectMany(segment => 
                segment
                    .Split(new[] { '-', '.' }, StringSplitOptions.RemoveEmptyEntries) // optional
                    .Select(s => Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9_]", "_")));

        return string.Join('.', sanitized);
    }

    /// <summary>
    /// Converts an LTree path (dot-separated) into a file-like path using '/'.
    /// </summary>
    public static string FromLTree(string ltreePath)
    {
        if (string.IsNullOrWhiteSpace(ltreePath))
            return string.Empty;

        var segments = ltreePath.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join('/', segments);
    }
}
