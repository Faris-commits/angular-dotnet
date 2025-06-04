using API.Interfaces;

namespace API.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    public IUserRepository UserRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public ILikesRepository LikesRepository { get; }
    public IPhotoRepository PhotoRepository { get; }
    public ITagRepository TagRepository { get; }

    public DataContext Context => _context;

    public UnitOfWork(
        DataContext context,
        IUserRepository userRepository,
        ILikesRepository likesRepository,
        IMessageRepository messageRepository,
        IPhotoRepository photoRepository,
        ITagRepository tagRepository
    )
    {
        _context = context;
        UserRepository = userRepository;
        LikesRepository = likesRepository;
        MessageRepository = messageRepository;
        PhotoRepository = photoRepository;
        TagRepository = tagRepository;
    }

    public async Task<bool> Complete()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return _context.ChangeTracker.HasChanges();
    }
}
