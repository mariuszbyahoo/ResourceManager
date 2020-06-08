using Microsoft.AspNetCore.Mvc;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Repos
{
    public interface ILeasingDataRepo
    {
        public ManagerDbContext Context { get; set; }

        public Task<IActionResult> AddDataAboutTenant(ITenantData tenantData);
        public Task<IActionResult> AddDataAboutResource(IResourceData resourceData);
        public ITenantData GetDataAboutTenant(Guid tenantId);
        public IResourceData GetDataAboutResource(Guid resourceId);
        public IActionResult SetDataAboutResource(Guid resourceId, IResourceData newData);
        public Task<IActionResult> WithdrawResourceData(Guid id, DateTime fromDate);
    }
}
