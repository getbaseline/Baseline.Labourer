namespace Baseline.Labourer.Internal.Extensions
{
    /// <summary>
    /// A set of extensions related to strings.
    /// </summary>
    internal static class StringExtensions
    {
        public static string AsNormalizedScheduledJobId(this string nameOrId)
        {
            var normalizedName = nameOrId
                .ToLower()
                .Replace(ResourceKeyPrefixes.ScheduledJob, string.Empty)
                .Replace(" ", "-");
            
            return $"{ResourceKeyPrefixes.ScheduledJob}{normalizedName}";
        }
    }
}