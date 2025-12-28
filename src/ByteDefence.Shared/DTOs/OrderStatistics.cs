namespace ByteDefence.Shared.DTOs;

public class OrderStatistics
{
    public int Draft { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Total => Draft + Pending + Approved + Completed + Cancelled;
}
