namespace Baseline.Labourer.Internal
{
    /// <summary>
    /// A set of extensions related to strings.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Convert a string containing a scheduled job's name or id into a normalized scheduled job key.
        /// </summary>
        /// <param name="nameOrId">The name or id of the scheduled job.</param>
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