using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Application.Constants
{

    public static class Permissions
    {
        public static List<string> GetRegisteredPermissions()
        {
            var permissions = new List<string>();
            foreach (var field in typeof(Permissions)
                     .GetNestedTypes()
                     .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    permissions.Add(value);
            }
            return permissions;
        }
        public static class ApiDetails
        {
            public const string Create = "Permissions.ApiDetails.Create";
            public const string Delete = "Permissions.ApiDetails.Delete";
            public const string Edit = "Permissions.ApiDetails.Edit";
            public const string View = "Permissions.ApiDetails.View";
            public const string Export = "Permissions.ApiDetails.Export";
            public const string Import = "Permissions.ApiDetails.Import";
        }
        public static class ApiParameterMaps
        {
            public const string Create = "Permissions.ApiParameterMaps.Create";
            public const string Delete = "Permissions.ApiParameterMaps.Delete";
            public const string Edit = "Permissions.ApiParameterMaps.Edit";
            public const string View = "Permissions.ApiParameterMaps.View";
            public const string Export = "Permissions.ApiParameterMaps.Export";
            public const string Import = "Permissions.ApiParameterMaps.Import";
        }
        public static class ApiParameters
        {
            public const string Create = "Permissions.ApiParameters.Create";
            public const string Delete = "Permissions.ApiParameters.Delete";
            public const string Edit = "Permissions.ApiParameters.Edit";
            public const string View = "Permissions.ApiParameters.View";
            public const string Export = "Permissions.ApiParameters.Export";
            public const string Import = "Permissions.ApiParameters.Import";
        }
        public static class ApiResponses
        {
            public const string Create = "Permissions.ApiResponses.Create";
            public const string Delete = "Permissions.ApiResponses.Delete";
            public const string Edit = "Permissions.ApiResponses.Edit";
            public const string View = "Permissions.ApiResponses.View";
            public const string Export = "Permissions.ApiResponses.Export";
            public const string Import = "Permissions.ApiResponses.Import";
        }
        public static class AppSetting
        {
            public const string Edit = "Permissions.AppSetting.Edit";
        }
        public static class Audit
        {
            public const string Create = "Permissions.Audit.Create";
            public const string Delete = "Permissions.Audit.Delete";
            public const string Edit = "Permissions.Audit.Edit";
            public const string View = "Permissions.Audit.View";
        }

        public static class BulkImport
        {

            public const string View = "Permissions.BulkImport.View";
            public const string DownloadImportedFile = "Permissions.BulkImport.Download";
            public const string Import = "Permissions.BulkImport.Import";
        }
        public static class Category
        {
            public const string Create = "Permissions.Category.Create";
            public const string Delete = "Permissions.Category.Delete";
            public const string Edit = "Permissions.Category.Edit";
            public const string View = "Permissions.Category.View";
            public const string Export = "Permissions.Category.Export";
            public const string Import = "Permissions.Category.Import";
        }
        public static class City
        {
            public const string Create = "Permissions.City.Create";
            public const string Delete = "Permissions.City.Delete";
            public const string Edit = "Permissions.City.Edit";
            public const string View = "Permissions.City.View";
        }
        public static class Condition
        {
            public const string Create = "Permissions.Condition.Create";
            public const string Delete = "Permissions.Condition.Delete";
            public const string Edit = "Permissions.Condition.Edit";
            public const string View = "Permissions.Condition.View";
        }
        public static class Country
        {
            public const string Create = "Permissions.Country.Create";
            public const string Delete = "Permissions.Country.Delete";
            public const string Edit = "Permissions.Country.Edit";
            public const string View = "Permissions.Country.View";
        }
        public static class Currency
        {
            public const string Create = "Permissions.Currency.Create";
            public const string Delete = "Permissions.Currency.Delete";
            public const string Edit = "Permissions.Currency.Edit";
            public const string View = "Permissions.Currency.View";
        }
        public static class Dashboard
        {

            public const string View = "Permissions.Dashboard.View";
        }

        public static class DataType
        {
            public const string Create = "Permissions.DataType.Create";
            public const string Delete = "Permissions.DataType.Delete";
            public const string Edit = "Permissions.DataType.Edit";
            public const string View = "Permissions.DataType.View";
        }
        public static class ECard
        {
            public const string Create = "Permissions.ECard.Create";
            public const string Delete = "Permissions.ECard.Delete";
            public const string Edit = "Permissions.ECard.Edit";
            public const string View = "Permissions.ECard.View";
            public const string Export = "Permissions.ECard.Export";
            public const string Import = "Permissions.ECard.Import";
        }
        public static class EvaluationHistory
        {
            public const string Create = "Permissions.EvaluationHistory.Create";
            public const string Delete = "Permissions.EvaluationHistory.Delete";
            public const string Edit = "Permissions.EvaluationHistory.Edit";
            public const string View = "Permissions.EvaluationHistory.View";
        }
        public static class Exception
        {
            public const string Create = "Permissions.Exception.Create";
            public const string Delete = "Permissions.Exception.Delete";
            public const string Edit = "Permissions.Exception.Edit";
            public const string View = "Permissions.Exception.View";
        }
        public static class Factor
        {
            public const string Create = "Permissions.Factor.Create";
            public const string Delete = "Permissions.Factor.Delete";
            public const string Edit = "Permissions.Factor.Edit";
            public const string View = "Permissions.Factor.View";
            public const string Export = "Permissions.Factor.Export";
            public const string Import = "Permissions.Factor.Import";
        }
        public static class GroupRole
        {
            public const string Create = "Permissions.GroupRole.Create";
            public const string Delete = "Permissions.GroupRole.Delete";
            public const string Edit = "Permissions.GroupRole.Edit";
            public const string View = "Permissions.GroupRole.View";
        }
        public static class HistoryEc
        {
            public const string Create = "Permissions.HistoryEc.Create";
            public const string Delete = "Permissions.HistoryEc.Delete";
            public const string Edit = "Permissions.HistoryEc.Edit";
            public const string View = "Permissions.HistoryEc.View";
        }
        public static class HistoryEr
        {
            public const string Create = "Permissions.HistoryEr.Create";
            public const string Delete = "Permissions.HistoryEr.Delete";
            public const string Edit = "Permissions.HistoryEr.Edit";
            public const string View = "Permissions.HistoryEr.View";
        }
        public static class HistoryParameter
        {
            public const string Create = "Permissions.HistoryParameter.Create";
            public const string Delete = "Permissions.HistoryParameter.Delete";
            public const string Edit = "Permissions.HistoryParameter.Edit";
            public const string View = "Permissions.HistoryParameter.View";
        }
        public static class HistoryPc
        {
            public const string Create = "Permissions.HistoryPc.Create";
            public const string Delete = "Permissions.HistoryPc.Delete";
            public const string Edit = "Permissions.HistoryPc.Edit";
            public const string View = "Permissions.HistoryPc.View";
        }
        public static class ListItem
        {
            public const string Create = "Permissions.ListItem.Create";
            public const string Delete = "Permissions.ListItem.Delete";
            public const string Edit = "Permissions.ListItem.Edit";
            public const string View = "Permissions.ListItem.View";
            public const string Export = "Permissions.ListItem.Export";
            public const string Import = "Permissions.ListItem.Import";
        }
        public static class Log
        {
            public const string View = "Permissions.Log.View";

        }
        public static class MakerChecker
        {
            public const string Create = "Permissions.MakerChecker.Create";
            public const string Delete = "Permissions.MakerChecker.Delete";
            public const string Edit = "Permissions.MakerChecker.Edit";
            public const string View = "Permissions.MakerChecker.View";

        }
        public static class ManagedList
        {
            public const string Create = "Permissions.ManagedList.Create";
            public const string Delete = "Permissions.ManagedList.Delete";
            public const string Edit = "Permissions.ManagedList.Edit";
            public const string View = "Permissions.ManagedList.View";
            public const string Export = "Permissions.ManagedList.Export";
            public const string Import = "Permissions.ManagedList.Import";
        }

        public static class MapFunction
        {
            public const string Create = "Permissions.MapFunction.Create";
            public const string Delete = "Permissions.MapFunction.Delete";
            public const string Edit = "Permissions.MapFunction.Edit";
            public const string View = "Permissions.MapFunction.View";

        }
        public static class NodeApi
        {
            public const string Create = "Permissions.NodeApi.Create";
            public const string Delete = "Permissions.NodeApi.Delete";
            public const string Edit = "Permissions.NodeApi.Edit";
            public const string View = "Permissions.NodeApi.View";

        }
        public static class Node
        {
            public const string Create = "Permissions.Node.Create";
            public const string Delete = "Permissions.Node.Delete";
            public const string Edit = "Permissions.Node.Edit";
            public const string View = "Permissions.Node.View";

        }
        public static class Parameter
        {
            public const string Create = "Permissions.Parameter.Create";
            public const string Delete = "Permissions.Parameter.Delete";
            public const string Edit = "Permissions.Parameter.Edit";
            public const string View = "Permissions.Parameter.View";
            public const string Export = "Permissions.Parameter.Export";
            public const string Import = "Permissions.Parameter.Import";
            public const string CheckComputedValue = "Permissions.Parameter.CheckComputedValue";

        }
        public static class ParameterBinding
        {
            public const string Create = "Permissions.ParameterBinding.Create";
            public const string View = "Permissions.ParameterBinding.View";

        }

        public static class ParametersMap
        {
            public const string Create = "Permissions.ParametersMap.Create";
            public const string Delete = "Permissions.ParametersMap.Delete";
            public const string Edit = "Permissions.ParametersMap.Edit";
            public const string View = "Permissions.ParametersMap.View";

        }
        public static class PCard
        {
            public const string Create = "Permissions.PCard.Create";
            public const string Delete = "Permissions.PCard.Delete";
            public const string Edit = "Permissions.PCard.Edit";
            public const string View = "Permissions.PCard.View";
            public const string Export = "Permissions.PCard.Export";
            public const string Import = "Permissions.PCard.Import";

        }
        public static class ProductCapAmount
        {
            public const string Create = "Permissions.ProductCapAmount.Create";
            public const string Delete = "Permissions.ProductCapAmount.Delete";
            public const string Edit = "Permissions.ProductCapAmount.Edit";
            public const string View = "Permissions.ProductCapAmount.View";


        }
        public static class ProductCap
        {
            public const string Create = "Permissions.ProductCap.Create";
            public const string Delete = "Permissions.ProductCap.Delete";
            public const string Edit = "Permissions.ProductCap.Edit";
            public const string View = "Permissions.ProductCap.View";


        }
        public static class Product
        {
            public const string Create = "Permissions.Product.Create";
            public const string Delete = "Permissions.Product.Delete";
            public const string Edit = "Permissions.Product.Edit";
            public const string View = "Permissions.Product.View";
            public const string Export = "Permissions.Product.Export";
            public const string Import = "Permissions.Product.Import";

        }
        public static class ProductParam
        {
            public const string Create = "Permissions.ProductParam.Create";
            public const string Delete = "Permissions.ProductParam.Delete";
            public const string Edit = "Permissions.ProductParam.Edit";
            public const string View = "Permissions.ProductParam.View";
            public const string Export = "Permissions.ProductParam.Export";
            public const string Import = "Permissions.ProductParam.Import";

        }
        public static class Role
        {
            public const string Create = "Permissions.Role.Create";
            public const string Delete = "Permissions.Role.Delete";
            public const string Edit = "Permissions.Role.Edit";
            public const string View = "Permissions.Role.View";


        }
        public static class Rule
        {
            public const string Create = "Permissions.Rule.Create";
            public const string Delete = "Permissions.Rule.Delete";
            public const string Edit = "Permissions.Rule.Edit";
            public const string View = "Permissions.Rule.View";
            public const string Export = "Permissions.Rule.Export";
            public const string Import = "Permissions.Rule.Import";
        }
        public static class Screen
        {
            public const string Create = "Permissions.Screen.Create";
            public const string Delete = "Permissions.Screen.Delete";
            public const string Edit = "Permissions.Screen.Edit";
            public const string View = "Permissions.Screen.View";


        }
        public static class User
        {
            public const string Create = "Permissions.User.Create";
            public const string Delete = "Permissions.User.Delete";
            public const string Edit = "Permissions.User.Edit";
            public const string View = "Permissions.User.View";


        }
        public static class UserGroup
        {
            public const string Create = "Permissions.UserGroup.Create";
            public const string Delete = "Permissions.UserGroup.Delete";
            public const string Edit = "Permissions.UserGroup.Edit";
            public const string View = "Permissions.UserGroup.View";


        }
        public static class UserStatus
        {
            public const string Create = "Permissions.UserStatus.Create";
            public const string Delete = "Permissions.UserStatus.Delete";
            public const string Edit = "Permissions.UserStatus.Edit";
            public const string View = "Permissions.UserStatus.View";


        }
        public static class Validator
        {
            public const string Rule = "Permissions.Validator.Rule";
            public const string ECard = "Permissions.Validator.ECard";
            public const string PCard = "Permissions.Validator.PCard";



        }
        public static class MakerCheckerConfig
        {

            public const string Edit = "Permissions.MakerCheckerConfig.Edit";
            public const string View = "Permissions.MakerCheckerConfig.View";


        }
        public static class Group
        {
            public const string Create = "Permissions.Group.Create";
            public const string Delete = "Permissions.Group.Delete";
            public const string Edit = "Permissions.Group.Edit";
            public const string View = "Permissions.Group.View";


        }
    }
}
