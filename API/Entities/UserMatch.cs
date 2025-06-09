namespace API.Entities;

public class UserMatch
{
    public int Id { get; set; }
    public int SourceUserId { get; set; }
    public AppUser SourceUser { get; set; }
    public int TargetUserId { get; set; }
    public AppUser TargetUser { get; set; }
    public double Score { get; set; }
}
