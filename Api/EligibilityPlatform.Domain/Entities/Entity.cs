using System.Numerics;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace MEligibilityPlatform.Domain.Entities;

public partial class Entity
{
    public int EntityId { get; set; }

    public string? EntityName { get; set; }

    public int? CountryId { get; set; }

    public int? CityId { get; set; }

    public string? EntityAddress { get; set; }

    //public int? BaseCurrencyId { get; set; }

    public string? Code { get; set; }

    public string? Entitylocation { get; set; }

    public bool Isparent { get; set; }

    public int? ParentEnitityId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }

    //public virtual Currency? BaseCurrency { get; set; }

    //public virtual ICollection<Category> Categories { get; set; } = [];

    //public virtual City? City { get; set; }

    //public virtual Country? Country { get; set; }

    //public virtual ICollection<Ecard> Ecards { get; set; } = [];

    //public virtual ICollection<Erule> Erules { get; set; } = [];

    //public virtual ICollection<Factor> Factors { get; set; } = [];

    //public virtual ICollection<HistoryPc> HistoryPcs { get; set; } = [];

    //public virtual ICollection<ManagedList> ManagedLists { get; set; } = [];

    //public virtual ICollection<Node> Nodes { get; set; } = [];

    ////public virtual ICollection<Parameter> Parameters { get; set; } = [];

    //public virtual ICollection<Pcard> Pcards { get; set; } = [];

    //public virtual ICollection<Product> Products { get; set; } = [];

    //public virtual ICollection<Setting> Settings { get; set; } = [];

    //public virtual ICollection<User> Users { get; set; } = [];

    //public virtual ICollection<ExceptionManagement> ExceptionManagements { get; set; } = [];


}
