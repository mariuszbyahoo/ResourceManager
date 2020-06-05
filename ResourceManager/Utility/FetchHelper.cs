using ResourceManager.Data;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class FetchHelper : IFetchHelper
    {
        public Tenant GetTenantById(Guid Id, ManagerDbContext _ctx)
        {
            return _ctx.Tenants.Where(t => t.Id.Equals(Id)).FirstOrDefault();
        }

        public Resource GetResourceById(Guid Id, ManagerDbContext _ctx)
        {
            return _ctx.Resources.Where(r => r.Id.Equals(Id)).FirstOrDefault();
        }
    }
}
