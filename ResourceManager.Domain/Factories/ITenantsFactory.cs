using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;

namespace ResourceManager.Domain.Factories
{
    public interface ITenantsFactory
    {
        ITenant CreateInstance(byte Priority, TenantType type);
    }
}