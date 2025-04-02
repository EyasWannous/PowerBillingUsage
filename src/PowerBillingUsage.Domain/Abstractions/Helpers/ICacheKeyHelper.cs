namespace PowerBillingUsage.Domain.Abstractions.Helpers;

public interface ICacheKeyHelper<Entity>
{
    public string EntityName { get; }
    public string KeyAll => $"allOf_{EntityName}";
    public string PaginateKey => $"paginate_{EntityName}_";
    public string KeyOne => $"oneOf_{EntityName}_Id: ";
    public string CountKey => $"count_{EntityName}";
    public string MakeKeyOne(IEntityId id) => KeyOne + id.Value.ToString();
    public string MakePaginateKey(int skip, int take)
        => PaginateKey + "skip: " + skip + "_take: " + take;
}
