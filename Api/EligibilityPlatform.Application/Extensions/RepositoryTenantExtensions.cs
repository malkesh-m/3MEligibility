using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Extensions
{
    public static class RepositoryTenantExtensions
    {
        public static IQueryable<T> GetAllByTenantId<T>(
            this IQueryable<T> query,
            int tenantId)
            where T : ITenantEntity
        {
            return query.Where(x => x.TenantId == tenantId);
        }
    }
}
