using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for processing images for measurement entries
/// </summary>
public class WaImageService : IWaImageService
{
    private const int TargetMegapixels = 3;
    private const int JpegQuality = 85;
    private readonly string[] _supportedFormats = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };
    
    /// <summary>
    /// Maximum file size in bytes (20MB)
    /// </summary>
    public long MaxFileSizeBytes => 20 * 1024 * 1024;
    
    /// <summary>
    /// Resizes an image to approximately 3 megapixels while maintaining aspect ratio
    /// </summary>
    /// <param name="imageStream">The input image stream</param>
    /// <returns>Base64 encoded JPEG image string</returns>
    public async Task<string> ResizeAndConvertToBase64Async(Stream imageStream)
    {
        try
        {
            using var image = await Image.LoadAsync(imageStream);
            
            // Calculate target dimensions for 3MP
            var currentPixels = image.Width * image.Height;
            var targetPixels = TargetMegapixels * 1_000_000;
            
            // Only resize if image is larger than 3MP
            if (currentPixels > targetPixels)
            {
                var scaleFactor = Math.Sqrt((double)targetPixels / currentPixels);
                var targetWidth = (int)(image.Width * scaleFactor);
                var targetHeight = (int)(image.Height * scaleFactor);
                
                // Resize the image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                }));
            }
            
            // Convert to JPEG and encode to Base64
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = JpegQuality });
            
            var bytes = outputStream.ToArray();
            return Convert.ToBase64String(bytes);
        }
        catch (UnknownImageFormatException)
        {
            throw new InvalidOperationException("The provided file is not a valid image format");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to process image: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Validates if a file is a supported image format based on its extension
    /// </summary>
    /// <param name="fileName">The file name to validate</param>
    /// <returns>True if the file is a supported image format</returns>
    public bool IsSupportedImageFormat(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _supportedFormats.Contains(extension);
    }
}