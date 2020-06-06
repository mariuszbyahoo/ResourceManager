using ResourceManager.Data.Repos;
using System;
using System.Threading.Tasks;

namespace ResourceManager.Services
{
    public interface IRemoveService
    {
        Task<bool> CheckDate(DateTime withdrawalDate);
    }
}