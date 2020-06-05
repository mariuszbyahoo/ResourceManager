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
    [Route("tenants")]
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
        [Route("add")]
        public ActionResult<ITenant> AddTenantAction([FromBody]Tenant Tenant)
        {
            // W przypadku dostarczenia nieprawidłowego GUID, obiekt "Tenant" będzie referencją do null
            if (Tenant == null)
                return BadRequest("Incorrect data provided");

            var lookup = _ctx.Tenants.Where(tenant => tenant.Id.Equals(Tenant.Id)).FirstOrDefault();

            if (lookup != null)
                return BadRequest("Tenant with such an ID already exists");

            AddTenant(Tenant);
            return Created("addTenant", Tenant);
        }
        
        public void AddTenant(ITenant Tenant)
        {
            var tenant = _factory.CreateInstance(Tenant.Id, Tenant.Priority, "Tenant") as Tenant;
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
