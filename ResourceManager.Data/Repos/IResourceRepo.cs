using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Data.Repos
{
    public interface IResourceRepo
    {
        public ManagerDbContext Ctx { get; set; }
        Resource GetResource(Guid id);
        Resource[] GetResources(string variant);
        Resource[] GetAvailableResources(Resource[] resources);

        void AddResource(IResource resource, DateTime availableFromDate);
        void WithdrawResource(IResource resource, DateTime withdrawedOnDate);
        bool FreeResource(IResource resource, ITenant tenant, DateTime date);
        bool LeaseResource(IResource resource, ITenant tenant, DateTime date);
        IResource LeaseResource(string variant, ITenant tenant, DateTime date);
        
    }
}
