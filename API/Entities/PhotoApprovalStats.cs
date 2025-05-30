namespace API.Entities;

public class PhotoApprovalStats
{

    public string Username { get; set; } = null!;
    public int ApprovedPhotos { get; set; }
    public int UnapprovedPhotos { get; set; }
}