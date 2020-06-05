using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data;
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

            var lookup = _helper.GetResourceById(resource.Id, _ctx);
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
            var res = _helper.GetResourceById(Id, _ctx);
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
            var resource = _helper.GetResourceById(res, _ctx);
            if (resource == null)
                return BadRequest($"Resource with an ID of: {res} is missing");
            var tenant = _ctx.Tenants.Where(tenant => tenant.Id.Equals(ten)).FirstOrDefault();
            if (tenant == null)
                return BadRequest($"Tenant with an ID of: {ten} is missing");

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
                throw new Exception("An error occured, specific information logged to 'errors.txt'", ex);
            }
        }

        [HttpPatch]
        [Route("lease")]
        public ActionResult LeaseResourceAction(Guid res, Guid ten) 
        {
            var resource = _helper.GetResourceById(res, _ctx);
            if (resource == null)
                return BadRequest("Resource with such an ID is missing");
            var tenant = _helper.GetTenantById(ten, _ctx);
            if (tenant == null)
                return BadRequest("Resource with such an ID is missing");

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
                throw new Exception("An error occured, specific information logged to 'errors.txt'", ex);
            }
        }

        public bool LeaseResource(string variant, ITenant tenant, DateTime date, out IResource resource)
        {
            // null-check of tenant & resource
            throw new NotImplementedException();
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
    }
}