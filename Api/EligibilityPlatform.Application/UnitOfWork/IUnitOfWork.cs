using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Repository.MEligibilityPlatform.Application.Repository;

namespace MEligibilityPlatform.Application.UnitOfWork
{
    /// <summary>
    /// Defines the unit of work pattern for managing repositories and database transactions.
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the API detail repository.
        /// </summary>
        IApiDetailRepository ApiDetailRepository { get; }

        /// <summary>
        /// Gets the category repository.
        /// </summary>
        ICategoryRepository CategoryRepository { get; }

        /// <summary>
        /// Gets the city repository.
        /// </summary>
        ICityRepository CityRepository { get; }

        /// <summary>
        /// Gets the condition repository.
        /// </summary>
        IConditionRepository ConditionRepository { get; }

        /// <summary>
        /// Gets the country repository.
        /// </summary>
        ICountryRepository CountryRepository { get; }

        /// <summary>
        /// Gets the entity repository.
        /// </summary>
        IEntityRepository EntityRepository { get; }

        /// <summary>
        /// Gets the bulk import repository.
        /// </summary>
        IBulkImportRepository BulkImportRepository { get; }

        /// <summary>
        /// Gets the parameter repository.
        /// </summary>
        IParameterRepository ParameterRepository { get; }

        /// <summary>
        /// Gets the currency repository.
        /// </summary>
        ICurrencyRepository CurrencyRepository { get; }

        /// <summary>
        /// Gets the data type repository.
        /// </summary>
        IDataTypeRepository DataTypeRepository { get; }

        /// <summary>
        /// Gets the E-card repository.
        /// </summary>
        IEcardRepository EcardRepository { get; }

        /// <summary>
        /// Gets the E-rule repository.
        /// </summary>
        IEruleRepository EruleRepository { get; }

        /// <summary>
        /// Gets the factor repository.
        /// </summary>
        IFactorRepository FactorRepository { get; }

        /// <summary>
        /// Gets the history E-card repository.
        /// </summary>
        IHistoryEcRepository HistoryEcRepository { get; }

        /// <summary>
        /// Gets the history E-rule repository.
        /// </summary>
        IHistoryErRepository HistoryErRepository { get; }

        /// <summary>
        /// Gets the history parameter repository.
        /// </summary>
        IHistoryParameterRepository HistoryParameterRepository { get; }

        /// <summary>
        /// Gets the history P-card repository.
        /// </summary>
        IHistoryPcRepository HistoryPcRepository { get; }

        /// <summary>
        /// Gets the list item repository.
        /// </summary>
        IListItemRepository ListItemRepository { get; }

        /// <summary>
        /// Gets the managed list repository.
        /// </summary>
        IManagedListRepository ManagedListRepository { get; }

        /// <summary>
        /// Gets the mapping function repository.
        /// </summary>
        IMappingFunctionRepository MappingFunctionRepository { get; }

        /// <summary>
        /// Gets the node model repository.
        /// </summary>
        INodeModelRepository NodeModelRepository { get; }

        /// <summary>
        /// Gets the node API repository.
        /// </summary>
        INodeApiRepository NodeApiRepository { get; }

        /// <summary>
        /// Gets the parameters map repository.
        /// </summary>
        IParamtersMapRepository ParamtersMapRepository { get; }

        /// <summary>
        /// Gets the P-card repository.
        /// </summary>
        IPcardRepository PcardRepository { get; }

        /// <summary>
        /// Gets the product repository.
        /// </summary>
        IProductRepository ProductRepository { get; }

        /// <summary>
        /// Gets the product parameter repository.
        /// </summary>
        IProductParamRepository ProductParamRepository { get; }

        /// <summary>
        /// Gets the user repository.
        /// </summary>
        IUserRepository UserRepository { get; }

        /// <summary>
        /// Gets the security group repository.
        /// </summary>
        ISecurityGroupRepository SecurityGroupRepository { get; }

        /// <summary>
        /// Gets the user group repository.
        /// </summary>
        IUserGroupRepository UserGroupRepository { get; }

        /// <summary>
        /// Gets the screen repository.
        /// </summary>
        IScreenRepository ScreenRepository { get; }

        /// <summary>
        /// Gets the role repository.
        /// </summary>
        IRoleRepository RoleRepository { get; }

        /// <summary>
        /// Gets the group role repository.
        /// </summary>
        IGroupRoleRepository GroupRoleRepository { get; }

        /// <summary>
        /// Gets the user status repository.
        /// </summary>
        IUserStatusRepository UserStatusRepository { get; }

        /// <summary>
        /// Gets the audit repository.
        /// </summary>
        IAuditRepository AuditRepository { get; }

        /// <summary>
        /// Gets the maker-checker repository.
        /// </summary>
        IMakerCheckerRepository MakerCheckerRepository { get; }

        /// <summary>
        /// Gets the setting repository.
        /// </summary>
        ISettingRepository SettingRepository { get; }

        /// <summary>
        /// Gets the import document history repository.
        /// </summary>
        IImportDocumentRepository ImportDocumentHistoryRepository { get; }

        /// <summary>
        /// Gets the application setting repository.
        /// </summary>
        IAppSettingRepository AppSettingRepository { get; }

        /// <summary>
        /// Gets the exception management repository.
        /// </summary>
        IExceptionManagementRepository ExceptionManagementRepository { get; }

        /// <summary>
        /// Gets the exception product repository.
        /// </summary>
        IExceptionProductRepository ExceptionProductRepository { get; }

        /// <summary>
        /// Gets the amount eligibility repository.
        /// </summary>
        IAmountEligibilityRepository AmountEligibilityRepository { get; }

        /// <summary>
        /// Gets the API parameters repository.
        /// </summary>
        IApiParametersRepository ApiParametersRepository { get; }

        /// <summary>
        /// Gets the API responses repository.
        /// </summary>
        IApiResponsesRepository ApiResponsesRepository { get; }

        /// <summary>
        /// Gets the API parameter maps repository.
        /// </summary>
        IApiParameterMapsRepository ApiParameterMapsRepository { get; }

        /// <summary>
        /// Gets the product cap repository.
        /// </summary>
        IProductCapRepository ProductCapRepository { get; }

        /// <summary>
        /// Gets the evaluation history repository.
        /// </summary>
        IEvaluationHistoryRepository EvaluationHistoryRepository { get; }

        /// <summary>
        /// Gets the E-rule master repository.
        /// </summary>
        IEruleMasterRepository EruleMasterRepository { get; }

        /// <summary>
        /// Gets the product cap amount repository.
        /// </summary>
        IProductCapAmountRepository ProductCapAmountRepository { get; }

        /// <summary>
        /// Gets the log repository.
        /// </summary>
        ILogRepository LogRepository { get; }
        IIntegrationApiEvaluationRepository IntegrationApiEvaluationRepository { get; }
        IRejectionReasonRepository RejectionReasonRepository { get; }
        ISystemParameterRepository SystemParameterRepository { get; }
        IParameterBindingRepository ParameterBindingRepository { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously completes the unit of work transaction by saving all changes to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CompleteAsync();

        #endregion
    }
}
