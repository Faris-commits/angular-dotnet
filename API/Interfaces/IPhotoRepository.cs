
using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<List<Photo>> GetUnapprovedPhotos(); 
    Task<Photo?> GetPhotoById(int id);
    Task<IEnumerable<Photo>> GetPhotosByTagAsync(int tagId);
    void RemovePhoto(Photo photo);
}
