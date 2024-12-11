using System.Diagnostics;
using SkiaSharp;

namespace AdvancedImageConverter;

internal static class ImageConverter
{
    private static readonly Dictionary<string, string[]> SupportedFormats = new()
    {
        { ".jpg", [".jpeg", ".png", ".bmp", ".webp", ".gif", ".tiff"] },
        { ".jpeg", [".jpg", ".png", ".bmp", ".webp", ".gif", ".tiff"] },
        { ".png", [".jpg", ".jpeg", ".bmp", ".webp", ".gif", ".tiff"] },
        { ".bmp", [".jpg", ".jpeg", ".png", ".webp", ".gif", ".tiff"] },
        { ".webp", [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff"] },
        { ".gif", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".tiff"] },
        { ".tiff", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".gif"] }
    };

    private static void Main()
    {
        Console.WriteLine("Multi-Format Image Converter");
        Console.WriteLine("----------------------------");

        while (true)
        {
            try
            {
                // File Selection
                var sourceFilePath = PromptForFilePath();
                if (string.IsNullOrEmpty(sourceFilePath)) break;

                // Target Format Selection
                var targetFormat = PromptForTargetFormat(Path.GetExtension(sourceFilePath));
                if (string.IsNullOrEmpty(targetFormat)) continue;

                // Conversion
                var outputFilePath = ConvertImage(sourceFilePath, targetFormat);

                // Confirmation and Open Option
                Console.WriteLine($"\nConversion completed. File saved at: {outputFilePath}");
                PromptToOpenFile(outputFilePath);

                // Continue or Exit
                if (!PromptToContinue()) break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static string PromptForFilePath()
    {
        while (true)
        {
            Console.Write("\nEnter the full path of the image to convert (or 'exit' to quit): ");
            var filePath = Console.ReadLine()?.Trim();

            if (string.Equals(filePath, "exit", StringComparison.OrdinalIgnoreCase))
                return null;

            if (File.Exists(filePath))
            {
                var extension = Path.GetExtension(filePath).ToLower();
                if (SupportedFormats.ContainsKey(extension) || extension == ".pdf")
                    return filePath;
                Console.WriteLine("Unsupported file format. Please try again.");
            }
            else
            {
                Console.WriteLine("File not found. Please check the path and try again.");
            }
        }
    }

    private static string PromptForTargetFormat(string currentFormat)
    {
        Console.WriteLine($"\nCurrent file format: {currentFormat}");

        if (SupportedFormats.TryGetValue(currentFormat, out var supportedFormats))
        {
            Console.WriteLine("Supported conversion formats:");
            for (var i = 0; i < supportedFormats.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {supportedFormats[i]}");
            }
        }
        else if (currentFormat == ".pdf")
        {
            Console.WriteLine("Supported conversion formats:");
            Console.WriteLine("1. .jpg\n2. .png\n3. .bmp");
        }
        else
        {
            Console.WriteLine("Limited conversion options available.");
            return null;
        }

        while (true)
        {
            Console.Write("\nEnter the number of the target format: ");
            if (int.TryParse(Console.ReadLine(), out var choice))
            {
                var availableFormats = SupportedFormats.TryGetValue(currentFormat, out var value)
                    ? value : [".jpg", ".png", ".bmp"];

                if (choice > 0 && choice <= availableFormats.Length)
                {
                    return availableFormats[choice - 1];
                }
            }

            Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    private static string ConvertImage(string sourcePath, string targetFormat)
    {
        var sourceExtension = Path.GetExtension(sourcePath).ToLower();
        var outputFileName = Path.GetFileNameWithoutExtension(sourcePath) + targetFormat;
        var outputDirectory = Path.Combine(Path.GetDirectoryName(sourcePath) ?? string.Empty, "Converted");

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Basic image format conversion using SkiaSharp
        using var inputStream = File.OpenRead(sourcePath);
        using var bitmap = SKBitmap.Decode(inputStream);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(GetSkiaFormat(targetFormat), 100);
        using var outputStream = File.OpenWrite(outputPath);
        data.SaveTo(outputStream);

        return outputPath;
    }

    private static SKEncodedImageFormat GetSkiaFormat(string extension)
    {
        return extension switch
        {
            ".png" => SKEncodedImageFormat.Png,
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".webp" => SKEncodedImageFormat.Webp,
            ".bmp" => SKEncodedImageFormat.Bmp,
            ".gif" => SKEncodedImageFormat.Gif,
            _ => SKEncodedImageFormat.Jpeg  // Default fallback
        };
    }

    private static void PromptToOpenFile(string filePath)
    {
        Console.Write("Do you want to open the converted file? (Y/N): ");
        if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) != true) return;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open file: {ex.Message}");
        }
    }

    private static bool PromptToContinue()
    {
        Console.Write("\nDo you want to convert another image? (Y/N): ");
        return Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
    }
}