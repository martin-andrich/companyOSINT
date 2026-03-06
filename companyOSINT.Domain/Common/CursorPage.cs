namespace companyOSINT.Domain.Common;

public class CursorPage<T>(List<T> items, Guid? nextCursor)
{
    public List<T> Items { get; set; } = items;
    public Guid? NextCursor { get; set; } = nextCursor;
}
