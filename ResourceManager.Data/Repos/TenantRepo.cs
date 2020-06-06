using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceManager.Data.Repos
{
    public class TenantRepo : ITenantRepo
    {
        public ManagerDbContext Ctx { get; set; }
        public ITenantsFactory Factory { get; set; }

        public TenantRepo(ManagerDbContext ctx, ITenantsFactory factory)
        {
            Ctx = ctx;
            Factory = factory;
        }

        public void AddTenant(ITenant tenant)
        {

            Ctx.Tenants.Add(tenant as Tenant);
            Ctx.SaveChanges();
        }

        public ITenant GetTenant(Guid id)
        {
            return Ctx.Tenants.Where(t => t.Id.Equals(id)).FirstOrDefault();
        }

        public void SetTenantsPriority(ITenant tenant, byte newPriority)
        {
            tenant.Priority = newPriority;
            Ctx.Update(tenant);
            Ctx.SaveChanges();
        }

        public void WithdrawTenant(ITenant tenant)
        {
            Ctx.Remove(tenant);
            Ctx.SaveChanges();
        }
    }
}
