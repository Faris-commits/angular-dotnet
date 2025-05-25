using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class TagRepository(DataContext context) : ITagRepository
{
    public async Task AddTagAsync(Tag tag)
    {
        await context.Tags.AddAsync(tag);
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync() =>
        await context.Tags.OrderBy(t => t.Name).ToListAsync();

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        return await context.Tags.FindAsync(id);
    }

    public async Task<Tag?> GetTagByNameAsync(string name) =>
        await context.Tags.FirstOrDefaultAsync(t => t.Name == name);

    public void RemoveTag(Tag tag)
    {
        context.Tags.Remove(tag);
    }

    public async Task<bool> SaveAllAsync() =>
        await context.SaveChangesAsync() > 0;
}