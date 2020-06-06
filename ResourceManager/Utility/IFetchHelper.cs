using ResourceManager.Data;
using ResourceManager.Domain.Models;
using System;

namespace ResourceManager
{
    /// <summary>
    /// Utility class to obtain exact entity from dbContext
    /// </summary>
    public interface IFetchHelper
    {
        Tenant GetTenant(Guid Id, ManagerDbContext _ctx);
        Resource GetResource(Guid Id, ManagerDbContext _ctx);
        Resource[] GetResources(string variant, ManagerDbContext _ctx);

        Resource[] GetAvailableResources(Resource[] resources);
    }
}