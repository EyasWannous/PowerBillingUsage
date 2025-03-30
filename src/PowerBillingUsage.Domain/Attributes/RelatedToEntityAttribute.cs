namespace PowerBillingUsage.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RelatedToEntityAttribute : Attribute
{
    public Type EntityType { get; }

    public RelatedToEntityAttribute(Type entityType) => EntityType = entityType;
}