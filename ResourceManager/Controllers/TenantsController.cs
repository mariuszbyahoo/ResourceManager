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
        private IFetchHelper _helper;

        public TenantsController(ManagerDbContext ctx, IFetchHelper helper, ITenantsFactory factory)
        {
            _ctx = ctx;
            _helper = helper;
            _factory = factory;
        }

        [HttpPost]
        [Route("add")]
        public ActionResult<ITenant> AddTenantAction([FromBody]Tenant Tenant)
        {
            // Jeśli podane dane są nieprawidłowe, będzie referencją do null.
            if (Tenant == null)
                return BadRequest("Incorrect data provided");

            var lookup = _helper.GetTenantById(Tenant.Id, _ctx);

            // Czy istnieje już Tenant z takim ID
            if (lookup != null)
                return BadRequest("Tenant with such an ID already exists");

            AddTenant(Tenant);

            return Created("add", Tenant);
        }
        
        /// <summary>
        /// Metoda wywoływana przez akcję kontrolera po sprawdzeniu błędnych scenariuszy
        /// </summary>
        /// <param name="Tenant"></param>
        public void AddTenant(ITenant Tenant)
        {
            var tenant = _factory.CreateInstance(Tenant.Id, Tenant.Priority, "Tenant") as Tenant;
            _ctx.Tenants.Add(tenant);
            _ctx.SaveChanges();
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public ActionResult RemoveTenantAction(Guid Id)
        {
            if (_helper.GetTenantById(Id, _ctx) == null)
                return BadRequest("Such a Tenant is missing.");

            RemoveTenant(Id);

            _ctx.SaveChanges();

            return NoContent();
        }

        /// <summary>
        /// Metoda wywoływana przez akcję kontrolera po sprawdzeniu błędnych scenariuszy
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveTenant(Guid Id)
        {
            _ctx.Tenants.Remove(_helper.GetTenantById(Id, _ctx));
            _ctx.SaveChanges();
        }

        [HttpPatch]
        [Route("patch")]
        public ActionResult SetTenantsPriorityAction([FromBody] Tenant tenant)
        {
            var tenantFromDb = _helper.GetTenantById(tenant.Id, _ctx);
            if (tenantFromDb == null)
                return BadRequest("Such a tenant is missing");

            SetTenantsPriority(tenant.Id, tenant.Priority);

            return Ok($"Tenant with an ID of: {tenant.Id} has been updated with priority of: {tenant.Priority}");
        }

        public void SetTenantsPriority(Guid Id, byte NewPriority)
        {
            // ciągłe powtarzanie użycia metody GetTenantById wymuszone przez z góry ustalone w wymaganiach argumenty do metod interfejsu.
            var tenant = _helper.GetTenantById(Id, _ctx);
            tenant.Priority = NewPriority;
            _ctx.Update(tenant);
            _ctx.SaveChanges();
        }
    }
}
