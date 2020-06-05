using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models.Interfaces;

namespace ResourceManager.Domain.Factories
{
    public interface IResourceFactory
    {
        IResource CreateInstance(string variant, ResourceType type);
    }
}