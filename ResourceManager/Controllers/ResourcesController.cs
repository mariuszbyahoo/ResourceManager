using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var res = GetResourceById(resource.Id);
            if (res == null)
                return BadRequest("Resource with such an ID already exists");

            // O CO CHODZI Z TYMI DATAMI??? >>>==--> SPRAWDŹ

            AddResource(resource, DateTime.Now);

            return Created("add", resource);
        }

        public void AddResource(IResource resource, DateTime fromDate)
        {
            throw new NotImplementedException();
        }
        public void WithdrawResource(IResource resource, DateTime fromDate)
        {
            // null-check of resource
            throw new NotImplementedException();
        }
        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            // null-check of tenant & resource
            throw new NotImplementedException();
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
    }
}