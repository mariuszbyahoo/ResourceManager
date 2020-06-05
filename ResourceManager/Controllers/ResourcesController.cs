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

        public ResourcesController(ManagerDbContext ctx, IResourceFactory factory)
        {
            _ctx = ctx;
            _factory = factory;
        }

        [HttpPost]
        [Route("add")]
        public ActionResult<IResource> AddResourceAction([FromBody] Resource resource)
        {
            if (resource == null)
                return BadRequest("Incorrect data provided");

            var lookup = GetResourceById(resource.Id);
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
            var res = GetResourceById(Id);
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
        [Route("free/res={ResourceId}&ten={TenantId}")]
        public ActionResult FreeResourceAction(Guid ResourceId, Guid TenantId)
        {
            var res = GetResourceById(ResourceId);
            if (res == null)
                return BadRequest($"Resource with an ID of: {ResourceId} is missing");
            var ten = _ctx.Tenants.Where(tenant => tenant.Id.Equals(TenantId)).FirstOrDefault();
            if (ten == null)
                return BadRequest($"Tenant with an ID of: {TenantId} is missing");

            if(FreeResource(res, ten, DateTime.Now))
                return Ok($"Resource with an ID of:{ResourceId} released, and message has been sent succesfully to tenant with an ID of:{TenantId}");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when releasing the resource, check the 'errors.txt' file");

            // TODO notify the tenant !!

        }

        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            try
            {
                resource.Availability = Domain.Enums.ResourceStatus.Available;
                _ctx.Update(resource);
                _ctx.SaveChanges();
                // send a message to Tenant
                // do something with the date
                return true;
            } catch(Exception ex)
            {
                LogErrorToFile(ex);
                throw new Exception("An error occured, specific information logged to 'errors.txt'", ex);
            }
        }

        public bool LeaseResource(IResource resource, ITenant tenant, DateTime date)
        {
            // null-check of tenant & resource
            throw new NotImplementedException();
        }

        public bool LeaseResource(string variant, ITenant tenant, DateTime date, out IResource resource)
        {
            // null-check of tenant & resource
            throw new NotImplementedException();
        }

        private IResource GetResourceById(Guid Id)
        {
            return _ctx.Resources.Where(res => res.Id.Equals(Id)).FirstOrDefault();
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