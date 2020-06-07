using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data.Repos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    public class LeasingDataService : ILeasingDataService
    {
        private IResourceRepo _resources;
        private ITenantRepo _tenants;

        public LeasingDataService(IResourceRepo resources, ITenantRepo tenants)
        {
            _resources = resources;
            _tenants = tenants;
        }

        public Task<IActionResult> AddResource(Guid resourceId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> FreeResource(Guid resourceId, Guid tenantsId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetTenantsEmail(Guid tenantsId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> LeaseResource(Guid resourceId, Guid tenantsId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> WithdrawResource(Guid resourceId, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
