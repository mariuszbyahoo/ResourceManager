using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data;
using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;
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
        private ITenantsFactory _factory;

        public TenantsController(ManagerDbContext ctx, ITenantsFactory factory)
        {
            _ctx = ctx;
            _factory = factory;
        }

        [HttpPost]
        [Route("addTenant")]
        public ActionResult<ITenant> AddTenantAction([FromBody]Tenant Tenant)
        {
            AddTenant(Tenant);
            return Created("addTenant", Tenant);
        }
        
        public void AddTenant(ITenant Tenant)
        {
            var tenant = new Tenant { Id = Tenant.Id, Priority = Tenant.Priority };
            _ctx.Tenants.Add(tenant);
            _ctx.SaveChanges();
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
