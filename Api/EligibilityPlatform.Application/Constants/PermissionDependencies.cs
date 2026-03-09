using MEligibilityPlatform.Application.Constants;

namespace MEligibilityPlatform.Application.Constants
{
    /// <summary>
    /// Maps permissions to their required dependent .View permissions.
    /// When an admin assigns a permission to a role, the system automatically
    /// expands it to include all necessary .View permissions so that
    /// API calls made by those screens succeed without 403 errors.
    ///
    /// Rules:
    ///  - Only .View permissions are auto-granted (never .Screen or .Access).
    ///  - .Screen permissions auto-grant their own .View so the screen data loads.
    ///  - Admins only see .Screen + action permissions in the UI; .View is hidden.
    /// </summary>
    public static class PermissionDependencies
    {
        // Pre-built as HashSets for fast O(1) UnionWith lookups.
        private static readonly Dictionary<string, HashSet<string>> _map =
            new(StringComparer.OrdinalIgnoreCase)
        {
            // Granting a module grants all its screens + their technical views
            [Permissions.MasterData.Access] = [
                Permissions.Product.Screen, Permissions.ManagedList.Screen,
                Permissions.Factor.Screen, Permissions.Parameter.Screen,
                Permissions.ParameterBinding.Screen,
                Permissions.Product.View, Permissions.Category.View, Permissions.ListItem.View,
                Permissions.Parameter.View, Permissions.Condition.View, Permissions.DataType.View, 
                Permissions.Factor.View, Permissions.ManagedList.View, Permissions.ParameterBinding.View
            ],
            [Permissions.BusinessLogic.Access] = [
                Permissions.Rule.Screen, Permissions.ECard.Screen, Permissions.PCard.Screen,
                Permissions.Rule.View, Permissions.ECard.View, Permissions.PCard.View,
                Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View, Permissions.Product.View
            ],
            [Permissions.Connections.Access] = [
                Permissions.Integration.Screen, Permissions.Integration.View,
                Permissions.Node.View, Permissions.NodeApi.View, Permissions.ApiDetails.View,
                Permissions.ApiParameters.View, Permissions.ApiParameterMaps.View,
                Permissions.DataType.View, Permissions.Parameter.View
            ],
            [Permissions.AccessControl.Access] = [
                Permissions.Role.Screen, Permissions.Permission.Screen,
                Permissions.Role.View, Permissions.Permission.View, Permissions.User.View
            ],
            [Permissions.Approvals.Access] = [
                Permissions.MakerChecker.Screen, Permissions.MakerChecker.View
            ],
            [Permissions.Logs.Access] = [
                Permissions.Audit.Screen, Permissions.Audit.View
            ],
            [Permissions.LimitAndCaps.Access] = [
                Permissions.MakerCheckerConfig.Screen, Permissions.ProductCap.Screen, Permissions.ProductCapAmount.Screen,
                Permissions.MakerCheckerConfig.View, Permissions.ProductCap.View, Permissions.ProductCapAmount.View,
                Permissions.Product.View
            ],
            [Permissions.BulkImport.Access] = [
                Permissions.BulkImport.Screen, Permissions.BulkImport.View
            ],

            [Permissions.Parameter.Screen]          = [ Permissions.Parameter.View, Permissions.Condition.View, Permissions.DataType.View, Permissions.MasterData.Access ],
            [Permissions.Factor.Screen]             = [ Permissions.Factor.View, Permissions.Parameter.View, Permissions.Condition.View, Permissions.ManagedList.View, Permissions.DataType.View, Permissions.MasterData.Access ],
            [Permissions.Rule.Screen]               = [ Permissions.Rule.View, Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View, Permissions.BusinessLogic.Access ],
            [Permissions.ECard.Screen]              = [ Permissions.ECard.View, Permissions.Rule.View, Permissions.BusinessLogic.Access ],
            [Permissions.PCard.Screen]              = [ Permissions.PCard.View, Permissions.ECard.View, Permissions.Rule.View, Permissions.Product.View, Permissions.BusinessLogic.Access ],
            [Permissions.Product.Screen]            = [ Permissions.Product.View, Permissions.Category.View, Permissions.Parameter.View, Permissions.Factor.View, Permissions.MasterData.Access ],
            [Permissions.ManagedList.Screen]        = [ Permissions.ManagedList.View, Permissions.ListItem.View, Permissions.MasterData.Access ],
            [Permissions.Role.Screen]               = [ Permissions.Role.View, Permissions.User.View, Permissions.AccessControl.Access ],
            [Permissions.Permission.Screen]         = [ Permissions.Permission.View, Permissions.Role.View, Permissions.AccessControl.Access ],
            [Permissions.Audit.Screen]              = [ Permissions.Audit.View, Permissions.Logs.Access ],
            [Permissions.MakerChecker.Screen]       = [ Permissions.MakerChecker.View, Permissions.Approvals.Access ],
            [Permissions.BulkImport.Screen]         = [ Permissions.BulkImport.View, Permissions.BulkImport.Access ],
            [Permissions.ProductCap.Screen]         = [ Permissions.ProductCap.View, Permissions.Product.View, Permissions.LimitAndCaps.Access ],
            [Permissions.ProductCapAmount.Screen]   = [ Permissions.ProductCapAmount.View, Permissions.Product.View, Permissions.LimitAndCaps.Access ],
            [Permissions.MakerCheckerConfig.Screen] = [ Permissions.MakerCheckerConfig.View, Permissions.LimitAndCaps.Access ],
            [Permissions.ParameterBinding.Screen]   = [ Permissions.ParameterBinding.View, Permissions.Parameter.View, Permissions.MasterData.Access ],
            [Permissions.Dashboard.Screen]          = [ Permissions.Dashboard.View ],
            [Permissions.Integration.Screen]        = [
                Permissions.Integration.View, Permissions.Node.View, Permissions.NodeApi.View, 
                Permissions.ApiDetails.View, Permissions.ApiParameters.View, Permissions.ApiParameterMaps.View, 
                Permissions.DataType.View, Permissions.Parameter.View, Permissions.Connections.Access
            ],

            [Permissions.Parameter.Create]          = [ Permissions.Condition.View, Permissions.DataType.View ],
            [Permissions.Parameter.Edit]            = [ Permissions.Condition.View, Permissions.DataType.View ],
            [Permissions.Factor.Create]             = [ Permissions.Parameter.View, Permissions.Condition.View, Permissions.ManagedList.View, Permissions.DataType.View ],
            [Permissions.Factor.Edit]               = [ Permissions.Parameter.View, Permissions.Condition.View, Permissions.ManagedList.View, Permissions.DataType.View ],
            [Permissions.Rule.Create]               = [ Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View ],
            [Permissions.Rule.Edit]                 = [ Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View ],
            [Permissions.ECard.Create]              = [ Permissions.Rule.View ],
            [Permissions.ECard.Edit]                = [ Permissions.Rule.View ],
            [Permissions.PCard.Create]              = [ Permissions.ECard.View, Permissions.Rule.View, Permissions.Product.View ],
            [Permissions.PCard.Edit]                = [ Permissions.ECard.View, Permissions.Rule.View, Permissions.Product.View ],
            [Permissions.Product.Create]            = [ Permissions.Category.View ],
            [Permissions.Product.Edit]              = [ Permissions.Category.View, Permissions.Parameter.View, Permissions.Factor.View ],
        };

