using microservices_project.Core.Domain;
using microservices_project.Infrastructure.DataStorage;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;


namespace microservices_project.Infrastructure.DataStorage.Services;



public class MediaService
{
    private readonly IMinioClient _minio;
    private readonly ServerDbContext _context;
    private const string BucketName = "media";

    public MediaService(IMinioClient minio, ServerDbContext context)
    {
        _minio = minio;
        _context = context;
        // await InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var exists = await _minio
            .BucketExistsAsync(
            new BucketExistsArgs()
            .WithBucket(BucketName));

        if (!exists)
        {
            await _minio.MakeBucketAsync(
            new MakeBucketArgs()
            .WithBucket(BucketName));
        }

    }

    public async Task<Media> AddAsync(IFormFile file, Notification notification)
    {
        var objectName = Guid.NewGuid() + "-" + file.FileName;

        using (var fileStream = new MemoryStream())
        {
            file.CopyTo(fileStream);
            var fileBytes = fileStream.ToArray();
            await _minio.PutObjectAsync(
                new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithStreamData(new MemoryStream(fileBytes))
                .WithObjectSize(fileStream.Length)
                .WithContentType("application/octet-stream")
            ).ConfigureAwait(false);
        }

        var media = new Media
        {
            Source = objectName,
            CreatedAt = DateTime.UtcNow,
            Notification = notification
        };

        _context.Medias.Add(media);
        await _context.SaveChangesAsync();

        return media;
    }

    public async Task<Media?> FindAsync(long id) => await _context.Medias.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<bool> RemoveAsync(int id)
    {
        var media = await _context.Medias.FirstOrDefaultAsync(m => m.Id == id);

        if (media == null) return false;

        try
        {
            await _minio.RemoveObjectAsync(
                new RemoveObjectArgs()
                .WithBucket(BucketName)
                .WithObject(media.Source));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        _context.Medias.Remove(media);
        await _context.SaveChangesAsync();

        return true;
    }

      public async Task<string> GetPresignedUrlAsync(string objectName)
    {
        return await _minio.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithExpiry(3600));
    }
}