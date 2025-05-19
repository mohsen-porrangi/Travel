namespace BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}