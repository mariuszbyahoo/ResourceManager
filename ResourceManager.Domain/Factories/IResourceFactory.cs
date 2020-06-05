using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;

namespace ResourceManager.Domain.Factories
{
    public interface IResourceFactory
    {
        IResource CreateInstance(Guid Id, string variant, ResourceType type);
    }
}