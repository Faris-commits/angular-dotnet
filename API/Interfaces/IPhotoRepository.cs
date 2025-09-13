using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<List<Photo>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int id);
    Task<IEnumerable<Photo>> GetPhotosByTagAsync(int tagId);
    Task<IEnumerable<Photo>> GetAllPhotosAsync();
    void RemovePhoto(Photo photo);
}
