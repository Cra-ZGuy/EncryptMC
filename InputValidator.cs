using EncryptMC.Utility;

namespace EncryptMC
{
    internal static class InputValidator
    {
        internal static bool ValidatePathsAndKey(string inputPath, string outputPath, string contentKey, bool isInteractive)
        {
            // Check input path
            if (!Directory.Exists(inputPath))
            {
                ShowError("Input path does not exist or is not a directory.\n", isInteractive);
                return false;
            }

            // Check output path
            if (!Directory.Exists(outputPath))
            {
                ShowError("Output path does not exist or is not a directory.\n", isInteractive);
                return false;
            }

            // Check if input path is a subpath of output path
            if (PathUtility.IsSubPath(inputPath, outputPath))
            {
                ShowError("Input path cannot be a subpath of output path.\n", isInteractive);
                return false;
            }

            // Check if content key is 32 bytes
            if (contentKey.Length != 32)
            {
                ShowError("Content key must be exactly 32 bytes long.\n", isInteractive);
                return false;
            }

            return true;
        }

        private static void ShowError(string message, bool isInteractive)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

            if (isInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Press Enter to exit...");
                Console.ReadKey(true);
                Console.ResetColor();
            }
        }
    }
}
