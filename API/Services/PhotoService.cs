using System.Net;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<PhotoService> _logger;
    private readonly CloudinarySettings _cloudinarySettings;

    public PhotoService(IOptions<CloudinarySettings> config, ILogger<PhotoService> logger)
    {
        _cloudinarySettings = config.Value;
        var acc = new Account(
            _cloudinarySettings.CloudName,
            _cloudinarySettings.ApiKey,
            _cloudinarySettings.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
        _logger = logger;
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("The file is empty", nameof(file));
        }
        try
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Height(500)
                    .Width(500)
                    .Crop("fill")
                    .Gravity("face"),
                Folder = _cloudinarySettings.RootFolder,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation(
                    "Photo uploaded successfully: {FileName}, URL: {Url}",
                    file.FileName,
                    result.Url
                );
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while uploading photo to cloudinary");
            throw new InvalidOperationException("Error happened while uploading the phoot", ex);
        }
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            throw new ArgumentException("Public ID is required", nameof(publicId));
        }
        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation(
                    "Photo with public ID {PublicId} deleted successfully.",
                    publicId
                );
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting photo from Cloudinary");
            throw new Exception("An error occurred while deleting the photo.", ex);
        }
    }
}
