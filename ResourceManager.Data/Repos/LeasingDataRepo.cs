using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data.Services;
using ResourceManager.Domain.Models;
using ResourceManager.Services;
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

        private IRemoveService _removeService;
        private ILoggerService _logger;

        public LeasingDataRepo(ManagerDbContext context, IRemoveService removeService, ILoggerService logger)
        {
            Context = context;
            _removeService = removeService;
            _logger = logger;
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

            Context.Update(processed);
            Context.SaveChanges();

            return new OkObjectResult(0);
        }

        public async Task<IActionResult> WithdrawResourceData(Guid id, DateTime fromDate)
        {
            try
            {
                var res = await _removeService.CheckDate(fromDate);
                Context.ResourceDatas.Remove(GetDataAboutResource(id) as ResourceData);
                await Context.SaveChangesAsync();
                return new OkObjectResult(0);
            }
            catch (Exception ex)
            {
                await _logger.LogToFile(ex, "errors.txt");
                return new StatusCodeResult(500);
            }
        }
    }
}
