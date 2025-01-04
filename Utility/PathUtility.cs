namespace EncryptMC.Utility
{
    internal static class PathUtility
    {
        internal static void EnsureOutputFolderExists(string outputPath)
        {
            try
            {
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to ensure directory exists: {ex.Message}", ex);
            }
        }

        internal static bool IsSubPath(string inputPath, string outputPath)
        {
            // Normalize paths to remove relative elements and ensure consistency
            string? normalizedInput = Path.GetFullPath(inputPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string? normalizedOutput = Path.GetFullPath(outputPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            bool pathsEqual = string.Equals(normalizedInput, normalizedOutput, StringComparison.OrdinalIgnoreCase);

            // Check if output starts with input
            return normalizedOutput.StartsWith(normalizedInput, StringComparison.OrdinalIgnoreCase) && !pathsEqual;
        }
    }
}
