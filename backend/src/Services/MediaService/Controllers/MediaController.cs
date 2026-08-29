using EasyTrack.MediaService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyTrack.MediaService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/media")]
public class MediaController : ControllerBase
{
    private readonly IS3Service _s3Service;
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public MediaController(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    [HttpPost("upload-slip")]
    [RequestSizeLimit(MaxFileSizeBytes + 1024 * 50)] // Prevent DOS by rejecting large payloads early
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes)]
    public async Task<IActionResult> UploadSlip(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "File size exceeds the 5 MB limit." });
        }

        var contentType = file.ContentType.ToLower();
        if (!AllowedContentTypes.Contains(contentType))
        {
            return BadRequest(new { message = "Unsupported file type. Allowed formats: JPEG, PNG, WEBP." });
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        
        // Enforce safe and allowed file extensions to prevent masquerading
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Invalid file extension. Allowed extensions: .jpg, .jpeg, .png, .webp" });
        }

        // Verify File Signature (Magic Numbers) to prevent script injection in forged image formats
        try
        {
            using var signatureStream = file.OpenReadStream();
            if (!ValidateImageSignature(signatureStream, contentType))
            {
                return BadRequest(new { message = "File content verification failed. The file is not a valid image format." });
            }
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Failed to verify file integrity." });
        }

        var userId = GetUserId();
        
        // Folder layout: {userId}/{yyyy}/{mm}/{uuid}.{ext}
        var now = DateTime.UtcNow;
        var yyyy = now.ToString("yyyy", System.Globalization.CultureInfo.InvariantCulture);
        var mm = now.ToString("MM", System.Globalization.CultureInfo.InvariantCulture);
        var filename = $"{Guid.NewGuid()}{extension}";
        var key = $"{userId}/{yyyy}/{mm}/{filename}";

        try
        {
            using var stream = file.OpenReadStream();
            var fileUrl = await _s3Service.UploadFileAsync(key, stream, contentType);

            return Ok(new { url = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload file to storage.", error = ex.Message });
        }
    }

    private static bool ValidateImageSignature(Stream stream, string contentType)
    {
        // Save stream position if seekable
        long originalPosition = 0;
        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
        }

        try
        {
            byte[] buffer = new byte[12];
            int readBytes = stream.Read(buffer, 0, buffer.Length);
            if (readBytes < 4)
            {
                return false;
            }

            // JPEG Signature: FF D8 FF
            if (contentType == "image/jpeg")
            {
                return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            }

            // PNG Signature: 89 50 4E 47
            if (contentType == "image/png")
            {
                return buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
            }

            // WEBP Signature: "RIFF" header and "WEBP" format marker
            if (contentType == "image/webp")
            {
                if (readBytes < 12) return false;
                bool isRiff = buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46;
                bool isWebp = buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
                return isRiff && isWebp;
            }

            return false;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    private Guid GetUserId()
    {
        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }
}
