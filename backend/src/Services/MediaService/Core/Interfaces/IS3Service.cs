using System.IO;
using System.Threading.Tasks;

namespace EasyTrack.MediaService.Core.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(string key, Stream fileStream, string contentType);
    Task EnsureBucketExistsAsync(string bucketName);
}
