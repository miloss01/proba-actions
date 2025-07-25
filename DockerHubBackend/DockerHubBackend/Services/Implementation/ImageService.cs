using DockerHubBackend.Models;
using DockerHubBackend.Repository.Implementation;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Services.Interface;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using DockerHubBackend.Config;
using Microsoft.Extensions.Options;

namespace DockerHubBackend.Services.Implementation
{
    public class ImageService : IImageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<ImageService> _logger;
        private readonly string _bucketName = "uks-2024";    
     
        public ImageService(IOptions<AwsSettings> awsSettings, ILogger<ImageService> logger)
        {
            var awsConfig = awsSettings.Value;

            // int AWS client
            var credentials = new BasicAWSCredentials(awsConfig.AccessKey, awsConfig.SecretKey);
            _s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(awsConfig.Region));
            _logger = logger;
        }

        // get image from s3
        public async Task<string> GetImageUrl(string fileName)
        {
            try
            {
                _logger.LogInformation("Generating pre-signed URL for file: {FileName}", fileName);
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };

                string preSignedUrl = _s3Client.GetPreSignedURL(request);
                _logger.LogInformation("Pre-signed URL generated successfully for file: {FileName}", fileName);
                return preSignedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pre-signed URL for file: {FileName}", fileName);
                return null;
            }
        }

        // upload file in s3
        public async Task UploadImage(string imageName, Stream fileStream)
        {
            try
            {
                _logger.LogInformation("Uploading file to S3 with name: {ImageName}", imageName);
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(fileStream, _bucketName, imageName);
                _logger.LogInformation("File uploaded successfully to S3 with name: {ImageName}", imageName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to S3 with name: {ImageName}", imageName);
            }
        }

        // delete image from S3
        public async Task DeleteImage(string filePath)
        {
            try
            {
                _logger.LogInformation("Deleting file from S3 with path: {FilePath}", filePath);
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                _logger.LogInformation("File deleted successfully from S3 with path: {FilePath}", filePath);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Amazon S3 error while deleting file with path: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from S3 with path: {FilePath}", filePath);
            }
        }

    }
}
