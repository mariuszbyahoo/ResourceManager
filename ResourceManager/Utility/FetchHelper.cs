using ResourceManager.Data;
using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class FetchHelper : IFetchHelper
    {
        public Tenant GetTenant(Guid Id, ManagerDbContext _ctx)
        {
            return _ctx.Tenants.Where(t => t.Id.Equals(Id)).FirstOrDefault();
        }

        public Resource GetResource(Guid Id, ManagerDbContext _ctx)
        {
            return _ctx.Resources.Where(r => r.Id.Equals(Id)).FirstOrDefault();
        }

        public Resource[] GetResources(string variant, ManagerDbContext _ctx)
        {
            return _ctx.Resources.Where(r => r.Variant.ToLower().Equals(variant.ToLower())).ToArray();
        }

        public Resource[] GetAvailableResources(Resource[] resources)
        {
            return resources.Where(r => r.Availability.Equals(ResourceStatus.Available)).ToArray();
        }
    }
}
