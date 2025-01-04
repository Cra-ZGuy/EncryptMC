using System.Diagnostics;

namespace EncryptMC
{
    internal static class CommandLineHandler
    {
        internal static (string inputPath, string outputPath, string contentKey, bool isInteractiveMode) ParseArgs(string[] args)
        {
            int argCount = args.Length;
            bool isInteractive = false;

            string inputPath;
            string outputPath;
            string contentKey;

            switch (argCount)
            {
                case 0: // Interactive mode
                    isInteractive = true;
                    Console.Clear();
                    Console.WriteLine("===== EncryptMC =====\n");

                    Console.Write("Enter the input path: ");
                    string? temp = Console.ReadLine();
                    if (string.IsNullOrEmpty(temp))
                    {
                        ShowError("Invalid input path.");
                        return (string.Empty, string.Empty, string.Empty, true);
                    }
                    inputPath = Path.GetFullPath(temp);
                    Console.WriteLine($"\nInput Path: {inputPath}\n");

                    Console.Write("Enter the output path: ");
                    temp = Console.ReadLine();
                    if (string.IsNullOrEmpty(temp))
                    {
                        ShowError("Invalid output path.");
                        return (string.Empty, string.Empty, string.Empty, true);
                    }
                    outputPath = Path.GetFullPath(temp);
                    Console.WriteLine($"\nOutput Path: {outputPath}\n");

                    Console.Write("Enter the content key (must be 32 bytes): ");
                    temp = Console.ReadLine();
                    if (string.IsNullOrEmpty(temp))
                    {
                        ShowError("Invalid content key.");
                        return (string.Empty, string.Empty, string.Empty, true);
                    }
                    contentKey = temp;
                    Console.WriteLine($"\nContent Key: {contentKey}\n");
                    break;

                case 3: // Command line mode
                    inputPath = Path.GetFullPath(args[0]);
                    outputPath = Path.GetFullPath(args[1]);
                    contentKey = args[2];

                    Console.WriteLine($"\nInput Path: {inputPath}\n");
                    Console.WriteLine($"\nOutput Path: {outputPath}\n");
                    Console.WriteLine($"\nContent Key: {contentKey}\n");
                    break;

                default: // Invalid arguments
                    ShowUsage();
                    return (string.Empty, string.Empty, string.Empty, false);
            }

            return (inputPath, outputPath, contentKey, isInteractive);
        }

        private static void ShowUsage()
        {
            string exeName = Process.GetCurrentProcess().ProcessName;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nUsage: ./{exeName}.exe <inputPath> <outputPath> <contentKey>");
            Console.ResetColor();
        }

        private static void ShowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{error}");
            Console.ResetColor();
        }
    }
}
