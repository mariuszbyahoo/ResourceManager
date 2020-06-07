using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public interface IResourceDataFactory
    {
        IResourceData CreateInstance(Guid resourceId, ResourceStatus Availability, Guid leasedTo, DateTime occupiedTill);
    }
}
