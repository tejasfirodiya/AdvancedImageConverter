using System.Diagnostics;
using ImageMagick;

namespace AdvancedImageConverter;

internal static class AdvancedImageConverter
{
    // Comprehensive format mapping
    private static readonly Dictionary<string, string[]> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        // Raster Image Formats
        { ".jpg", [".jpeg", ".png", ".bmp", ".webp", ".gif", ".heic", ".tiff"] },
        { ".jpeg", [".jpg", ".png", ".bmp", ".webp", ".gif", ".heic", ".tiff"] },
        { ".png", [".jpg", ".jpeg", ".bmp", ".webp", ".gif", ".heic", ".tiff"] },
        { ".bmp", [".jpg", ".jpeg", ".png", ".webp", ".gif", ".heic", ".tiff"] },
        { ".webp", [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".heic", ".tiff"] },
        { ".gif", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".heic", ".tiff"] },
        { ".heic", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".gif", ".tiff"] },
        { ".tiff", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".gif", ".heic"] },

        // RAW Image Formats
        { ".cr2", [".jpg", ".png", ".tiff", ".dng"] },
        { ".nef", [".jpg", ".png", ".tiff", ".dng"] },
        { ".arw", [".jpg", ".png", ".tiff", ".dng"] },
        { ".dng", [".jpg", ".png", ".tiff", ".cr2", ".nef", ".arw"] },

        // Vector Formats
        { ".svg", [".png", ".jpg", ".pdf", ".eps", ".ai"] },
        { ".ai", [".svg", ".eps", ".png", ".jpg", ".pdf"] },
        { ".eps", [".svg", ".ai", ".png", ".jpg", ".pdf"] },

        // 3D and Specialized Formats
        { ".obj", [".stl", ".fbx"] },
        { ".stl", [".obj", ".fbx"] },
        { ".fbx", [".obj", ".stl"] },

        // Scientific and Medical Formats
        { ".fits", [".png", ".jpg", ".tiff"] },
        { ".dcm", [".png", ".jpg", ".tiff"] },

        // Additional Formats
        { ".dxf", [".png", ".jpg", ".pdf"] },
        { ".pcx", [".png", ".jpg", ".bmp"] },
        { ".xbm", [".png", ".jpg", ".bmp"] }
    };

    private static void Main()
    {
        Console.WriteLine("Advanced Multi-Format Image Converter");
        Console.WriteLine("-------------------------------------");

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
                var outputFilePath = ConvertFile(sourceFilePath, targetFormat);

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
            Console.Write("\nEnter the full path of the file to convert (or 'exit' to quit): ");
            var filePath = Console.ReadLine()?.Trim();

            if (string.Equals(filePath, "exit", StringComparison.OrdinalIgnoreCase))
                return null;

            if (File.Exists(filePath))
            {
                var extension = Path.GetExtension(filePath).ToLower();
                if (SupportedFormats.ContainsKey(extension))
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
        Console.WriteLine("Supported conversion formats:");

        // Display all supported formats, not just those for the current file type
        var allFormats = SupportedFormats.SelectMany(kv => kv.Value).Distinct().ToList();
        var i = 1;
        foreach (var format in allFormats)
        {
            Console.WriteLine($"{i++}. {format}");
        }

        while (true)
        {
            Console.Write("\nEnter the number of the target format: ");
            if (int.TryParse(Console.ReadLine(), out var choice))
            {
                if (choice > 0 && choice <= allFormats.Count)
                {
                    return allFormats[choice - 1];
                }
            }
            Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    private static string ConvertFile(string sourcePath, string targetFormat)
    {
        var sourceExtension = Path.GetExtension(sourcePath).ToLower();
        var outputFileName = Path.GetFileNameWithoutExtension(sourcePath) + targetFormat;
        var outputDirectory = Path.Combine(Path.GetDirectoryName(sourcePath) ?? string.Empty, "Converted");

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Use ImageMagick for comprehensive conversion
        using var image = new MagickImage(sourcePath);
        // Normalize image for consistent output
        image.AutoOrient();

        // Special handling for vector and 3D formats
        switch (sourceExtension)
        {
            case ".svg":
            case ".ai":
            case ".eps":
                // Rasterize vector graphics
                image.Density = new Density(300);
                image.BackgroundColor = MagickColors.Transparent;
                break;

            case ".obj":
            case ".stl":
            case ".fbx":
                // These require specialized 3D conversion libraries
                // For this example, we'll attempt a basic image representation
                Console.WriteLine("Warning: 3D format conversion is limited.");
                break;

            case ".fits":
            case ".dcm":
                // Scientific/Medical image formats may need specialized processing
                Console.WriteLine("Warning: Scientific image format conversion may lose metadata.");
                break;
        }

        // Write to target format
        image.Write(outputPath);

        return outputPath;
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
        Console.Write("\nDo you want to convert another file? (Y/N): ");
        return Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
    }
}

//using System.Diagnostics;
//using SkiaSharp;

//namespace AdvancedImageConverter;

//internal static class ImageConverter
//{
//    private static readonly Dictionary<string, string[]> SupportedFormats = new()
//    {
//        { ".jpg", [".jpeg", ".png", ".bmp", ".webp", ".gif", ".tiff"] },
//        { ".jpeg", [".jpg", ".png", ".bmp", ".webp", ".gif", ".tiff"] },
//        { ".png", [".jpg", ".jpeg", ".bmp", ".webp", ".gif", ".tiff"] },
//        { ".bmp", [".jpg", ".jpeg", ".png", ".webp", ".gif", ".tiff"] },
//        { ".webp", [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff"] },
//        { ".gif", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".tiff"] },
//        { ".tiff", [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".gif"] }
//    };

//    private static void Main()
//    {
//        Console.WriteLine("Multi-Format Image Converter");
//        Console.WriteLine("----------------------------");

//        while (true)
//        {
//            try
//            {
//                // File Selection
//                var sourceFilePath = PromptForFilePath();
//                if (string.IsNullOrEmpty(sourceFilePath)) break;

//                // Target Format Selection
//                var targetFormat = PromptForTargetFormat(Path.GetExtension(sourceFilePath));
//                if (string.IsNullOrEmpty(targetFormat)) continue;

//                // Conversion
//                var outputFilePath = ConvertImage(sourceFilePath, targetFormat);

//                // Confirmation and Open Option
//                Console.WriteLine($"\nConversion completed. File saved at: {outputFilePath}");
//                PromptToOpenFile(outputFilePath);

//                // Continue or Exit
//                if (!PromptToContinue()) break;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//            }
//        }
//    }

//    private static string PromptForFilePath()
//    {
//        while (true)
//        {
//            Console.Write("\nEnter the full path of the image to convert (or 'exit' to quit): ");
//            var filePath = Console.ReadLine()?.Trim();

//            if (string.Equals(filePath, "exit", StringComparison.OrdinalIgnoreCase))
//                return null;

//            if (File.Exists(filePath))
//            {
//                var extension = Path.GetExtension(filePath).ToLower();
//                if (SupportedFormats.ContainsKey(extension) || extension == ".pdf")
//                    return filePath;
//                Console.WriteLine("Unsupported file format. Please try again.");
//            }
//            else
//            {
//                Console.WriteLine("File not found. Please check the path and try again.");
//            }
//        }
//    }

//    private static string PromptForTargetFormat(string currentFormat)
//    {
//        Console.WriteLine($"\nCurrent file format: {currentFormat}");

//        if (SupportedFormats.TryGetValue(currentFormat, out var supportedFormats))
//        {
//            Console.WriteLine("Supported conversion formats:");
//            for (var i = 0; i < supportedFormats.Length; i++)
//            {
//                Console.WriteLine($"{i + 1}. {supportedFormats[i]}");
//            }
//        }
//        else if (currentFormat == ".pdf")
//        {
//            Console.WriteLine("Supported conversion formats:");
//            Console.WriteLine("1. .jpg\n2. .png\n3. .bmp");
//        }
//        else
//        {
//            Console.WriteLine("Limited conversion options available.");
//            return null;
//        }

//        while (true)
//        {
//            Console.Write("\nEnter the number of the target format: ");
//            if (int.TryParse(Console.ReadLine(), out var choice))
//            {
//                var availableFormats = SupportedFormats.TryGetValue(currentFormat, out var value)
//                    ? value : [".jpg", ".png", ".bmp"];

//                if (choice > 0 && choice <= availableFormats.Length)
//                {
//                    return availableFormats[choice - 1];
//                }
//            }

//            Console.WriteLine("Invalid selection. Please try again.");
//        }
//    }

//    private static string ConvertImage(string sourcePath, string targetFormat)
//    {
//        var sourceExtension = Path.GetExtension(sourcePath).ToLower();
//        var outputFileName = Path.GetFileNameWithoutExtension(sourcePath) + targetFormat;
//        var outputDirectory = Path.Combine(Path.GetDirectoryName(sourcePath) ?? string.Empty, "Converted");

//        // Create output directory if it doesn't exist
//        Directory.CreateDirectory(outputDirectory);

//        var outputPath = Path.Combine(outputDirectory, outputFileName);

//        // Basic image format conversion using SkiaSharp
//        using var inputStream = File.OpenRead(sourcePath);
//        using var bitmap = SKBitmap.Decode(inputStream);
//        using var image = SKImage.FromBitmap(bitmap);
//        using var data = image.Encode(GetSkiaFormat(targetFormat), 100);
//        using var outputStream = File.OpenWrite(outputPath);
//        data.SaveTo(outputStream);

//        return outputPath;
//    }

//    private static SKEncodedImageFormat GetSkiaFormat(string extension)
//    {
//        return extension switch
//        {
//            ".png" => SKEncodedImageFormat.Png,
//            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
//            ".webp" => SKEncodedImageFormat.Webp,
//            ".bmp" => SKEncodedImageFormat.Bmp,
//            ".gif" => SKEncodedImageFormat.Gif,
//            _ => SKEncodedImageFormat.Jpeg  // Default fallback
//        };
//    }

//    private static void PromptToOpenFile(string filePath)
//    {
//        Console.Write("Do you want to open the converted file? (Y/N): ");
//        if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) != true) return;
//        try
//        {
//            Process.Start(new ProcessStartInfo
//            {
//                FileName = filePath,
//                UseShellExecute = true
//            });
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Could not open file: {ex.Message}");
//        }
//    }

//    private static bool PromptToContinue()
//    {
//        Console.Write("\nDo you want to convert another image? (Y/N): ");
//        return Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
//    }
//}
