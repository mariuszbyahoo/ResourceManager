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
        // ADD
        void AddTenant(ITenant tenant);
        // DELETE
        void WithdrawTenant(ITenant tenant);
        // MODIFY
        void SetTenantsPriority(ITenant tenant, byte newPriority);
        // GET SINGLE
        ITenant GetTenant(Guid id);
    }
}
