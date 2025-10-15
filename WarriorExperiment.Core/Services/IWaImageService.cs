namespace WarriorExperiment.Core.Services;

/// <summary>
/// Interface for image processing service
/// </summary>
public interface IWaImageService
{
    /// <summary>
    /// Resizes an image to 3 megapixels and converts to Base64 string
    /// </summary>
    /// <param name="imageStream">The input image stream</param>
    /// <returns>Base64 encoded JPEG image string</returns>
    /// <exception cref="InvalidOperationException">Thrown when image processing fails</exception>
    Task<string> ResizeAndConvertToBase64Async(Stream imageStream);
    
    /// <summary>
    /// Validates if a file is a supported image format
    /// </summary>
    /// <param name="fileName">The file name to validate</param>
    /// <returns>True if the file is a supported image format</returns>
    bool IsSupportedImageFormat(string fileName);
    
    /// <summary>
    /// Gets the maximum allowed file size in bytes (before processing)
    /// </summary>
    long MaxFileSizeBytes { get; }
}