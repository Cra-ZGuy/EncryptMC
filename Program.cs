using EncryptMC.Utility;

namespace EncryptMC
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Parse arguments or prompt for them interactively
            var (inputPath, outputPath, contentKey, isInteractiveMode) = CommandLineHandler.ParseArgs(args);

            // Validate user input
            if (!InputValidator.ValidatePathsAndKey(inputPath, outputPath, contentKey, isInteractiveMode))
            {
                return;
            }

            // Retrieve UUID from manifest
            string manifestUuid;
            try
            {
                manifestUuid = ManifestUtility.GetManifestUuid(inputPath);
                Console.WriteLine($"Manifest UUID: {manifestUuid}\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to get manifest UUID: {ex.Message}\n");
                ExitIfInteractive(isInteractiveMode);
                return;
            }

            bool inputOutputPathsEqual = inputPath.Equals(outputPath, StringComparison.OrdinalIgnoreCase);

            // Remove existing output directory (if any) to rebuild cleanly
            if (Directory.Exists(outputPath) && !inputOutputPathsEqual)
            {
                try
                {
                    Directory.Delete(outputPath, recursive: true);
                    Console.WriteLine($"Successfully removed the directory: {outputPath}\n");
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to remove the directory \"{outputPath}\": {ex.Message}", ex);
                }
            }

            // Create and encrypt manifest signature
            try
            {
                ManifestUtility.CreateManifestSignature(inputPath, outputPath, contentKey);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to create manifest signature: {ex.Message}\n");
                ExitIfInteractive(isInteractiveMode);
                return;
            }

            // Encrypt entire pack
            try
            {
                PackEncryption.EncryptPack(inputPath, outputPath, contentKey, manifestUuid);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to encrypt pack: {ex.Message}\n");
                ExitIfInteractive(isInteractiveMode);
                return;
            }

            // Success message
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Encryption completed successfully.\n");
            ExitIfInteractive(isInteractiveMode);
            Console.ResetColor();
        }

        private static void ExitIfInteractive(bool isInteractiveMode)
        {
            if (isInteractiveMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Press any key to exit...");
                Console.ReadKey(true);
                Console.ResetColor();
            }
        }
    }
}
