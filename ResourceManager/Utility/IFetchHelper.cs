using ResourceManager.Data;
using ResourceManager.Domain.Models;
using System;

namespace ResourceManager
{
    /// <summary>
    /// Utility class to obtain exact entity from dbContext
    /// </summary>
    internal interface IFetchHelper
    {
        Tenant GetTenantById(Guid Id, ManagerDbContext _ctx);
        Resource GetResourceById(Guid Id, ManagerDbContext _ctx);
    }
}