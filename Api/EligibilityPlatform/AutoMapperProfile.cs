using AutoMapper;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
namespace EligibilityPlatform
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Configures bidirectional mapping between Apidetail entity and ApiDetailListModel
            CreateMap<Apidetail, ApiDetailListModel>().ReverseMap();
            // Configures bidirectional mapping between Apidetail entity and ApiDetailCreateotUpdateModel
            CreateMap<Apidetail, ApiDetailCreateotUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between Category entity and CategoryModel
            CreateMap<Category, CategoryModel>().ReverseMap();
            // Configures bidirectional mapping between Category entity and CategoryCreateUpdateModel
            CreateMap<Category, CategoryCreateUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between Category entity and CategoryListModel
            CreateMap<Category, CategoryListModel>().ReverseMap();

            // Configures bidirectional mapping between City entity and CityModel
            CreateMap<City, CityModel>().ReverseMap();
            // Configures bidirectional mapping between Audit entity and AuditModel
            CreateMap<Audit, AuditModel>().ReverseMap();
            CreateMap<Audit, AuditCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between Condition entity and ConditionModel
            CreateMap<Condition, ConditionModel>().ReverseMap();
            // Configures bidirectional mapping between Country entity and CountryModel
            CreateMap<Country, CountryModel>().ReverseMap();

            // Configures bidirectional mapping between Entity entity and EntityModel
            CreateMap<Entity, EntityModel>().ReverseMap();
            // Configures bidirectional mapping between Entity entity and CreateOrUpdateEntityModel
            CreateMap<Entity, CreateOrUpdateEntityModel>().ReverseMap();

            // Configures bidirectional mapping between Parameter entity and ParameterModel
            CreateMap<Parameter, ParameterModel>().ReverseMap();
            // Configures bidirectional mapping between ParameterComputedValue entity and ParameterComputedValueModel
            CreateMap<ParameterComputedValue, ParameterComputedValueModel>().ReverseMap();
            // Configures bidirectional mapping between Parameter entity and ParameterAddUpdateModel
            CreateMap<Parameter, ParameterAddUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between Parameter entity and ParameterListModel
            CreateMap<Parameter, ParameterListModel>().ReverseMap();

            // Configures bidirectional mapping between Currency entity and CurrencyModel
            CreateMap<Currency, CurrencyModel>().ReverseMap();
            // Configures bidirectional mapping between DataType entity and DataTypeModel
            CreateMap<DataType, DataTypeModel>().ReverseMap();

            // Configures bidirectional mapping between Ecard entity and EcardModel
            CreateMap<Ecard, EcardModel>().ReverseMap();
            // Configures bidirectional mapping between Ecard entity and EcardAddUpdateModel
            CreateMap<Ecard, EcardAddUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between Ecard entity and EcardListModel
            CreateMap<Ecard, EcardListModel>().ReverseMap();

            // Configures bidirectional mapping between Erule entity and EruleModel
            CreateMap<Erule, EruleModel>().ReverseMap();
            // Configures bidirectional mapping between Erule entity and EruleListModel
            CreateMap<Erule, EruleListModel>().ReverseMap();
            // Configures bidirectional mapping between Erule entity and EruleCreateOrUpdateModel
            CreateMap<Erule, EruleCreateOrUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between Erule entity and EruleCreateModel
            CreateMap<Erule, EruleCreateModel>().ReverseMap();
            // Configures bidirectional mapping between Erule entity and EruleUpdateModel
            CreateMap<Erule, EruleUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between User entity and UserModel
            CreateMap<User, UserModel>().ReverseMap();
            // Configures bidirectional mapping between UserEditModel and User entity
            CreateMap<UserEditModel, User>().ReverseMap();
            // Configures bidirectional mapping between UserPictureModel and User entity
            CreateMap<UserPictureModel, User>().ReverseMap();
            // Configures bidirectional mapping between User entity and UserEditModel
            CreateMap<User, UserEditModel>().ReverseMap();

            // Configures bidirectional mapping between SecurityGroup entity and SecurityGroupModel
            CreateMap<SecurityGroup, SecurityGroupModel>().ReverseMap();
            CreateMap<SecurityGroup, SecurityGroupUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between UserGroup entity and UserGroupModel
            CreateMap<UserGroup, UserGroupModel>().ReverseMap();
            CreateMap<UserGroup, UserGroupCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between Screen entity and ScreenModel
            CreateMap<Screen, ScreenModel>().ReverseMap();
            // Configures bidirectional mapping between Role entity and RoleModel
            CreateMap<Role, RoleModel>().ReverseMap();
            CreateMap<Role, RoleCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between GroupRole entity and GroupRoleModel
            CreateMap<GroupRole, GroupRoleModel>().ReverseMap();
            // Configures bidirectional mapping between UserStatus entity and UserStatusModel
            CreateMap<UserStatus, UserStatusModel>().ReverseMap();
            CreateMap<UserStatus, UserStatusAddModel>().ReverseMap();


            // Configures bidirectional mapping between Factor entity and FactorModel
            CreateMap<Factor, FactorModel>().ReverseMap();
            // Configures bidirectional mapping between Factor entity and FactorListModel
            CreateMap<Factor, FactorListModel>().ReverseMap();
            // Configures bidirectional mapping between Factor entity and FactorAddUpdateModel
            CreateMap<Factor, FactorAddUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between HistoryEc entity and HistoryEcModel
            CreateMap<HistoryEc, HistoryEcModel>().ReverseMap();
            // Configures bidirectional mapping between HistoryEr entity and HistoryErModel
            CreateMap<HistoryEr, HistoryErModel>().ReverseMap();
            // Configures bidirectional mapping between HistoryParameter entity and HistoryParameterModel
            CreateMap<HistoryParameter, HistoryParameterModel>().ReverseMap();
            // Configures bidirectional mapping between HistoryPc entity and HistoryPcModel
            CreateMap<HistoryPc, HistoryPcModel>().ReverseMap();

            // Configures bidirectional mapping between ListItem entity and ListItemModel
            CreateMap<ListItem, ListItemModel>().ReverseMap();
            CreateMap<ListItem, ListItemCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between ManagedList entity and ManagedListModel
            CreateMap<ManagedList, ManagedListModel>().ReverseMap();
            // Configures bidirectional mapping between ManagedList entity and ManagedListAddUpdateModel
            CreateMap<ManagedList, ManagedListAddUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between ManagedList entity and ManagedListGetModel
            CreateMap<ManagedList, ManagedListGetModel>().ReverseMap();

            // Configures bidirectional mapping between MappingFunction entity and MappingFunctionModel
            CreateMap<MappingFunction, MappingFunctionModel>().ReverseMap();

            // Configures bidirectional mapping between Node entity and NodeListModel
            CreateMap<Node, NodeListModel>().ReverseMap();
            // Configures bidirectional mapping between Node entity and NodeCreateUpdateModel
            CreateMap<Node, NodeCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between NodeApi entity and NodeApiCreateOrUpdateModel
            CreateMap<NodeApi, NodeApiCreateOrUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between NodeApi entity and NodeApiListModel
            CreateMap<NodeApi, NodeApiListModel>().ReverseMap();

            // Configures bidirectional mapping between ParamtersMap entity and ParamtersMapModel
            CreateMap<ParamtersMap, ParamtersMapModel>().ReverseMap();

            // Configures bidirectional mapping between Pcard entity and PcardModel
            CreateMap<Pcard, PcardModel>().ReverseMap();
            // Configures bidirectional mapping between Pcard entity and PcardListModel
            CreateMap<Pcard, PcardListModel>().ReverseMap();
            // Configures bidirectional mapping between Pcard entity and PcardAddUpdateModel
            CreateMap<Pcard, PcardAddUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between Product entity and ProductModel
            CreateMap<Product, ProductModel>().ReverseMap();
            // Configures bidirectional mapping between Product entity and ProductListModel
            CreateMap<Product, ProductListModel>().ReverseMap();
            // Configures bidirectional mapping between Product entity and ProductAddUpdateModel
            CreateMap<Product, ProductAddUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between Product entity and ProductIdAndNameModel
            CreateMap<Product, ProductIdAndNameModel>().ReverseMap();

            // Configures bidirectional mapping between ProductParam entity and ProductParamModel
            CreateMap<ProductParam, ProductParamModel>().ReverseMap();

            // Configures bidirectional mapping between AppSetting entity and AppSettingModel
            CreateMap<AppSetting, AppSettingModel>().ReverseMap();

            // Configures bidirectional mapping between MakerChecker entity and MakerCheckerModelCopy
            CreateMap<MakerChecker, MakerCheckerModelCopy>().ReverseMap();
            // Configures unidirectional mapping from MakerCheckerModel to MakerChecker entity
            CreateMap<MakerCheckerModel, MakerChecker>();
            // Configures custom mapping from MakerChecker entity to MakerCheckerModel with additional property mappings
            CreateMap<MakerChecker, MakerCheckerModel>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString())) // Maps Status enum to StatusName string
            .ForMember(dest => dest.MakerName, opt => opt.MapFrom(src => src.Maker.UserName)); // Maps Maker's UserName to MakerName

            // Configures custom mapping from ProductParam entity to ProductParamListModel with navigation properties
            CreateMap<ProductParam, ProductParamListModel>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName)) // Maps Product navigation property to ProductName
            .ForMember(dest => dest.ParameterName, opt => opt.MapFrom(src => src.Parameter.ParameterName)); // Maps Parameter navigation property to ParameterName
            // Configures bidirectional mapping between ProductParam entity and ProductParamAddUpdateModel
            CreateMap<ProductParam, ProductParamAddUpdateModel>().ReverseMap();

            // Configures custom mapping from User entity to UserGetModel with navigation properties
            CreateMap<User, UserGetModel>()
                .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.Entity!.EntityName)) // Maps Entity navigation property to EntityName
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status!.StatusName)); // Maps Status navigation property to StatusName

            // Configures bidirectional mapping between ExceptionManagement entity and ExceptionManagementListModel
            CreateMap<ExceptionManagement, ExceptionManagementListModel>().ReverseMap();
            // Configures bidirectional mapping between ExceptionManagement entity and ExceptionManagementCreateOrUpdateModel
            CreateMap<ExceptionManagement, ExceptionManagementCreateOrUpdateModel>().ReverseMap();
            // Configures bidirectional mapping between ExceptionManagement entity and ExceptionManagementGetModel
            CreateMap<ExceptionManagement, ExceptionManagementGetModel>().ReverseMap();

            // Configures bidirectional mapping between AmountEligibilityModel and AmountEligibility entity
            CreateMap<AmountEligibilityModel, AmountEligibility>().ReverseMap();
            //CreateMap<ExceptionParameterModel, ExceptionParameter>().ReverseMap();

            // Configures bidirectional mapping between ApiResponse entity and ApiResponsesListModel
            CreateMap<ApiResponse, ApiResponsesListModel>().ReverseMap();
            // Configures bidirectional mapping between ApiResponse entity and ApiResponsesCreateUpdateModel
            CreateMap<ApiResponse, ApiResponsesCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between ApiParameter entity and ApiParametersListModel
            CreateMap<ApiParameter, ApiParametersListModel>().ReverseMap();
            // Configures bidirectional mapping between ApiParameter entity and ApiParametersCreateUpdateModel
            CreateMap<ApiParameter, ApiParametersCreateUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between ApiParameterMap entity and ApiParameterListMapModel
            CreateMap<ApiParameterMap, ApiParameterListMapModel>().ReverseMap();
            // Configures bidirectional mapping between ApiParameterMap entity and ApiParameterCreateUpdateMapModel
            CreateMap<ApiParameterMap, ApiParameterCreateUpdateMapModel>().ReverseMap();

            // Configures bidirectional mapping between ProductCap entity and ProductCapModel
            CreateMap<ProductCap, ProductCapModel>().ReverseMap();
            // Configures bidirectional mapping between EvaluationHistory entity and EvaluationHistoryModel
            CreateMap<EvaluationHistory, EvaluationHistoryModel>().ReverseMap();

            // Configures bidirectional mapping between EruleMaster entity and EruleMasterModel
            CreateMap<EruleMaster, EruleMasterModel>().ReverseMap();
            // Configures bidirectional mapping between EruleMaster entity and EruleMasterListModel
            CreateMap<EruleMaster, EruleMasterListModel>().ReverseMap();
            // Configures bidirectional mapping between EruleMaster entity and EruleCreateOrUpdateModel
            CreateMap<EruleMaster, EruleCreateOrUpdateModel>().ReverseMap();

            // Configures bidirectional mapping between ProductCapAmount entity and ProductCapAmountAddModel
            CreateMap<ProductCapAmount, ProductCapAmountAddModel>().ReverseMap();
            // Configures bidirectional mapping between ProductCapAmount entity and ProductCapAmountModel
            CreateMap<ProductCapAmount, ProductCapAmountModel>().ReverseMap();
            CreateMap<IntegrationApiEvaluation, IntegrationApiEvaluationModel>().ReverseMap();

            // Configures bidirectional mapping between Log entity and LogModel
            CreateMap<Log, LogModel>().ReverseMap();
        }

    }
}
