using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EncryptMC.Utility
{
    internal static class ManifestUtility
    {
        internal static string GetManifestUuid(string inputPath)
        {
            string manifestPath = Path.Combine(inputPath, "manifest.json");

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Manifest file not found.", manifestPath);
            }

            string manifestContents = File.ReadAllText(manifestPath);

            // Parse the JSON content
            JObject manifest;
            try
            {
                manifest = JObject.Parse(manifestContents);
            }
            catch (JsonReaderException ex)
            {
                throw new FormatException($"Invalid JSON formatting in manifest file: {ex.Message}", ex);
            }

            // Get "header" object
            var header = manifest["header"];
            if (header == null)
            {
                throw new KeyNotFoundException("Manifest header not found.");
            }

            // Get "uuid" property
            var uuid = header["uuid"]?.ToString();
            if (string.IsNullOrEmpty(uuid))
            {
                throw new KeyNotFoundException("Manifest UUID not found.");
            }

            return uuid;
        }

        internal static void CreateManifestSignature(string inputPath, string outputPath, string contentKey)
        {
            string manifestPath = Path.Combine(inputPath, "manifest.json");
            string signaturePath = Path.Combine(outputPath, "signatures.json");

            // Check if manifest file exists
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Manifest file not found.", manifestPath);
            }

            // Read manifest file contents
            string manifestContents = File.ReadAllText(manifestPath);

            // Calculate SHA-256 hash of manifest
            string manifestHashBase64 = CalculateSha256Hash(manifestContents);

            // Prepare signatures contents
            var signaturesContents = new List<Dictionary<string, string>>
            {
                new()
                {
                    { "hash", manifestHashBase64 },
                    { "path", "manifest.json" }
                }
            };

            // Convert signatures contents to JSON string
            string contentsString = JsonConvert.SerializeObject(signaturesContents, Formatting.None);
            byte[] contentsBytes = Encoding.UTF8.GetBytes(contentsString);

            // Encrypt and write to file
            EncryptionUtility.WriteDataToEncryptedFile(signaturePath, contentsBytes, contentKey);
        }

        private static string CalculateSha256Hash(string plaintext)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
