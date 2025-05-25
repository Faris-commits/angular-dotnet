using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Include(x => x.AppUser)
            .Where(x => x.IsApproved == false)
            .Select(x => new PhotoForApprovalDto
            {
                Id = x.Id,
                Username = x.AppUser != null ? x.AppUser.UserName : null,
                Url = x.Url,
                IsApproved = x.IsApproved,
            }).ToListAsync();
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
