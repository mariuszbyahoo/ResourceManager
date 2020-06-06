using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data;
using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;

namespace ResourceManager.Controllers
{
    /// <summary>
    /// Implementacja kontrolera zasobów
    /// </summary>
    [Route("resources")]
    public class ResourcesController : Controller, IController
    {
        private ManagerDbContext _ctx;
        private IResourceFactory _factory;
        private IFetchHelper _helper;

        public ResourcesController(ManagerDbContext ctx, IFetchHelper helper, IResourceFactory factory)
        {
            _ctx = ctx;
            _helper = helper;
            _factory = factory;
        }

        [HttpPost]
        [Route("add")]
        public ActionResult<IResource> AddResourceAction([FromBody] Resource resource)
        {
            if (resource == null)
                return BadRequest("Incorrect data provided");

            var lookup = _helper.GetResource(resource.Id, _ctx);
            if (lookup != null)
                return BadRequest("Resource with such an ID already exists");

            // O CO CHODZI Z TYMI DATAMI??? >>>==--> SPRAWDŹ

            AddResource(resource, DateTime.Now);

            return Created("add", resource);
        }

        public void AddResource(IResource resource, DateTime fromDate)
        {
            var res = _factory.CreateInstance(resource.Id, resource.Variant, "Resource") as Resource;
            _ctx.Resources.Add(res);
            _ctx.SaveChanges();
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public IActionResult WithdrawResourceAction(Guid Id)
        {
            var res = _helper.GetResource(Id, _ctx);
            if (res == null)
                return BadRequest("Resource with such an ID does not exist.");

            // O CO CHODZI Z TYMI DATAMI??? >>>==--> SPRAWDŹ
            WithdrawResource(res, DateTime.Now);

            return NoContent();
        }
        public void WithdrawResource(IResource resource, DateTime fromDate)
        {
            // Do something with those dates!!!
            var res = _ctx.Resources.Where(res => res.Id.Equals(resource.Id)).FirstOrDefault();
            _ctx.Resources.Remove(res);
            _ctx.SaveChanges();
        }

        [HttpPatch]
        [Route("free")]
        public ActionResult FreeResourceAction(Guid res, Guid ten)
        {
            var resource = _helper.GetResource(res, _ctx);
            if (resource == null)
                return BadRequest($"Resource with an ID of: {res} is missing");
            var tenant = _ctx.Tenants.Where(tenant => tenant.Id.Equals(ten)).FirstOrDefault();
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
                resource.Availability = Domain.Enums.ResourceStatus.Available;
                resource.LeasedTo = Guid.Empty;
                _ctx.Update(resource);
                _ctx.SaveChanges();
                // send a message to Tenant
                // do something with the date
                return true;
            } catch (Exception ex)
            {
                LogErrorToFile(ex);
                return false;
            }
        }

        [HttpPatch]
        [Route("lease")]
        public IActionResult LeaseResourceAction(Guid res, Guid ten) 
        {
            var resource = _helper.GetResource(res, _ctx);
            if (resource == null)
                return BadRequest("Resource with such an ID is missing");
            var tenant = _helper.GetTenant(ten, _ctx);
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
                resource.Availability = Domain.Enums.ResourceStatus.Occupied;
                resource.LeasedTo = tenant.Id;
                _ctx.Update(resource);
                _ctx.SaveChanges();
                return true;
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
            var resources = _helper.GetResources(variant, _ctx);
            if (resources.Length == 0)
                return NotFound("Not found any resources with such a variant.");
            // **************************************
            var tenant = _helper.GetTenant(ten, _ctx);
            // opisz w mailu czemu tu NotFound a innym razem BadRequest
            if (tenant == null)
                return BadRequest("Not found any tenant with such an ID");

            resource = _helper.GetAvailableResources(resources).FirstOrDefault();
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
                var resources = _helper.GetResources(variant, _ctx);
                if (resources.Length == 0)
                    return false;

                // Wybierz ten, który najdłużej leży odłogiem i jest wolny
                resource = _helper.GetAvailableResources(resources).FirstOrDefault();

                resource.Availability = ResourceStatus.Occupied;
                resource.LeasedTo = tenant.Id;

                _ctx.Update(resource);
                _ctx.SaveChanges();

                return true;
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
                var concurrent = _helper.GetTenant(resource.LeasedTo, _ctx);
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