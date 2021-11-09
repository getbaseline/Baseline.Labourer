namespace Baseline.Labourer.Internal.Utils;

/// <summary>
/// Contains numerous utilities related to the generation of strings.
/// </summary>
public class StringGenerationUtils
{
    /// <summary>
    /// Generates a random string pretty much guaranteed to be unique.
    /// </summary>
    public static string GenerateUniqueRandomString()
    {
        return Guid.NewGuid().ToString().Replace("-", "");
    }
}
