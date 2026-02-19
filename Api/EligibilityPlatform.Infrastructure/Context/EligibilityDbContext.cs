using MEligibilityPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Context;

public partial class EligibilityDbContext : DbContext
{
    public EligibilityDbContext()
    {
    }

    public EligibilityDbContext(DbContextOptions<EligibilityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AmountEligibility> AmountEligibilities { get; set; }

    public virtual DbSet<ApiParameter> ApiParameters { get; set; }

    public virtual DbSet<ApiResponse> ApiResponses { get; set; }

    public virtual DbSet<Apidetail> Apidetails { get; set; }

    public virtual DbSet<ApiParameterMap> ApiParameterMaps { get; set; }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<Audit> Audits { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Condition> Conditions { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<DataType> DataTypes { get; set; }

    public virtual DbSet<Ecard> Ecards { get; set; }

    public virtual DbSet<Entity> Entities { get; set; }

    public virtual DbSet<Erule> Erules { get; set; }

    public virtual DbSet<ExceptionManagement> ExceptionManagements { get; set; }


    public virtual DbSet<ExceptionProduct> ExceptionProducts { get; set; }

    public virtual DbSet<Factor> Factors { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<HistoryEc> HistoryEcs { get; set; }

    public virtual DbSet<HistoryEr> HistoryErs { get; set; }

    public virtual DbSet<HistoryParameter> HistoryParameters { get; set; }

    public virtual DbSet<HistoryPc> HistoryPcs { get; set; }

    public virtual DbSet<ImportDocument> ImportDocuments { get; set; }

    public virtual DbSet<ListItem> ListItems { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<MakerChecker> MakerCheckers { get; set; }

    public virtual DbSet<ManagedList> ManagedLists { get; set; }

    public virtual DbSet<MappingFunction> MappingFunctions { get; set; }

    public virtual DbSet<Node> Nodes { get; set; }

    public virtual DbSet<NodeApi> NodeApis { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }
    public virtual DbSet<ParameterComputedValue> ParameterComputedValues { get; set; }

    public virtual DbSet<ParamtersMap> ParamtersMaps { get; set; }

    public virtual DbSet<Pcard> Pcards { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCap> ProductCaps { get; set; }

    public virtual DbSet<ProductParam> ProductParams { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Screen> Screens { get; set; }

    public virtual DbSet<SecurityRole> SecurityRoles { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserStatus> UserStatuses { get; set; }

    public virtual DbSet<EvaluationHistory> EvaluationHistories { get; set; }
    public virtual DbSet<EruleMaster> EruleMasters { get; set; }
    public virtual DbSet<ProductCapAmount> ProductCapAmounts { get; set; }
    public virtual DbSet<RejectionReasons> RejectionReasons { get; set; }
    public virtual DbSet<IntegrationApiEvaluation> IntegrationApiEvaluation { get; set; }
    public virtual DbSet<SystemParameter> SystemParameters { get; set; }
    public virtual DbSet<ParameterBinding> ParameterBindings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AmountEligibility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AmaountEligibility");

            entity.ToTable("AmountEligibility");

            entity.HasIndex(e => e.PcardId, "IX_AmaountEligibility_PcardID");

            entity.Property(e => e.PcardId).HasColumnName("PcardID");

            entity.HasOne(d => d.Pcard).WithMany(p => p.AmountEligibilities)
                .HasForeignKey(d => d.PcardId)
                .HasConstraintName("FK_AmaountEligibility_PCards_PcardID");
        });

        modelBuilder.Entity<ApiParameter>(entity =>
        {
            entity.HasKey(e => e.ApiParamterId);
            entity.HasIndex(e => e.TenantId, "IX_ApiParameter_TenantId");

            entity.HasIndex(e => e.ApiId, "IX_ApiParameters_ApiId");

            entity.HasOne(d => d.Api).WithMany(p => p.ApiParameters).HasForeignKey(d => d.ApiId);
        });

        modelBuilder.Entity<ApiResponse>(entity =>
        {
            entity.HasKey(e => e.ResponceId);

            entity.HasIndex(e => e.NodeApiApiid, "IX_ApiResponses_NodeApiApiid");

            entity.HasOne(d => d.NodeApiApi).WithMany(p => p.ApiResponses).HasForeignKey(d => d.NodeApiApiid);
        });

        modelBuilder.Entity<Apidetail>(entity =>
        {
            entity.HasKey(e => e.ApidetailsId).HasName("PK__APIDetai__172F379ECF036E62");

            entity.ToTable("APIDetails");

            entity.Property(e => e.ApidetailsId).HasColumnName("APIDetailsId");
            entity.Property(e => e.Apiid).HasColumnName("APIId");
            entity.Property(e => e.CallingParamName).HasMaxLength(50);
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.FromApiid).HasColumnName("FromAPIId");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.SourceApiparam)
                .HasMaxLength(50)
                .HasColumnName("SourceAPIParam");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Api).WithMany(p => p.Apidetails)
                .HasForeignKey(d => d.Apiid)
                .HasConstraintName("FK__APIDetail__APIId__0E6E26BF");

            entity.HasOne(d => d.DataType).WithMany(p => p.Apidetails)
                .HasForeignKey(d => d.DataTypeId)
                .HasConstraintName("FK__APIDetail__DataT__6D0D32F4");
        });

        modelBuilder.Entity<ApiParameterMap>(entity =>
        {
            entity.ToTable("ApiParameterMaps");
            entity.HasIndex(e => e.TenantId, "IX_ApiParameterMap_TenantId");

            entity.HasIndex(e => e.ApiParameterId, "IX_ApisParameterMaps_ApiParameterId");

            entity.HasIndex(e => e.ParameterId, "IX_ApisParameterMaps_ParameterId");

            entity.HasOne(d => d.ApiParameter).WithMany(p => p.ApisParameterMaps).HasForeignKey(d => d.ApiParameterId);

            entity.HasOne(d => d.Parameter).WithMany(p => p.ApisParameterMaps).HasForeignKey(d => d.ParameterId);
        });

        modelBuilder.Entity<Audit>(entity =>
        {
            entity.ToTable("Audit");
            entity.HasIndex(e => e.TenantId, "Audit_TenantId");
           
            entity.Property(e => e.ActionDate).HasColumnType("datetime");
            entity.Property(e => e.ActionName).HasMaxLength(50);
            entity.Property(e => e.FieldName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.NewValue).HasColumnType("longtext");
            entity.Property(e => e.OldValue).HasColumnType("longtext");
            entity.Property(e => e.TableName).HasMaxLength(50);
            entity.Property(e => e.IPAddress).HasMaxLength(50);
            entity.Property(e => e.Comments).HasMaxLength(250);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B94173D4F");

            entity.ToTable("Category");

            entity.HasIndex(e => e.TenantId, "IX_Category_TenantId");

            entity.Property(e => e.CatDescription).HasMaxLength(250);
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Categories)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__Category__Entity__123EB7A3");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__City__F2D21B76E8C23F00");

            entity.ToTable("City");

            entity.Property(e => e.CityName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK__City__CountryId__25869641");
        });

        modelBuilder.Entity<Condition>(entity =>
        {
            entity.HasKey(e => e.ConditionId).HasName("PK__Conditio__37F5C0CFF020DF36");

            entity.ToTable("Condition");

            entity.Property(e => e.ConditionValue).HasMaxLength(20);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PK__Country__10D1609FC12BD2CA");

            entity.ToTable("Country");

            entity.Property(e => e.CountryName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("PK__Currency__14470AF03460AA25");

            entity.ToTable("Currency");

            entity.Property(e => e.CurrencyName).HasMaxLength(250);
            entity.Property(e => e.Isocode).HasMaxLength(3);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.MidRate).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MinorUnitsName).HasMaxLength(20);
        });

        modelBuilder.Entity<DataType>(entity =>
        {
            entity.HasKey(e => e.DataTypeId).HasName("PK__DataType__4382081F001AD10C");

            entity.ToTable("DataType");

            entity.Property(e => e.DataTypeName).HasMaxLength(20);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Ecard>(entity =>
        {
            entity.HasKey(e => e.EcardId).HasName("PK__ECards__B49054664FE04CEA");

            entity.ToTable("ECards");

            entity.HasIndex(e => e.TenantId, "IX_ECards_TenantId");

            entity.Property(e => e.EcardId).HasColumnName("ECardId");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.EcardDesc)
                .HasMaxLength(50)
                .HasColumnName("ECardDesc");
            entity.Property(e => e.EcardName)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("ECardName");
            entity.Property(e => e.Expression)
                .HasDefaultValue("")
                .HasColumnType("text");
            entity.Property(e => e.Expshown).HasColumnType("longtext");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Ecards)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__ECards__TenantId__160F4887");
        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.EntityId).HasName("PK__Entity__9C892F9DF2DF4E0E");

            entity.ToTable("Entity");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.EntityAddress).HasMaxLength(250);
            entity.Property(e => e.EntityName).HasMaxLength(50);
            entity.Property(e => e.Entitylocation).HasMaxLength(50);
            entity.Property(e => e.Isparent).HasColumnName("isparent");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            //entity.HasOne(d => d.BaseCurrency).WithMany(p => p.Entities)
            //    .HasForeignKey(d => d.BaseCurrencyId)
            //    .HasConstraintName("FK__Entity__BaseCurr__2E1BDC42");

            //entity.HasOne(d => d.City).WithMany(p => p.Entities)
            //    .HasForeignKey(d => d.CityId)
            //    .HasConstraintName("FK__Entity__CityId__300424B4");

            //entity.HasOne(d => d.Country).WithMany(p => p.Entities)
            //    .HasForeignKey(d => d.CountryId)
            //    .HasConstraintName("FK__Entity__CountryI__2F10007B");
        });
        modelBuilder.Entity<EvaluationHistory>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_EvaluationHistory_TenantId");

            entity.Property(e => e.LoanNo)
        .HasMaxLength(50)
        .IsRequired();

            entity.Property(e => e.NationalId)
                .HasMaxLength(50)
                .IsRequired();
            //        entity.HasIndex(e => new { e.LoanNo, e.NationalId })
            //.IsUnique();
            entity.Property(e => e.TenantId)
            .ValueGeneratedNever();
        });
        modelBuilder.Entity<EruleMaster>(entity =>
        {
            entity.ToTable("EruleMaster");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.TenantId, "IX_ExceptionManagement_TenantId");

            entity.Property(e => e.EruleName).HasMaxLength(50);
            entity.Property(e => e.EruleDesc).HasMaxLength(1000);

            entity.HasIndex(i => new { i.EruleName, i.TenantId }).IsUnique();
        });

        modelBuilder.Entity<Erule>(entity =>
        {
            entity.HasKey(e => e.EruleId).HasName("PK__ERule__5A54BCF376F1F4DF");
            entity.Property(e => e.EruleId)
             .ValueGeneratedOnAdd();
            entity.ToTable("ERule");

            entity.HasIndex(e => e.TenantId, "IX_ERule_TenantId");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(e => e.ExpShown).HasColumnType("longtext");
            entity.Property(e => e.Expression)
                .HasDefaultValue("")
                .HasColumnType("longtext");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasIndex(i => new { i.EruleMasterId, i.Version }).IsUnique();

            //entity.HasOne(d => d.Entity).WithMany(p => p.Erules)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__ERule__TenantId__19DFD96B");

            entity.HasOne(d => d.EruleMaster).WithMany(p => p.Erules)
                .HasForeignKey(d => d.EruleMasterId)
                .HasConstraintName("FK__ERule__EruleMasterId__33EED56C");

            entity.HasMany(d => d.Ecards).WithMany(p => p.Rules)
                .UsingEntity<Dictionary<string, object>>(
                    "EcardDetail",
                    r => r.HasOne<Ecard>().WithMany()
                        .HasForeignKey("EcardId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ECardDeta__ECard__14270015"),
                    l => l.HasOne<Erule>().WithMany()
                        .HasForeignKey("RuleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ECardDeta__RuleI__151B244E"),
                    j =>
                    {
                        j.HasKey("RuleId", "EcardId").HasName("PK__ECardDet__6A4D5DA4302C8048");
                        j.ToTable("ECardDetails");
                        j.IndexerProperty<int>("EcardId").HasColumnName("ECardId");
                    });
        });

        modelBuilder.Entity<ExceptionManagement>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_ExceptionManagement_TenantId");

            entity.Property(e => e.Expression).HasDefaultValue("");
            entity.Property(e => e.FixedPercentage).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LimitAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VariationPercentage).HasColumnType("decimal(18, 2)");
            //entity.HasOne(d => d.Entity).WithMany(p => p.ExceptionManagements)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__ExceptionMgmt__TenantId__20DFD973").OnDelete(DeleteBehavior.Cascade); ;
        });

     

        modelBuilder.Entity<ExceptionProduct>(entity =>
        {
            entity.ToTable("ExceptionProducts");

            entity.HasIndex(e => e.ExceptionManagementId, "IX_ExceptionProduct_ExceptionManagementId");

            entity.HasIndex(e => e.ProductId, "IX_ExceptionProduct_ProductId");

            entity.HasOne(d => d.ExceptionManagement).WithMany(p => p.ExceptionProducts).HasForeignKey(d => d.ExceptionManagementId);

            entity.HasOne(d => d.Product).WithMany(p => p.ExceptionProducts).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Factor>(entity =>
        {
            entity.HasKey(e => e.FactorId).HasName("PK__Factors__E733AADDA7D15B56");

            entity.HasIndex(e => e.TenantId, "IX_Factors_TenantId");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.FactorName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Value1).HasMaxLength(50);
            entity.Property(e => e.Value2).HasMaxLength(50);

            entity.HasOne(d => d.Condition).WithMany(p => p.Factors)
                .HasForeignKey(d => d.ConditionId)
                .HasConstraintName("FK__Factors__Conditi__4316F928");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Factors)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__Factors__EntityI__1DB06A4F");

            entity.HasOne(d => d.Parameter).WithMany(p => p.Factors)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK__Factors__Paramet__1EA48E88");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.PermissionId, e.RoleId }).HasName("PK__RolePerm__3BB3612C136B1C77");
            entity.HasIndex(e => e.TenantId, "IX_RolePermission_TenantId");

            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(gr => gr.RoleId)
                .HasDatabaseName("IX_RolePermission_RoleId");
            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RolePerm__Role__1F98B2C1");          
            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RolePerm__PermI__208CD6FA");
        });
        modelBuilder.Entity<HistoryEc>(entity =>
        {
            entity.HasKey(e => e.Seq).HasName("PK__HistoryE__CA1E3C885A597344");

            entity.ToTable("HistoryEC");

            entity.Property(e => e.EcardId).HasColumnName("ECardId");
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Ecard).WithMany(p => p.HistoryEcs)
                .HasForeignKey(d => d.EcardId)
                .HasConstraintName("FK__HistoryEC__ECard__2180FB33");

            entity.HasOne(d => d.Tran).WithMany(p => p.HistoryEcs)
                .HasForeignKey(d => d.TranId)
                .HasConstraintName("FK__HistoryEC__TranI__22751F6C");
        });

        modelBuilder.Entity<HistoryEr>(entity =>
        {
            entity.HasKey(e => e.Seq).HasName("PK__HistoryE__CA1E3C8873B9647A");

            entity.ToTable("HistoryER");

            entity.Property(e => e.EcardId).HasColumnName("ECardId");
            entity.Property(e => e.EruleId).HasColumnName("ERuleId");
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Ecard).WithMany(p => p.HistoryErs)
                .HasForeignKey(d => d.EcardId)
                .HasConstraintName("FK__HistoryER__ECard__236943A5");

            entity.HasOne(d => d.Erule).WithMany(p => p.HistoryErs)
                .HasForeignKey(d => d.EruleId)
                .HasConstraintName("FK__HistoryER__ERule__245D67DE");

            entity.HasOne(d => d.Tran).WithMany(p => p.HistoryErs)
                .HasForeignKey(d => d.TranId)
                .HasConstraintName("FK__HistoryER__TranI__25518C17");
        });

        modelBuilder.Entity<HistoryParameter>(entity =>
        {
            entity.HasKey(e => e.Seq).HasName("PK__HistoryP__CA1E3C8804E78B61");

            entity.Property(e => e.Condition).HasMaxLength(250);
            entity.Property(e => e.EruleId).HasColumnName("ERuleId");
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ParameterId).HasColumnName("ParameterID");
            entity.Property(e => e.ValueRet).HasMaxLength(50);

            entity.HasOne(d => d.Erule).WithMany(p => p.HistoryParameters)
                .HasForeignKey(d => d.EruleId)
                .HasConstraintName("FK__HistoryPa__ERule__2645B050");

            entity.HasOne(d => d.Parameter).WithMany(p => p.HistoryParameters)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK__HistoryPa__Param__2739D489");

            entity.HasOne(d => d.Tran).WithMany(p => p.HistoryParameters)
                .HasForeignKey(d => d.TranId)
                .HasConstraintName("FK__HistoryPa__TranI__282DF8C2");
        });

        modelBuilder.Entity<HistoryPc>(entity =>
        {
            entity.HasKey(e => e.TranId).HasName("PK__HistoryP__F70897C964A130ED");

            entity.ToTable("HistoryPC");

            entity.HasIndex(e => e.TenantId, "IX_HistoryPC_TenantId");

            entity.Property(e => e.CustomerId).HasMaxLength(50);
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.PcardId).HasColumnName("PCardId");
            entity.Property(e => e.TransReference).HasMaxLength(50);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            //entity.HasOne(d => d.Entity).WithMany(p => p.HistoryPcs)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__HistoryPC__Entit__29221CFB");

            entity.HasOne(d => d.Pcard).WithMany(p => p.HistoryPcs)
                .HasForeignKey(d => d.PcardId)
                .HasConstraintName("FK__HistoryPC__PCard__2A164134");

            entity.HasOne(d => d.Product).WithMany(p => p.HistoryPcs)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__HistoryPC__Produ__2B0A656D");

            entity.HasOne(d => d.User).WithMany(p => p.HistoryPcs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__HistoryPC__UserI__2BFE89A6");
        });

        modelBuilder.Entity<ImportDocument>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<ListItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__ListItem__727E838B4F37F516");
            entity.HasIndex(e => e.TenantId, "IX_ListItem_TenantId");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ItemName).HasMaxLength(200);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasOne(d => d.List).WithMany(p => p.ListItems)
                .HasForeignKey(d => d.ListId)
                .HasConstraintName("FK__ListItems__ListI__2CF2ADDF")
                .OnDelete(DeleteBehavior.Restrict); ;
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Log");

            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
        });

        modelBuilder.Entity<MakerChecker>(entity =>
        {
            entity.ToTable("MakerChecker");

            entity.HasIndex(e => e.CheckerId, "IX_MakerChecker_CheckerId");

            entity.HasIndex(e => e.MakerId, "IX_MakerChecker_MakerId");

            entity.HasOne(d => d.Checker).WithMany(p => p.MakerCheckerCheckers).HasForeignKey(d => d.CheckerId);

            entity.HasOne(d => d.Maker).WithMany(p => p.MakerCheckerMakers).HasForeignKey(d => d.MakerId);
        });

        modelBuilder.Entity<ManagedList>(entity =>
        {
            entity.HasKey(e => e.ListId).HasName("PK__ManagedL__E3832805A9F28A9F");

            entity.ToTable("ManagedList");

            entity.HasIndex(e => e.TenantId, "IX_ManagedList_TenantId");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ListName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            //entity.HasOne(d => d.Entity).WithMany(p => p.ManagedLists)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__ManagedLi__Entit__2DE6D218");
        });

        modelBuilder.Entity<MappingFunction>(entity =>
        {
            entity.HasKey(e => e.MapFunctionId).HasName("PK__MappingF__8032889CB83B0F3A");

            entity.ToTable("MappingFunction");

            entity.Property(e => e.MapFunctionId).HasColumnName("MapFunctionID");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.MapFunctionValue).HasMaxLength(20);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.NodeId).HasName("PK__Nodes__6BAE22635CDBA8E1");

            entity.HasIndex(e => e.TenantId, "IX_Nodes_TenantId");

            entity.Property(e => e.Apipassword)
                .HasMaxLength(50)
                .HasColumnName("APIPassword");
            entity.Property(e => e.ApiuserName)
                .HasMaxLength(50)
                .HasColumnName("APIUserName");
            entity.Property(e => e.AuthSettings).HasDefaultValue("");
            entity.Property(e => e.AuthType).HasDefaultValue("");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Headers).HasDefaultValue("");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.NodeDesc).HasMaxLength(50);
            entity.Property(e => e.NodeName).HasMaxLength(50);
            entity.Property(e => e.NodeUrl)
                .HasColumnType("text")
                .HasColumnName("NodeURL");
            entity.Property(e => e.PasswordField).HasDefaultValue("");
            entity.Property(e => e.TokenKeyword).HasDefaultValue("");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UrlType)
                .HasMaxLength(10)
                .HasDefaultValue("");
            entity.Property(e => e.UsernameField).HasDefaultValue("");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Nodes)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__Nodes__TenantId__2FCF1A8A");
        });

        modelBuilder.Entity<NodeApi>(entity =>
        {
            entity.HasKey(e => e.Apiid).HasName("PK__NodeAPIs__ABCD70D2636BBE73");

            entity.ToTable("NodeAPIs");

            entity.HasIndex(e => e.NodeId, "NodeAPIs.IX_NodeAPIs_NodeId");
            entity.HasIndex(e => e.TenantId, "NodeAPIs.IX_NodeAPIs_TenantId");

            entity.Property(e => e.Apiid).HasColumnName("APIId");
            entity.Property(e => e.Apidesc)
                .HasColumnType("text")
                .HasColumnName("APIDesc");
            entity.Property(e => e.Apiname).HasColumnName("APIName");
            entity.Property(e => e.BinaryXml)
                .IsUnicode(false)
                .HasColumnName("BinaryXML");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.EndpointPath).HasDefaultValue("");
            entity.Property(e => e.Header).HasColumnType("text");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.RequestBody).HasDefaultValue("");
            entity.Property(e => e.RequestParameters).HasDefaultValue("");
            entity.Property(e => e.ResponseRootPath).HasDefaultValue("");
            entity.Property(e => e.TargetTable).HasDefaultValue("");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.XmlfileName)
                .HasMaxLength(60)
                .HasColumnName("XMLFileName");

            entity.HasOne(d => d.Node).WithMany(p => p.NodeApis)
                .HasForeignKey(d => d.NodeId)
                .HasConstraintName("FK__NodeAPIs__NodeId__2EDAF651");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__Paramete__F80C62776A8B4041");

            entity.ToTable("Parameter");

            entity.HasIndex(e => e.TenantId, "IX_Parameter_TenantId");
            entity.HasIndex(e => new { e.TenantId, e.ParameterName })
       .IsUnique()
       .HasDatabaseName("UQ_Parameter_TenantId_ParameterName");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.FactorOrder).HasMaxLength(3);
            entity.Property(e => e.IsKyc).HasColumnName("IsKYC");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ParameterName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Condition).WithMany(p => p.Parameters)
                .HasForeignKey(d => d.ConditionId)
                .HasConstraintName("FK__Parameter__Condi__34C8D9D1");

            entity.HasOne(d => d.DataType).WithMany(p => p.Parameters)
                .HasForeignKey(d => d.DataTypeId)
                .HasConstraintName("FK__Parameter__DataT__33D4B598");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Parameters)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__Parameter__Entit__32AB8735");

        });

        modelBuilder.Entity<ParameterComputedValue>(entity =>
        {
            entity.HasKey(e => e.ComputedValueId);
            entity.ToTable("ParameterComputedValue");

            entity.Property(e => e.ComputedValue)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasOne(e => e.Parameter)
                  .WithMany(p => p.ComputedValues)
                  .HasForeignKey(e => e.ParameterId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_ParameterComputedValue_Parameter_33D4B5983M");
        });

        modelBuilder.Entity<ParameterBinding>(entity =>
        {
            entity.ToTable("ParameterBinding");
            entity.HasIndex(e => e.TenantId, "IX_ParameterBinding_TenantId");

            entity.HasKey(e => e.Id);

            entity.HasOne(pb => pb.SystemParameter)
                  .WithMany(sp => sp.ParameterBindings)
                  .HasForeignKey(pb => pb.SystemParameterId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(pb => pb.MappedParameter)
                  .WithMany()
                  .HasForeignKey(pb => pb.MappedParameterId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ParamtersMap>(entity =>
        {
            entity.HasKey(e => new { e.Apiid, e.NodeId, e.ParameterId }).HasName("PK__Paramter__FB8F9E96CAE2A0AB");

            entity.ToTable("ParamtersMap");

            entity.Property(e => e.Apiid).HasColumnName("APIId");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Xmlnode)
                .HasMaxLength(50)
                .HasColumnName("XMLNode");
            entity.Property(e => e.Xmlparent)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("XMLParent");

            entity.HasOne(d => d.Api).WithMany(p => p.ParamtersMaps)
                .HasForeignKey(d => d.Apiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Paramters__APIId__339FAB6E");

            entity.HasOne(d => d.MapFunction).WithMany(p => p.ParamtersMaps)
                .HasForeignKey(d => d.MapFunctionId)
                .HasConstraintName("FK__Paramters__MapFu__3493CFA7");

            entity.HasOne(d => d.Node).WithMany(p => p.ParamtersMaps)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Paramters__NodeI__3587F3E0");

            entity.HasOne(d => d.Parameter).WithMany(p => p.ParamtersMaps)
                .HasForeignKey(d => d.ParameterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Paramters__Param__367C1819");
        });

        modelBuilder.Entity<Pcard>(entity =>
        {
            entity.HasKey(e => e.PcardId).HasName("PK__PCards__55754A82B7BE54A4");

            entity.ToTable("PCards");

            entity.HasIndex(e => e.TenantId, "IX_PCards_TenantId");

            entity.HasIndex(e => e.ProductId, "UK_PCards").IsUnique();

            entity.Property(e => e.PcardId).HasColumnName("PCardId");
            //entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Expression)
                .HasDefaultValue("")
                .HasColumnType("text");
            entity.Property(e => e.Expshown).HasColumnType("longtext");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.PcardDesc)
                .HasMaxLength(50)
                .HasColumnName("PCardDesc");
            entity.Property(e => e.PcardName)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("PCardName");
            entity.Property(e => e.Pstatus)
                .HasMaxLength(10)
                .HasColumnName("PStatus");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Pcards)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__PCards__TenantId__395884C4");

            entity.HasOne(d => d.Product).WithOne(p => p.Pcard)
                .HasForeignKey<Pcard>(d => d.ProductId)
                .HasConstraintName("FK__PCards__ProductI__3A4CA8FD");

            entity.HasMany(d => d.Ecards).WithMany(p => p.Pcards)
                .UsingEntity<Dictionary<string, object>>(
                    "PcardDetail",
                    r => r.HasOne<Ecard>().WithMany()
                        .HasForeignKey("EcardId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PCardDeta__ECard__37703C52"),
                    l => l.HasOne<Pcard>().WithMany()
                        .HasForeignKey("PcardId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PCardDeta__PCard__3864608B"),
                    j =>
                    {
                        j.HasKey("PcardId", "EcardId").HasName("PK__PCardDet__2E3C4FC4827D6CE6");
                        j.ToTable("PCardDetails");
                        j.IndexerProperty<int>("PcardId").HasColumnName("PCardId");
                        j.IndexerProperty<int>("EcardId").HasColumnName("ECardId");
                    });
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__B40CC6CD145C7B63");

            entity.ToTable("Product");

            entity.HasIndex(e => e.TenantId, "IX_Product_TenantId");

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Description).HasColumnType("nvarchar(50)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.MaxEligibleAmount).HasColumnType("decimal(18, 2)");
            //entity.Property(e => e.MimeType).HasMaxLength(20);
            entity.Property(e => e.Narrative).HasColumnType("longtext");
            entity.Property(e => e.ProductName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__Categor__3B40CD36");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Products)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__Product__EntityI__3C34F16F");
        });

        modelBuilder.Entity<ProductCap>(entity =>
        {
            entity.ToTable("ProductCap");

            entity.HasIndex(e => e.ProductId, "IX_ProductCap_ProductId");
            entity.HasIndex(e => e.TenantId, "IX_ProductCap_TenantId");

            entity.Property(e => e.ProductCapPercentage).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductId");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCaps).HasForeignKey(d => d.ProductId);
        });


        modelBuilder.Entity<ProductParam>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.ParameterId }).HasName("PK__ProductP__DB8C00EA175EE2AC");

            entity.ToTable("ProductParam");

            entity.HasIndex(e => e.TenantId, "IX_ProductParam_TenantId");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ParamValue)
                .HasMaxLength(50)
                .HasColumnName("paramValue");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(d => d.Parameter).WithMany(p => p.ProductParams)
                .HasForeignKey(d => d.ParameterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductPa__Param__3E1D39E1");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductParams)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductPa__Produ__3F115E1A");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissions__8AFACE1AD71FF567");
             entity.HasIndex(r => r.PermissionId)
                .HasDatabaseName("IX_Permission_PermissionId");
        
              
            entity.Property(e => e.PermissionId).ValueGeneratedNever();
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.PermissionAction).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasKey(e => e.ScreenId).HasName("PK__Screen__0AB60F85B6D8C93E");

            entity.ToTable("Screen");

            entity.Property(e => e.ScreenId).HasColumnName("ScreenID");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.ScreenName)
                .HasMaxLength(60)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SecurityRole>(entity =>
        {
            entity.ToTable("SecurityRoles");
            entity.HasKey(e => e.RoleId).HasName("PK__Security__149AF30AF87E70F1");
            entity.HasIndex(e => e.TenantId, "IX_SecurityRole_TenantId");

            entity.Property(e => e.RoleId).HasColumnName("RoleId");
            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.RoleDesc).HasMaxLength(250);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_Settings_TenantId");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Settings).HasForeignKey(d => d.TenantId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C14E89488");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.TenantId, "IX_Users_TenantId");

            entity.HasIndex(e => e.KeycloakUserId, "IX_Users_KeycloakUserId").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasDefaultValue("");
            //entity.Property(e => e.Issuspended).HasColumnName("issuspended");
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            //entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.KeycloakUserId)
                .HasMaxLength(50)
                .HasDefaultValue("");
            //entity.Property(e => e.MimeType).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(50);
            //entity.Property(e => e.SuspentionDate).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            //entity.Property(e => e.UserPassword)
            //    .HasMaxLength(255)
            //    .HasDefaultValue("");

            //entity.HasOne(d => d.Entity).WithMany(p => p.Users)
            //    .HasForeignKey(d => d.TenantId)
            //    .HasConstraintName("FK__Users__TenantId__41EDCAC5");

            entity.HasOne(d => d.Status).WithMany(p => p.Users)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__Users__StatusId__42E1EEFE");

            //entity.HasMany(d => d.Roles).WithMany(p => p.Users)
            //    .UsingEntity<UserRole>(
            //        r => r.HasOne<SecurityRole>(d => d.Role).WithMany(p => p.UserRoles).HasForeignKey(d => d.RoleId),
            //        l => l.HasOne<User>(d => d.User).WithMany(p => p.UserRoles).HasForeignKey(d => d.UserId),
            //        j =>
            //        {
            //            j.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__A6C1637AF327336E");
            //            j.ToTable("UserRoles");
            //        });
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__A6C1637AF327336E");
            entity.HasIndex(e => e.TenantId, "IX_UserRole_TenantId");

            entity.Property(e => e.CreatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(ug => ug.UserId)
            .HasDatabaseName("IX_UserRole_UserId");
            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRole__Role__40058253");

            //entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
            //    .HasForeignKey(d => d.UserId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK__UserRole__UserI__40F9A68C");
        });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__UserStat__C8EE2063EFEE8122");

            entity.ToTable("UserStatus");

            entity.Property(e => e.StatusId).ValueGeneratedNever();
            entity.Property(e => e.UpdatedByDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });
        modelBuilder.Entity<ProductCapAmount>(entity =>
        {
            entity.ToTable("ProductCapAmount");
            entity.HasIndex(e => e.TenantId, "IX_ProductCapAmount_TenantId");
            entity.HasIndex(e => e.ProductId, "IX_ProductCapAmount_ProductId");

            entity.HasKey(e => e.Id);
            entity.HasOne(pca => pca.Product)
            .WithMany(p => p.ProductCapAmounts)
            .HasForeignKey(pca => pca.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Amount)
      .HasPrecision(18, 4); // Example: 18 total digits, 4 after decimal

            entity.Property(e => e.MaxCapPerStream)
                .HasPrecision(18, 4);

        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
