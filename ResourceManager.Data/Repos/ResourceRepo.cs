using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceManager.Data.Repos
{
    public class ResourceRepo : IResourceRepo
    {
        public ManagerDbContext Ctx { get; set; }

        public ResourceRepo(ManagerDbContext ctx)
        {
            Ctx = ctx;
        }

        public void AddResource(IResource resource, DateTime availableFromDate)
        {
            // TODO Handle dates!
            Ctx.Resources.Add(resource as Resource);
            Ctx.SaveChanges();
        }

        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            resource.Availability = Domain.Enums.ResourceStatus.Available;
            resource.LeasedTo = Guid.Empty;
            Ctx.Update(resource);
            Ctx.SaveChanges();
            return true;
        }

        public Resource[] FilterUnavailableResources(Resource[] resources)
        {
            return resources.Where(r => r.Availability.Equals(ResourceStatus.Available)).ToArray();
        }

        public Resource GetResource(Guid id)
        {
            // TODO handle this exception
            // Throws error if duplicate
            return Ctx.Resources.Where(r => r.Id.Equals(id)).FirstOrDefault();
        }

        public Resource[] GetResources(string variant)
        {
            return Ctx.Resources.Where(r => r.Variant.ToLower().Equals(variant.ToLower())).ToArray();
        }

        public bool LeaseResource(IResource resource, ITenant tenant, DateTime date)
        {
            resource.Availability = Domain.Enums.ResourceStatus.Occupied;
            resource.LeasedTo = tenant.Id;
            Ctx.Update(resource);
            Ctx.SaveChanges();
            return true;
        }

        public IResource LeaseResource(string variant, ITenant tenant, DateTime date)
        {
            IResource resource;
            var resources = GetResources(variant);
            if (resources.Length == 0)
                return null;

            // Wybierz ten, który najdłużej leży odłogiem i jest wolny
            resource = FilterUnavailableResources(resources).FirstOrDefault();

            resource.Availability = ResourceStatus.Occupied;
            resource.LeasedTo = tenant.Id;

            Ctx.Update(resource);
            Ctx.SaveChanges();
            return resource;
        }

        public void WithdrawResource(IResource resource, DateTime withdrawedOnDate)
        {
            // TODO handle the dates
            Ctx.Resources.Remove(resource as Resource);
            Ctx.SaveChanges();
        }
    }
}
