using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public class ResourceDataFactory : IResourceDataFactory
    {
        /// <summary>
        /// Zakładam, że można jak najbardziej wprowadzić do puli nowy zasób, ale będzie on w trakcie dostarczenia do dostawcy, więc nie może być w tym czasie wydzierżawiony
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="occupiedTill"></param>
        /// <param name="tenant">Jeśli skracamy okres dostarczenia zasobu do puli, należy przekazać w tym miejscu null</param>
        /// <returns></returns>
        public IResourceData CreateInstance(Guid resourceId, DateTime occupiedTill, ITenant tenant, string typeName)
        {
            Guid leasedTo = (tenant != null) ? tenant.Id : Guid.Empty;
            switch (typeName)
            {
                case "ResourceData":
                    // TODO może zabezpiecz, żeby nie było możliwe utworzenie nowego rekordu z ID które nie odpowiada żadnemu rekordowi z tabeli Resources??
                    ResourceStatus status;
                    if (occupiedTill > DateTime.Now)
                        status = ResourceStatus.Occupied;
                    else
                        status = ResourceStatus.Available;
                    return new ResourceData() { Id = resourceId, Availability = status, LeasedTo = leasedTo, OccupiedTill = occupiedTill };

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
