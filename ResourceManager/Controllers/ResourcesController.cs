using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data.Repos;
using ResourceManager.Domain.Models;

namespace ResourceManager.Controllers
{
    /// <summary>
    /// Implementacja kontrolera zasobów
    /// </summary>
    [Route("resources")]
    public class ResourcesController : Controller, IController
    {
        private IResourceRepo _resources;
        private ITenantRepo _tenants;

        public ResourcesController(IResourceRepo resources, ITenantRepo tenants)
        {
            _resources = resources;
            _tenants = tenants;
        }

        [HttpPost]
        [Route("add")]
        public ActionResult<IResource> AddResourceAction([FromBody] Resource resource)
        {
            if (resource == null)
                return BadRequest("Incorrect data provided");

            if (_resources.GetResource(resource.Id) != null)
                return BadRequest("Resource with such an ID already exists");

            // O CO CHODZI Z TYMI DATAMI??? >>>==--> SPRAWDŹ

            AddResource(resource, DateTime.Now);

            return Created("add", resource);
        }

        public void AddResource(IResource resource, DateTime fromDate)
        {
            _resources.AddResource(resource, fromDate);
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public IActionResult WithdrawResourceAction(Guid Id)
        {
            IResource res = _resources.GetResource(Id);
            if (res == null)
                return BadRequest("Resource with such an ID does not exist.");

            // O CO CHODZI Z TYMI DATAMI??? >>>==--> SPRAWDŹ
            WithdrawResource(res, DateTime.Now);

            return NoContent();
        }
        public void WithdrawResource(IResource resource, DateTime fromDate)
        {
            // Do something with those dates!!!
            _resources.WithdrawResource(resource, fromDate);
        }

        [HttpPatch]
        [Route("free")]
        public ActionResult FreeResourceAction(Guid res, Guid ten)
        {
            var resource = _resources.GetResource(res);
            if (resource == null)
                return BadRequest($"Resource with an ID of: {res} is missing");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return BadRequest($"Tenant with an ID of: {ten} is missing");
            if (!resource.LeasedTo.Equals(tenant.Id))
                return BadRequest($"Resource with ID of:{resource.Id} not belongs to tenant with ID of: {tenant.Id}");


            if (FreeResource(resource, tenant, DateTime.Now))
                return Ok($"Resource with an ID of:{res} released, and message has been sent succesfully to tenant with an ID of:{ten}");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when releasing the resource, check the 'errors.txt' file");

            // TODO notify the tenant !!

        }

        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            try
            {
                // TODO send a message to Tenant
                // TODO do something with the date
                return _resources.FreeResource(resource, tenant, date);
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                return false;
            }
        }

        [HttpPatch]
        [Route("lease")]
        public IActionResult LeaseResourceAction(Guid res, Guid ten) 
        {
            var resource = _resources.GetResource(res);
            if (resource == null)
                return BadRequest("Resource with such an ID is missing");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return BadRequest("Resource with such an ID is missing");
            if (!resource.LeasedTo.Equals(Guid.Empty))
                return ResolveTenantsConflict(resource, tenant);

            if(LeaseResource(resource, tenant, DateTime.Now))
                return Ok($"Resource with an ID of:{res} leased to the tenant with an ID of {ten}, and a message has been sent");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
        }

        public bool LeaseResource(IResource resource, ITenant tenant, DateTime date)
        {
            try
            {
                return _resources.LeaseResource(resource, tenant, date);
            }
            catch(Exception ex)
            {
                LogErrorToFile(ex);
                return false;
            }
        }

        [HttpPatch]
        [Route("lease/any")]
        public ActionResult LeaseResourceAction(string variant, Guid ten)
        {
            IResource resource;
            // zrób coś z null-checkiem co do zasobów, przemieść go np. do metody poniżej.
            var resources = _resources.GetResources(variant);
            if (resources.Length == 0)
                return NotFound("Not found any resources with such a variant.");
            // **************************************
            var tenant = _tenants.GetTenant(ten);
            // opisz w mailu czemu tu NotFound a innym razem BadRequest
            if (tenant == null)
                return BadRequest("Not found any tenant with such an ID");

            resource = _resources.FilterUnavailableResources(resources).FirstOrDefault();
            if (resource == null)
                return NotFound("Has not found any available resource with such a variant");

            if (LeaseResource(variant, tenant, DateTime.Now, out resource))
                return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {ten}, and a message has been sent");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
        }

        public bool LeaseResource(string variant, ITenant tenant, DateTime date, out IResource resource)
        {
            // LEPSZY SPOSÓB NA TRY CATCHE W ASP.NET CORE API : https://stackoverflow.com/questions/37793418/how-to-return-http-500-from-asp-net-core-rc2-web-api
            // ZRÓB TAK JAK POWYŻEJ!!!!!!!!!!!!!!!!!!!!!!*******************************************************************************************
            try
            {
                resource = null;
                var resources = _resources.GetResources(variant);
                if (resources.Length == 0)
                    return false;

                // Wybierz ten, który najdłużej leży odłogiem i jest wolny
                resource = _resources.FilterUnavailableResources(resources).FirstOrDefault();
                if (resource == null)
                    return false;

                return _resources.LeaseResource(resource, tenant, date);
            }
            catch(Exception ex)
            {
                LogErrorToFile(ex);
                resource = null;
                return false;
            }
        }

        public void LogErrorToFile(Exception ex)
        {
            var w = new StreamWriter("errors.txt");
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{ex.Message}");
            w.WriteLine("-------------------------------");
        }

        private IActionResult ResolveTenantsConflict(IResource resource, ITenant tenant)
        {
                var concurrent = _tenants.GetTenant(resource.LeasedTo);
                if (concurrent == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Resource is leased to tenant which is not existing anymore, an error occured.");
                if (concurrent.Priority < tenant.Priority)
                {
                // TODO notify the concurrent about the leasing contract termination due to higher priority tenant requested a resource
                    if (LeaseResource(resource, tenant, DateTime.Now))
                        return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {tenant.Id}, and a message has been sent");
                    else
                        return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
                }
                return BadRequest("Resource with such an ID is already leased by a tenant with higher priority");
        }
    }
}