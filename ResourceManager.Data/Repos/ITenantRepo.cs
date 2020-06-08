using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Data.Repos
{
    public interface ITenantRepo
    {
        public ManagerDbContext Ctx { get; set; }
        public ITenantsFactory Factory { get; set; }
        void AddTenant(ITenant tenant);
        void WithdrawTenant(ITenant tenant);
        void SetTenantsPriority(ITenant tenant, byte newPriority);
        ITenant GetTenant(Guid id);
    }
}
