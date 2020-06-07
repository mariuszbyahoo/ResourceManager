using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public class ResourceDataFactory : IResourceDataFactory
    {
        public IResourceData CreateInstance(Guid resourceId, ResourceStatus availability, Guid leasedTo, DateTime occupiedTill)
        {
            // TODO może zabezpiecz, żeby nie było możliwe utworzenie nowego rekordu z ID które nie odpowiada żadnemu rekordowi z tabeli Resources??
            return new ResourceData() { Id = resourceId, Availability = availability, LeasedTo = leasedTo, OccupiedTill = occupiedTill };
        }
    }
}
