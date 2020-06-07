using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data.Repos;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    public class LeasingDataService : ILeasingDataService
    {
        private ILeasingDataRepo _repo;

        public LeasingDataService(ILeasingDataRepo repo)
        {
            _repo = repo;
        }

        Task<ActionResult<string>> ILeasingDataService.GetTenantsEmail(Guid tenantsId)
        {
            throw new NotImplementedException();
        }

        Task ILeasingDataService.AddResource(Guid resourceId, DateTime date)
        {
            throw new NotImplementedException();
        }

        Task ILeasingDataService.WithdrawResource(Guid resourceId, DateTime date)
        {
            throw new NotImplementedException();
        }

        Task<ActionResult<IResource>> ILeasingDataService.LeaseResource(Guid resourceId, Guid tenantsId, DateTime date)
        {
            throw new NotImplementedException();
        }

        Task<ActionResult<IResource>> ILeasingDataService.FreeResource(Guid resourceId, Guid tenantsId, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
