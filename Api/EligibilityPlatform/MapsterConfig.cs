using MapsterMapper;
using Mapster;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
namespace MEligibilityPlatform
{
    public static class MapsterConfig
    {
        public static void Register()
        {


            TypeAdapterConfig<Apidetail, ApiDetailListModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Apidetail, ApiDetailCreateotUpdateModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Category, CategoryModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Category, CategoryCreateUpdateModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Category, CategoryListModel>.NewConfig().TwoWays();

            TypeAdapterConfig<City, CityModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Audit, AuditModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Audit, AuditCreateUpdateModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Condition, ConditionModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Country, CountryModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Currency, CurrencyModel>.NewConfig().TwoWays();
            TypeAdapterConfig<DataType, DataTypeModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Parameter, ParameterModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Parameter, ParameterAddUpdateModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Parameter, ParameterListModel>.NewConfig().TwoWays();
            TypeAdapterConfig<ParameterComputedValue, ParameterComputedValueModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Ecard, EcardModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Ecard, EcardAddUpdateModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Ecard, EcardListModel>.NewConfig().TwoWays();
            TypeAdapterConfig<EvaluationHistory, EvaluationHistoryModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Erule, EruleModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Erule, EruleListModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Erule, EruleCreateModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Erule, EruleUpdateModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Erule, EruleCreateOrUpdateModel>.NewConfig().TwoWays();

            TypeAdapterConfig<User, UserModel>.NewConfig().TwoWays();
            TypeAdapterConfig<User, UserEditModel>.NewConfig().TwoWays();
            TypeAdapterConfig<UserEditModel, User>.NewConfig().TwoWays();
            TypeAdapterConfig<UserPictureModel, User>.NewConfig().TwoWays();

            TypeAdapterConfig<UserStatus, UserStatusModel>.NewConfig().TwoWays();
            TypeAdapterConfig<UserStatus, UserStatusAddModel>.NewConfig().TwoWays();

            TypeAdapterConfig<Role, RoleModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Role, RoleCreateUpdateModel>.NewConfig().TwoWays();

            TypeAdapterConfig<SecurityGroup, SecurityGroupModel>.NewConfig().TwoWays();
            TypeAdapterConfig<SecurityGroup, SecurityGroupUpdateModel>.NewConfig().TwoWays();

            TypeAdapterConfig<UserGroup, UserGroupModel>.NewConfig().TwoWays();
            TypeAdapterConfig<UserGroup, UserGroupCreateUpdateModel>.NewConfig().TwoWays();

            TypeAdapterConfig<GroupRole, GroupRoleModel>.NewConfig().TwoWays();
            TypeAdapterConfig<Screen, ScreenModel>.NewConfig().TwoWays();

      

            TypeAdapterConfig<MakerChecker, MakerCheckerModel>
                .NewConfig()
                .Map(d => d.StatusName, s => s.Status.ToString())
                .Map(d => d.MakerName, s => s.Maker.UserName);

            TypeAdapterConfig<ProductParam, ProductParamListModel>
                .NewConfig()
                .Map(d => d.ProductName, s => s.Product.ProductName)
                .Map(d => d.ParameterName, s => s.Parameter.ParameterName);

            TypeAdapterConfig<ParameterBinding, ParameterBindingModel>
                .NewConfig()
                .Map(d => d.SystemParameterName, s => s.SystemParameter!.Name)
                .TwoWays();
        }
    }
}