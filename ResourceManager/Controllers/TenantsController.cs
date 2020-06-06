using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data;
using ResourceManager.Data.Repos;
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
        private ITenantRepo _repo;

        public TenantsController(ITenantRepo repo)
        {
            _repo = repo;
        }

        [HttpPost]
        [Route("add")]
        public ActionResult<ITenant> AddTenantAction([FromBody]Tenant Tenant)
        {
            // Jeśli podane dane są nieprawidłowe, będzie referencją do null.
            if (Tenant == null)
                return BadRequest("Incorrect data provided");

            var lookup = _repo.GetTenant(Tenant.Id);

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
            _repo.AddTenant(Tenant);
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public ActionResult RemoveTenantAction(Guid Id)
        {
            // TODO zwolnij wszystkie wynajęte przez niego zasoby od razu.
            if (_repo.GetTenant(Id) == null)
                return BadRequest("Such a Tenant is missing.");

            RemoveTenant(Id);
            return NoContent();
        }

        /// <summary>
        /// Metoda wywoływana przez akcję kontrolera po sprawdzeniu błędnych scenariuszy
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveTenant(Guid Id)
        {
            _repo.WithdrawTenant(_repo.GetTenant(Id));
        }

        [HttpPatch]
        [Route("patch")]
        public ActionResult SetTenantsPriorityAction([FromBody] Tenant tenant)
        {
            if (_repo.GetTenant(tenant.Id) == null)
                return BadRequest("Such a tenant is missing");

            SetTenantsPriority(tenant.Id, tenant.Priority);

            return Ok($"Tenant with an ID of: {tenant.Id} has been updated with priority of: {tenant.Priority}");
        }

        /// <summary>
        /// Metoda wywoływana przez akcję kontrolera po sprawdzeniu błędnych scenariuszy
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="NewPriority"></param>
        public void SetTenantsPriority(Guid Id, byte NewPriority)
        {
            _repo.SetTenantsPriority(_repo.GetTenant(Id), NewPriority);
        }
    }
}
