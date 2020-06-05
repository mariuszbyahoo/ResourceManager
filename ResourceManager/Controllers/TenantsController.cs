using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data;
using ResourceManager.Domain;
using ResourceManager.Domain.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManager.Controllers
{
    /// <summary>
    /// Implementacja kontrolera dzierżawców
    /// </summary>
    public class TenantsController : Controller, ITenantsController
    {
        private ManagerDbContext _ctx;

        public TenantsController(ManagerDbContext ctx)
        {
            _ctx = ctx;
        }

        public void AddTenant(Tenant Tenant)
        {

           _ctx.Tenants.Add(Tenant);
        }

        public void RemoveTenant(Guid Id)
        {
            throw new NotImplementedException();
        }

        public void SetTenantsPriority(Guid Id, byte NewPiority)
        {
            throw new NotImplementedException();
        }
    }
}
