using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;

namespace ResourceManager.Domain.Factories
{
    public interface ITenantsFactory
    {
        ITenant CreateInstance(Guid Id, byte Priority, string typeName);
    }
}