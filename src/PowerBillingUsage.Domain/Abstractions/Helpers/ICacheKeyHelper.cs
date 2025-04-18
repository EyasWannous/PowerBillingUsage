namespace PowerBillingUsage.Domain.Abstractions.Helpers;

public interface ICacheKeyHelper<Entity>
{
    public string EntityName { get; }
    public string AllKey => $"allOf_{EntityName}";
    public string PaginateKeyTag => $"paginate_{EntityName}_";
    public string KeyOfOneTag => $"oneOf_{EntityName}_Id: ";
    public string CountKey => $"count_{EntityName}";
    public string MakeKeyOne(IEntityId id) => KeyOfOneTag + id.Value.ToString();
    public string MakePaginateKey(int skip, int take)
        => PaginateKeyTag + "skip: " + skip + "_take: " + take;
}
