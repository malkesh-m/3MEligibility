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
                Permissions.Role.View, Permissions.Permission.View, Permissions.User.View,
                Permissions.RolePermission.View
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

            [Permissions.Parameter.Screen]          = [ Permissions.Parameter.View, Permissions.Condition.View, Permissions.DataType.View ],
            [Permissions.Factor.Screen]             = [ Permissions.Factor.View, Permissions.Parameter.View, Permissions.Condition.View, Permissions.ManagedList.View, Permissions.DataType.View ],
            [Permissions.Rule.Screen]               = [ Permissions.Rule.View, Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View ],
            [Permissions.ECard.Screen]              = [ Permissions.ECard.View, Permissions.Rule.View ],
            [Permissions.PCard.Screen]              = [ Permissions.PCard.View, Permissions.ECard.View, Permissions.Rule.View, Permissions.Product.View ],
            [Permissions.Product.Screen]            = [ Permissions.Product.View, Permissions.Category.View, Permissions.Parameter.View, Permissions.Factor.View ],
            [Permissions.ManagedList.Screen]        = [ Permissions.ManagedList.View, Permissions.ListItem.View ],
            [Permissions.Role.Screen]               = [ Permissions.Role.View, Permissions.User.View, Permissions.RolePermission.View ],
            [Permissions.Permission.Screen]         = [ Permissions.Permission.View, Permissions.Role.View, Permissions.RolePermission.View ],
            [Permissions.Audit.Screen]              = [ Permissions.Audit.View ],
            [Permissions.MakerChecker.Screen]       = [ Permissions.MakerChecker.View ],
            [Permissions.BulkImport.Screen]         = [ Permissions.BulkImport.View ],
            [Permissions.ProductCap.Screen]         = [ Permissions.ProductCap.View, Permissions.Product.View ],
            [Permissions.ProductCapAmount.Screen]   = [ Permissions.ProductCapAmount.View, Permissions.Product.View ],
            [Permissions.MakerCheckerConfig.Screen] = [ Permissions.MakerCheckerConfig.View ],
            [Permissions.ParameterBinding.Screen]   = [ Permissions.ParameterBinding.View, Permissions.Parameter.View ],
            [Permissions.Dashboard.Screen]          = [ Permissions.Dashboard.View ],
            [Permissions.Integration.Screen]        = [
                Permissions.Integration.View, Permissions.Node.View, Permissions.NodeApi.View, 
                Permissions.ApiDetails.View, Permissions.ApiParameters.View, Permissions.ApiParameterMaps.View, 
                Permissions.DataType.View, Permissions.Parameter.View
            ],

            [Permissions.Parameter.Create]          = [ Permissions.Condition.View, Permissions.DataType.View ],
            [Permissions.Parameter.Edit]            = [ Permissions.Condition.View, Permissions.DataType.View ],
            [Permissions.Factor.Create]             = [ Permissions.Parameter.View, Permissions.Condition.View, Permissions.ManagedList.View, Permissions.DataType.View ],
            [Permissions.Factor.Edit]               = [ Permissions.Parameter.View, Permissions.Condition.View, Permissions.ManagedList.View, Permissions.DataType.View ],
            [Permissions.Rule.Create]               = [ Permissions.Rule.View, Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View ],
            [Permissions.Rule.Edit]                 = [ Permissions.Rule.View, Permissions.Parameter.View, Permissions.Factor.View, Permissions.Condition.View ],
            [Permissions.ECard.Create]              = [ Permissions.Rule.View ],
            [Permissions.ECard.Edit]                = [ Permissions.Rule.View ],
            [Permissions.PCard.Create]              = [ Permissions.ECard.View, Permissions.Rule.View, Permissions.Product.View ],
            [Permissions.PCard.Edit]                = [ Permissions.ECard.View, Permissions.Rule.View, Permissions.Product.View ],
            [Permissions.Product.Create]            = [ Permissions.Category.View ],
            [Permissions.Product.Edit]              = [ Permissions.Category.View, Permissions.Parameter.View, Permissions.Factor.View ],
        };

        // Add-only parent links: assigning a screen should also assign its module access,
        // but removing the screen should NOT remove the parent access.
        private static readonly Dictionary<string, HashSet<string>> _addOnlyParents =
            new(StringComparer.OrdinalIgnoreCase)
        {
            [Permissions.Parameter.Screen]          = [ Permissions.MasterData.Access ],
            [Permissions.Factor.Screen]             = [ Permissions.MasterData.Access ],
            [Permissions.Product.Screen]            = [ Permissions.MasterData.Access ],
            [Permissions.ManagedList.Screen]        = [ Permissions.MasterData.Access ],
            [Permissions.ParameterBinding.Screen]   = [ Permissions.MasterData.Access ],

            [Permissions.Rule.Screen]               = [ Permissions.BusinessLogic.Access ],
            [Permissions.ECard.Screen]              = [ Permissions.BusinessLogic.Access ],
            [Permissions.PCard.Screen]              = [ Permissions.BusinessLogic.Access ],

            [Permissions.Integration.Screen]        = [ Permissions.Connections.Access ],

            [Permissions.Role.Screen]               = [ Permissions.AccessControl.Access ],
            [Permissions.Permission.Screen]         = [ Permissions.AccessControl.Access ],

            [Permissions.MakerChecker.Screen]       = [ Permissions.Approvals.Access ],
            [Permissions.Audit.Screen]              = [ Permissions.Logs.Access ],
            [Permissions.ProductCap.Screen]         = [ Permissions.LimitAndCaps.Access ],
            [Permissions.ProductCapAmount.Screen]   = [ Permissions.LimitAndCaps.Access ],
            [Permissions.MakerCheckerConfig.Screen] = [ Permissions.LimitAndCaps.Access ],
            [Permissions.BulkImport.Screen]         = [ Permissions.BulkImport.Access ],
        };

        /// <summary>
        /// Expands a set of permission action strings to include all
        /// required dependent .View permissions.
        /// Uses a recursive approach to handle transitive dependencies (A -> B -> C).
        /// </summary>
        public static HashSet<string> Resolve(IEnumerable<string> permissionActions)
        {
            var result = new HashSet<string>(permissionActions, StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(result);

            while (queue.Count > 0)
            {
                var action = queue.Dequeue();
                
                // Add explicit technical dependencies
                if (_map.TryGetValue(action, out var deps))
                {
                    foreach (var dep in deps)
                    {
                        if (result.Add(dep))
                        {
                            queue.Enqueue(dep);
                        }
                    }
                }

                // Add parent links (headers)
                if (_addOnlyParents.TryGetValue(action, out var parents))
                {
                    foreach (var parent in parents)
                    {
                        if (result.Add(parent))
                        {
                            queue.Enqueue(parent);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Expands a set of permission action strings to include ONLY 
        /// their technical .View dependencies.
        /// This is used during removal to find what technical permissions 
        /// must stay without following functional loops (like Header -> Screen).
        /// </summary>
        public static HashSet<string> ResolveTechnicalOnly(IEnumerable<string> permissionActions)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(permissionActions);
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (queue.Count > 0)
            {
                var action = queue.Dequeue();
                if (!processed.Add(action)) continue;

                if (_map.TryGetValue(action, out var deps))
                {
                    foreach (var dep in deps)
                    {
                        // ONLY follow .View dependencies for survival
                        if (dep.EndsWith(".View", StringComparison.OrdinalIgnoreCase))
                        {
                            if (result.Add(dep)) queue.Enqueue(dep);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all child .Screen permissions that belong to a specific .Access module.
        /// This is used during permission removal to cleanly remove the whole module.
        /// </summary>
        public static IEnumerable<string> GetChildScreens(string accessPermissionAction)
        {
            if (_map.TryGetValue(accessPermissionAction, out var deps))
            {
                return deps.Where(d => d.EndsWith(".Screen", StringComparison.OrdinalIgnoreCase));
            }
            return [];
        }
    }
}
