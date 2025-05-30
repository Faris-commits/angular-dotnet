using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context) : IPhotoRepository
{
    public async Task<Photo> GetPhotoById(int photoId)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Include(p => p.PhotoTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == photoId);
    }

    public async Task<List<Photo>> GetUnapprovedPhotos()
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Include(p => p.AppUser)
            .Include(p => p.PhotoTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => !p.IsApproved)
            .ToListAsync();
    }

    public async Task<IEnumerable<Photo>> GetPhotosByTagAsync(int tagId)
    {
        return await context.Photos
         .Include(p => p.PhotoTags)
         .ThenInclude(pt => pt.Tag)
         .Where(p => p.PhotoTags.Any(pt => pt.TagId == tagId))
         .ToListAsync();
    }


    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
