using API.Entities;

namespace API.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(int id);
    Task<Tag?> GetTagByNameAsync(string name);
    Task AddTagAsync(Tag tag);
    void RemoveTag(Tag tag);
    Task<bool> SaveAllAsync();
}
