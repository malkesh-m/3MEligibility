using System.Net.Http;
using System.Text.RegularExpressions;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Repository.MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using IIntegrationApiEvaluationRepository = MEligibilityPlatform.Application.Repository.IIntegrationApiEvaluationRepository;

namespace MEligibilityPlatform.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of Work implementation that provides access to all repositories and manages database transactions.
    /// Implements the <see cref="IUnitOfWork"/> interface for coordinating work across multiple repositories.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </remarks>
    /// <param name="dbContext">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    /// <exception cref="ArgumentNullException">Thrown when dbContext or httpContext is null.</exception>
    public partial class UnitOfWork(EligibilityDbContext dbContext, IHttpContextAccessor httpContext) : IUnitOfWork
    {
        #region Properties
        private CategoryRepository? _categoryRepository;
        private UserRepository? _userRepository;
        private SecurityGroupRepository? _securityGroupRepository;
        private UserGroupRepository? _userGroupRepository;
        private ScreenRepository? _screenRepository;
        private RoleRepository? _roleRepository;
        private GroupRoleRepository? _groupRoleRepository;
        private UserStatusRepository? _userStatusRepository;
        private BulkImportRepository? _bulkImportRepository;

        /// <summary>
        /// Gets the bulk import repository.
        /// </summary>
        public IBulkImportRepository BulkImportRepository => _bulkImportRepository ??= new BulkImportRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the category repository.
        /// </summary>
        public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the user repository.
        /// </summary>
        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the security group repository.
        /// </summary>
        public ISecurityGroupRepository SecurityGroupRepository => _securityGroupRepository ??= new SecurityGroupRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the user group repository.
        /// </summary>
        public IUserGroupRepository UserGroupRepository => _userGroupRepository ??= new UserGroupRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the screen repository.
        /// </summary>
        public IScreenRepository ScreenRepository => _screenRepository ??= new ScreenRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the role repository.
        /// </summary>
        public IRoleRepository RoleRepository => _roleRepository ??= new RoleRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the group role repository.
        /// </summary>
        public IGroupRoleRepository GroupRoleRepository => _groupRoleRepository ??= new GroupRoleRepository(_dbContext, _httpContext);

        /// <summary>
        /// Gets the user status repository.
        /// </summary>
        public IUserStatusRepository UserStatusRepository => _userStatusRepository ??= new UserStatusRepository(_dbContext, _httpContext);

        private ApiDetailRepository? _apiDetailRepository;

        /// <summary>
        /// Gets the API detail repository.
        /// </summary>
        public IApiDetailRepository ApiDetailRepository => _apiDetailRepository ??= new ApiDetailRepository(_dbContext, _httpContext);

        private CityRepository? _cityRepository;

        /// <summary>
        /// Gets the city repository.
        /// </summary>
        public ICityRepository CityRepository => _cityRepository ??= new CityRepository(_dbContext, _httpContext);

        private AuditRepository? _auditRepository;

        /// <summary>
        /// Gets the audit repository.
        /// </summary>
        public IAuditRepository AuditRepository => _auditRepository ??= new AuditRepository(_dbContext, _httpContext);

        private ConditionRepository? _conditionRepository;

        /// <summary>
        /// Gets the condition repository.
        /// </summary>
        public IConditionRepository ConditionRepository => _conditionRepository ??= new ConditionRepository(_dbContext, _httpContext);

        private CountryRepository? _countryRepository;

        /// <summary>
        /// Gets the country repository.
        /// </summary>
        public ICountryRepository CountryRepository => _countryRepository ??= new CountryRepository(_dbContext, _httpContext);

        private EntityRepository? _entityRepository;

        /// <summary>
        /// Gets the entity repository.
        /// </summary>
        public IEntityRepository EntityRepository => _entityRepository ??= new EntityRepository(_dbContext, _httpContext);

        private ParameterRepository? _parameterRepository;

        /// <summary>
        /// Gets the parameter repository.
        /// </summary>
        public IParameterRepository ParameterRepository => _parameterRepository ??= new ParameterRepository(_dbContext, _httpContext);

        private CurrencyRepository? _currencyRepository;

        /// <summary>
        /// Gets the currency repository.
        /// </summary>
        public ICurrencyRepository CurrencyRepository => _currencyRepository ??= new CurrencyRepository(_dbContext, _httpContext);

        private DataTypeRepository? _daTypeRepository;

        /// <summary>
        /// Gets the data type repository.
        /// </summary>
        public IDataTypeRepository DataTypeRepository => _daTypeRepository ??= new DataTypeRepository(_dbContext, _httpContext);

        private EcardRepository? _ecardRepository;

        /// <summary>
        /// Gets the e-card repository.
        /// </summary>
        public IEcardRepository EcardRepository => _ecardRepository ??= new EcardRepository(_dbContext, _httpContext);

        private EruleRepository? _eruleRepository;

        /// <summary>
        /// Gets the e-rule repository.
        /// </summary>
        public IEruleRepository EruleRepository => _eruleRepository ??= new EruleRepository(_dbContext, _httpContext);

        private FactorRepository? _factorRepository;

        /// <summary>
        /// Gets the factor repository.
        /// </summary>
        public IFactorRepository FactorRepository => _factorRepository ??= new FactorRepository(_dbContext, _httpContext);

        private HistoryEcRepository? _historyEcRepository;

        /// <summary>
        /// Gets the history EC repository.
        /// </summary>
        public IHistoryEcRepository HistoryEcRepository => _historyEcRepository ??= new HistoryEcRepository(_dbContext, _httpContext);

        private HistoryErRepository? _historyErRepository;

        /// <summary>
        /// Gets the history ER repository.
        /// </summary>
        public IHistoryErRepository HistoryErRepository => _historyErRepository ??= new HistoryErRepository(_dbContext, _httpContext);

        private HistoryParameterRepository? _historyParameterRepository;

        /// <summary>
        /// Gets the history parameter repository.
        /// </summary>
        public IHistoryParameterRepository HistoryParameterRepository => _historyParameterRepository ??= new HistoryParameterRepository(_dbContext, _httpContext);

        private HistoryPcRepository? _historyPcRepository;

        /// <summary>
        /// Gets the history PC repository.
        /// </summary>
        public IHistoryPcRepository HistoryPcRepository => _historyPcRepository ??= new HistoryPcRepository(_dbContext, _httpContext);

        private ListItemRepository? _listItemRepository;

        /// <summary>
        /// Gets the list item repository.
        /// </summary>
        public IListItemRepository ListItemRepository => _listItemRepository ??= new ListItemRepository(_dbContext, _httpContext);

        private ManagedListRepository? _managedListRepository;

        /// <summary>
        /// Gets the managed list repository.
        /// </summary>
        public IManagedListRepository ManagedListRepository => _managedListRepository ??= new ManagedListRepository(_dbContext, _httpContext);

        private MappingFunctionRepository? _mappingFunctionRepository;

        /// <summary>
        /// Gets the mapping function repository.
        /// </summary>
        public IMappingFunctionRepository MappingFunctionRepository => _mappingFunctionRepository ??= new MappingFunctionRepository(_dbContext, _httpContext);

        private NodeRepository? _nodeRepository;

        /// <summary>
        /// Gets the node model repository.
        /// </summary>
        public INodeModelRepository NodeModelRepository => _nodeRepository ??= new NodeRepository(_dbContext, _httpContext);

        private NodeApiRepository? _nodeApiRepository;

        /// <summary>
        /// Gets the node API repository.
        /// </summary>
        public INodeApiRepository NodeApiRepository => _nodeApiRepository ??= new NodeApiRepository(_dbContext, _httpContext);

        private ParamtersMapRepository? _paramtersMapRepository;

        /// <summary>
        /// Gets the parameters map repository.
        /// </summary>
        public IParamtersMapRepository ParamtersMapRepository => _paramtersMapRepository ??= new ParamtersMapRepository(_dbContext, _httpContext);

        private PcardRepository? _pcardRepository;

        /// <summary>
        /// Gets the PCard repository.
        /// </summary>
        public IPcardRepository PcardRepository => _pcardRepository ??= new PcardRepository(_dbContext, _httpContext);

        private ProductRepository? _productRepository;

        /// <summary>
        /// Gets the product repository.
        /// </summary>
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_dbContext, _httpContext);

        private ProductparamRepository? _productparamRepository;

        /// <summary>
        /// Gets the product parameter repository.
        /// </summary>
        public IProductParamRepository ProductParamRepository => _productparamRepository ??= new ProductparamRepository(_dbContext, _httpContext);

        private MakerCheckerRepository? _makercheckerRepository;

        /// <summary>
        /// Gets the maker-checker repository.
        /// </summary>
        public IMakerCheckerRepository MakerCheckerRepository => _makercheckerRepository ??= new MakerCheckerRepository(_dbContext, _httpContext);

        private SettingRepository? _settingRepository;

        /// <summary>
        /// Gets the setting repository.
        /// </summary>
        public ISettingRepository SettingRepository => _settingRepository ??= new SettingRepository(_dbContext, _httpContext);

        private ImportDocumentRepository? _ImportDocumentHistoryRepository;

        /// <summary>
        /// Gets the import document repository.
        /// </summary>
        public IImportDocumentRepository ImportDocumentHistoryRepository => _ImportDocumentHistoryRepository ??= new ImportDocumentRepository(_dbContext, _httpContext);

        private AppSettingRepository? _appSettingRepository;

        /// <summary>
        /// Gets the application setting repository.
        /// </summary>
        public IAppSettingRepository AppSettingRepository => _appSettingRepository ??= new AppSettingRepository(_dbContext, _httpContext);

        private ExceptionManagementRepository? _exceptionManagementRepository;

        /// <summary>
        /// Gets the exception management repository.
        /// </summary>
        public IExceptionManagementRepository ExceptionManagementRepository => _exceptionManagementRepository ??= new ExceptionManagementRepository(_dbContext, _httpContext);

        private AmountEligibilityRepository? _AmountEligibilityRepository;

        /// <summary>
        /// Gets the amount eligibility repository.
        /// </summary>
        public IAmountEligibilityRepository AmountEligibilityRepository => _AmountEligibilityRepository ??= new AmountEligibilityRepository(_dbContext, _httpContext);

        private ApiResponsesRepository? _ApiResponsesRepository;

        /// <summary>
        /// Gets the API responses repository.
        /// </summary>
        public IApiResponsesRepository ApiResponsesRepository => _ApiResponsesRepository ??= new ApiResponsesRepository(_dbContext, _httpContext);

        private ApiParametersRepository? _ApiParametersRepository;

        /// <summary>
        /// Gets the API parameters repository.
        /// </summary>
        public IApiParametersRepository ApiParametersRepository => _ApiParametersRepository ??= new ApiParametersRepository(_dbContext, _httpContext);

        private ApiParameterMapsRepository? _ApiParameterMapsRepository;

        /// <summary>
        /// Gets the API parameter maps repository.
        /// </summary>
        public IApiParameterMapsRepository ApiParameterMapsRepository => _ApiParameterMapsRepository ??= new ApiParameterMapsRepository(_dbContext, _httpContext);

        private ProductCapRepostitory? _ProductCapRepostitory;

        /// <summary>
        /// Gets the product cap repository.
        /// </summary>
        public IProductCapRepository ProductCapRepository => _ProductCapRepostitory ??= new ProductCapRepostitory(_dbContext, _httpContext);

        private ExceptionProductRepository? _ExceptionProductRepostitory;

        /// <summary>
        /// Gets the exception product repository.
        /// </summary>
        public IExceptionProductRepository ExceptionProductRepository => _ExceptionProductRepostitory ??= new ExceptionProductRepository(_dbContext, _httpContext);

        private EvaluationHistoryRepository? _EvaluationHistoryRepository;

        /// <summary>
        /// Gets the evaluation history repository.
        /// </summary>
        public IEvaluationHistoryRepository EvaluationHistoryRepository => _EvaluationHistoryRepository ??= new EvaluationHistoryRepository(_dbContext, _httpContext);

        private EruleMasterRepository? _EruleMasterRepository;

        /// <summary>
        /// Gets the e-rule master repository.
        /// </summary>
        public IEruleMasterRepository EruleMasterRepository => _EruleMasterRepository ??= new EruleMasterRepository(_dbContext, _httpContext);
        #endregion

        private ProductCapAmountRepository? _ProductCapAmountRepository;

        /// <summary>
        /// Gets the product cap amount repository.
        /// </summary>
        public IProductCapAmountRepository ProductCapAmountRepository => _ProductCapAmountRepository ??= new ProductCapAmountRepository(_dbContext, _httpContext);

        private LogRepository? _LogRepository;

        /// <summary>
        /// Gets the log repository.
        /// </summary>
        public ILogRepository LogRepository => _LogRepository ??= new LogRepository(_dbContext, _httpContext);
        private IntegrationApiEvaluationRepository? _integrationApiEvaluationRepository;

        /// <summary>
        /// Gets the product cap amount repository.
        /// </summary>
        public IIntegrationApiEvaluationRepository IntegrationApiEvaluationRepository => _integrationApiEvaluationRepository ??= new IntegrationApiEvaluationRepository(_dbContext, _httpContext);

        private RejectionReasonRepository? _rejectionReasonRepository;

        /// <summary>
        /// Gets the product cap amount repository.
        /// </summary>
        public IRejectionReasonRepository RejectionReasonRepository => _rejectionReasonRepository ??= new RejectionReasonRepository(_dbContext, _httpContext);

        private SystemParameterRepository? _systemParameterRepository;

        /// <summary>
        /// Gets the source parameter repository.
        /// </summary>
        public ISystemParameterRepository SystemParameterRepository => _systemParameterRepository ??= new SystemParameterRepository(_dbContext, _httpContext);

        private ParameterBindingRepository? _parameterBindingRepository;

        /// <summary>
        /// Gets the parameter binding repository.
        /// </summary>
        public IParameterBindingRepository ParameterBindingRepository => _parameterBindingRepository ??= new ParameterBindingRepository(_dbContext, _httpContext);

        #region Readonlys

        private readonly EligibilityDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly IHttpContextAccessor _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

        #endregion

        #region Methods

        /// <summary>
        /// Completes the unit of work by saving all changes to the database.
        /// Handles maker-checker pattern updates for newly added entities.
        /// </summary>
        /// <exception cref="Exception">Thrown when database update fails.</exception>
        public async Task CompleteAsync()
        {
            var addedEntries = _dbContext.ChangeTracker.Entries()
                      .Where(e => e.State == EntityState.Added)
            .ToList();
            var httpContext = _dbContext.GetService<IHttpContextAccessor>()?.HttpContext;
            var controller = httpContext?.GetRouteData()?.Values["controller"]?.ToString();
            var action = httpContext?.GetRouteData()?.Values["action"]?.ToString();
            bool isChecker = controller == "MakerChecker" && action == "StatusUpdate";


            try
            {
                if (isChecker)
                {
                    await _dbContext.SaveChangesAsync();


                    foreach (var entry in addedEntries)
                    {
                        var entity = entry.Entity;
                        var entityType = _dbContext.Model.FindEntityType(entity.GetType());
                        var primaryKey = entityType?.FindPrimaryKey()?.Properties[0];

                        if (primaryKey != null)
                        {
                            var property = entity.GetType().GetProperty(primaryKey.Name);
                            var recordId = property?.GetValue(entity);

                            if (recordId != null && int.TryParse(recordId.ToString(), out int parsedId))
                            {
                                var makerCheckerEntry = _dbContext.Set<MakerChecker>()
                                    .Where(m => m.TableName == entity.GetType().Name && m.RecordId == 0)
                                    .OrderByDescending(m => m.MakerCheckerId)
                                    .FirstOrDefault();

                                if (makerCheckerEntry != null)
                                {
                                    makerCheckerEntry.RecordId = parsedId;
                                    _dbContext.Update(makerCheckerEntry);
                                }
                            }
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("REFERENCE"))
                {
                    var match = MySqlTableRegex().Match(ex.InnerException.Message);
                    var dependentTable = match.Success ? match.Groups[1].Value : "UnknownDependentTable";

                    var principalEntry = ex.Entries?.FirstOrDefault();
                    var principalTable = principalEntry != null
                        ? _dbContext.Model
                            .FindEntityType(principalEntry.Entity.GetType())
                            ?.GetTableName()
                        : "Unknown";

                    throw new InvalidOperationException(
                        $"Cannot delete from '{principalTable}' because it is referenced by '{dependentTable}'."
                    );
                }


                throw new Exception("Database update failed: " + ex.Message, ex);
            }
        }
        #endregion

        #region Implements IDisposable

        #region Private Dispose Fields

        private bool _disposed;

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _dbContext.Dispose(); // fallback for legacy sync disposal
            GC.SuppressFinalize(this);

        }

        /// <summary>
        /// Asynchronously performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously releases the unmanaged resources used by the UnitOfWork and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    await _dbContext.DisposeAsync();
                }
                _disposed = true;
            }
        }
        [GeneratedRegex(@"`[^`]+`\.`([^`]+)`", RegexOptions.IgnoreCase)]
        private static partial Regex MySqlTableRegex();
        [GeneratedRegex(@"constraint ""?([^\\""]+)""?", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex1();
        #endregion
    }
}
