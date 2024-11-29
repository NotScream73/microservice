using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace StorageService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;
    private const string BucketName = "reports";

    public StorageController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is missing or empty.");
        }

        await using var stream = file.OpenReadStream();
        var uploadRequest = new PutObjectRequest
        {
            InputStream = stream,
            BucketName = BucketName,
            Key = file.FileName,
            ContentType = file.ContentType
        };

        var response = await _s3Client.PutObjectAsync(uploadRequest);
        return Ok("File uploaded successfully to MinIO.");
    }


    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(BucketName, fileName);

            await using var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms);
            ms.Position = 0;

            return File(ms.ToArray(), "text/csv");
        }
        catch (AmazonS3Exception s3Ex)
        {
            return NotFound(new { message = $"S3 Error: {s3Ex.Message}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal Error: {ex.Message}" });
        }
    }
}
