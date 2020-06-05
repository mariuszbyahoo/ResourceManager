using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;

namespace ResourceManager.Domain.Factories
{
    public interface IResourceFactory
    {
        IResource CreateInstance(string variant, ResourceType type);
    }
}