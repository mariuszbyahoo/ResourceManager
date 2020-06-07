using Microsoft.AspNetCore.Mvc;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Repos
{
    public class LeasingDataRepo : ILeasingDataRepo
    {
        public ManagerDbContext Context { get; set; }

        public LeasingDataRepo(ManagerDbContext context)
        {
            Context = context;
        }

        public async Task<IActionResult> AddDataAboutResource(IResourceData resourceData)
        {
            await Context.ResourceDatas.AddAsync(resourceData as ResourceData);
            await Context.SaveChangesAsync();
            return new OkObjectResult(0); // z tego co wiem, "exited with code 0" ogółem oznacza powodzenie, tak też robię
        }

        public async Task<IActionResult> AddDataAboutTenant(ITenantData tenantData)
        {
            await Context.TenantDatas.AddAsync(tenantData as TenantData);
            await Context.SaveChangesAsync();
            return new OkObjectResult(0);
        }

        public IResourceData GetDataAboutResource(Guid resourceId)
        {
            return Context.ResourceDatas.Where(r => r.Id.Equals(resourceId)).FirstOrDefault();
        }

        public ITenantData GetDataAboutTenant(Guid tenantId)
        {
            return Context.TenantDatas.Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
        }

        public IActionResult SetDataAboutResource(Guid resourceId, IResourceData newData)
        {
            var processed = GetDataAboutResource(resourceId);
            if (processed == null)
                throw new Exception("No resourceData found with such an Id");

            processed.Availability = newData.Availability;
            processed.LeasedTo = newData.LeasedTo;
            processed.OccupiedTill = newData.OccupiedTill;

            return new OkObjectResult(0);
        }
    }
}
