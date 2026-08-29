using Amazon.S3;
using Amazon.S3.Model;
using EasyTrack.MediaService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EasyTrack.MediaService.Infrastructure.S3;

public class MinIoS3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;

    public MinIoS3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["Aws:BucketName"] ?? _configuration["Aws__BucketName"] ?? "easytrack-slips";
    }

    public async Task<string> UploadFileAsync(string key, Stream fileStream, string contentType)
    {
        await EnsureBucketExistsAsync(_bucketName);

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        var response = await _s3Client.PutObjectAsync(putRequest);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Error uploading file to S3. Status: {response.HttpStatusCode}");
        }

        // Generate final URL. For local MinIO, it is: http://localhost:9000/easytrack-slips/key
        var serviceUrl = _configuration["Aws:ServiceUrl"] ?? _configuration["Aws__ServiceUrl"] ?? "http://localhost:9000";
        
        // Remove trailing slash if present
        if (serviceUrl.EndsWith("/"))
        {
            serviceUrl = serviceUrl.Substring(0, serviceUrl.Length - 1);
        }

        return $"{serviceUrl}/{_bucketName}/{key}";
    }

    public async Task EnsureBucketExistsAsync(string bucketName)
    {
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
        if (!bucketExists)
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName
            };
            await _s3Client.PutBucketAsync(putBucketRequest);

            // Set bucket policy to allow Public Read-Only so that images can be viewed
            var policyJson = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Sid"": ""PublicRead"",
                        ""Effect"": ""Allow"",
                        ""Principal"": ""*"",
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                    }}
                ]
            }}";

            await _s3Client.PutBucketPolicyAsync(bucketName, policyJson);
        }
    }
}
