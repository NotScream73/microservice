using Amazon.S3;
using Amazon.S3.Model;
using Domain.Models;
using MassTransit;
using System.Text;

namespace StorageService
{
    public class OrderSubmittedConsumer : IConsumer<MessageDTO>
    {
        private readonly IAmazonS3 _s3Client;
        private const string BucketName = "reports";
        public OrderSubmittedConsumer(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task Consume(ConsumeContext<MessageDTO> context)
        {
            if (!await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, BucketName))
            {
                await _s3Client.PutBucketAsync(BucketName);
            }
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(context.Message.File));

            var uploadRequest = new PutObjectRequest
            {
                InputStream = csvStream,
                BucketName = BucketName,
                Key = context.Message.FileName,
                ContentType = "text/csv"
            };

            await _s3Client.PutObjectAsync(uploadRequest);
        }
    }
}