        /// <summary>
        /// Finds all permissions that should be removed if the specified
        /// actions are being removed. (e.g. removing MasterData.Access 
        /// should remove all screens and views granted by it).
        /// </summary>
        public static HashSet<string> ResolveRemoval(IEnumerable<string> permissionActions)
        {
            var actionsToRemove = new HashSet<string>(permissionActions, StringComparer.OrdinalIgnoreCase);
            var result = new HashSet<string>(actionsToRemove, StringComparer.OrdinalIgnoreCase);

            // If a Module Access is removed, remove all screens/views listed in its grant map
            foreach (var action in actionsToRemove)
            {
                if (_map.TryGetValue(action, out var dependents))
                {
                    result.UnionWith(dependents);
                }
            }

            return result;
        }

        /// <summary>
        /// Expands a set of permission action strings to include all
        /// required dependent .View permissions (flat, single-pass).
        /// Uses a HashSet so no duplicates are added.
        /// </summary>
        public static HashSet<string> Resolve(IEnumerable<string> permissionActions)
        {
            var result = new HashSet<string>(permissionActions, StringComparer.OrdinalIgnoreCase);

            foreach (var action in result.ToList()) // ToList: snapshot before loop modifies set
            {
                if (_map.TryGetValue(action, out var deps))
                    result.UnionWith(deps);
            }

            return result;
        }
    }
}
