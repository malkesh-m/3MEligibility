namespace MEligibilityPlatform.Domain.Entities;

public partial class Node : ITenantEntity
{
    public int NodeId { get; set; }

    public string? NodeName { get; set; }

    public string? Code { get; set; }

    public string? NodeDesc { get; set; }

    public string? NodeUrl { get; set; }

    public string? ApiuserName { get; set; }

    public string? Apipassword { get; set; }

    public int TenantId { get; set; }

    public string UrlType { get; set; } = null!;

    public DateTime UpdatedByDateTime { get; set; }

    public bool IsAuthenticationRequired { get; set; }

    public bool IsTokenKeywordExist { get; set; }

    public string PasswordField { get; set; } = null!;

    public string TokenKeyword { get; set; } = null!;

    public string UsernameField { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public string Headers { get; set; } = null!;

    public int RetryCount { get; set; }

    public int TimeoutSeconds { get; set; }

    public string AuthType { get; set; } = null!;

    public string AuthSettings { get; set; } = null!;

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<NodeApi> NodeApis { get; set; } = [];

    public virtual ICollection<ParamtersMap> ParamtersMaps { get; set; } = [];
}
