using EncryptMC.Utility;
using Newtonsoft.Json;
using System.Text;

namespace EncryptMC
{
    internal static class PackEncryption
    {
        // Paths that should not be encrypted
        private static readonly HashSet<string> DoNotEncrypt =
        [
            "contents.json",
            "manifest.json",
            "pack_icon.png",
            "texts"
        ];

        internal static void EncryptPack(string inputPath, string outputPath, string contentKey, string uuid)
        {
            string contentsPath = Path.Combine(outputPath, "contents.json");
            List<Dictionary<string, string>> contentsList = [];

            // Parallel traversal and encryption
            ParallelEncrypt(inputPath, outputPath, contentsList);

            // Prepare contents.json file
            var contentsFile = new Dictionary<string, object>
            {
                { "version", 1 },
                { "content", contentsList }
            };

            // Serialize contents to JSON
            string jsonString = JsonConvert.SerializeObject(contentsFile, Formatting.None);

            // Encrypt JSON
            byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);
            byte[] encryptedDataForContents = EncryptionUtility.EncryptData(contentKey, jsonData);

            // Write result to contents.json with special header
            using (FileStream fileStream = new(contentsPath, FileMode.Create, FileAccess.Write))
            {
                // Write header (4 + 4 + 8 = 16 bytes)
                fileStream.Write(BitConverter.GetBytes(0), 0, 4);          // Zero bytes (4 bytes)
                fileStream.Write(BitConverter.GetBytes(0x9BCFB9FC), 0, 4); // Magic number (4 bytes)
                fileStream.Write(BitConverter.GetBytes(0L), 0, 8);         // Padding (8 bytes)

                // Write UUID length and contents (1 byte for length + actual UUID)
                byte[] uuidBytes = Encoding.UTF8.GetBytes(uuid);
                fileStream.WriteByte((byte) uuidBytes.Length);
                fileStream.Write(uuidBytes, 0, uuidBytes.Length);

                // Add padding to reach required length (0xEF - length of UUID)
                int paddingLength = 0xEF - uuidBytes.Length;
                fileStream.Write(new byte[paddingLength], 0, paddingLength);

                // Write encrypted data
                fileStream.Write(encryptedDataForContents, 0, encryptedDataForContents.Length);
            }
        }

        private static void ParallelEncrypt(string inputPath, string outputPath, List<Dictionary<string, string>> contents)
        {
            bool inputOutputPathsEqual = inputPath.Equals(outputPath, StringComparison.OrdinalIgnoreCase);

            // Create the base directory (just in case)
            PathUtility.EnsureOutputFolderExists(outputPath);

            // Create all subdirectories (sequentially)
            string[]? allDirectories = Directory.GetDirectories(inputPath, "*", SearchOption.AllDirectories);
            foreach (string dir in allDirectories)
            {
                string relativeFilePath = Path.GetRelativePath(inputPath, dir).Replace("\\", "/");
                string outputDir = Path.Combine(outputPath, relativeFilePath);

                Directory.CreateDirectory(outputDir);

                // Record this directory in final contents list
                lock (contents)
                {
                    contents.Add(new Dictionary<string, string>
                    {
                        { "path", relativeFilePath + "/" }
                    });
                }
            }

            // Grab all files
            string[]? allFiles = Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories);

            // Process all files in parallel
            Parallel.ForEach(allFiles, file =>
            {
                string relativeFilePath = Path.GetRelativePath(inputPath, file).Replace("\\", "/");
                string outputFilePath = Path.Combine(outputPath, relativeFilePath);
                bool shouldEncryptFile = ShouldEncrypt(relativeFilePath, DoNotEncrypt);

                if (shouldEncryptFile)
                {
                    string fileContentKey = EncryptionUtility.GenerateContentKey();

                    // Encrypt file contents
                    byte[] fileContents = File.ReadAllBytes(file);
                    EncryptionUtility.WriteDataToEncryptedFile(outputFilePath, fileContents, fileContentKey);

                    // Add file reference with encryption key
                    lock (contents)
                    {
                        contents.Add(new Dictionary<string, string>
                        {
                            { "key", fileContentKey },
                            { "path", relativeFilePath }
                        });
                    }
                }
                else
                {
                    if (!inputOutputPathsEqual)
                    {
                        // Copy file without encryption
                        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
                        File.Copy(file, outputFilePath, overwrite: true);
                    }

                    // Add file reference without encryption key
                    lock (contents)
                    {
                        contents.Add(new Dictionary<string, string>
                        {
                            { "path", relativeFilePath }
                        });
                    }
                }
            });
        }

        private static bool ShouldEncrypt(string relativePath, HashSet<string> doNotEncrypt)
        {
            // Normalize path to forward slashes
            string normalizedPath = relativePath.Replace("\\", "/").TrimStart('/');

            // Check if path is in DoNotEncrypt set or subdirectory
            foreach (string excludePath in doNotEncrypt)
            {
                string? cleanedExclude = excludePath.Replace("\\", "/").TrimStart('/');
                if (normalizedPath.StartsWith(cleanedExclude, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
