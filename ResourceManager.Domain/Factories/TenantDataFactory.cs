using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public class TenantDataFactory : ITenantDataFactory
    {
        public ITenantData CreateInstance(Guid tenantId, string email)
        {
            /* Warto byłoby zabezpieczyć, żeby nie było możliwe utworzenie nowego rekordu z ID,
             * które nie odpowiada żadnemu rekordowi z tabeli Tenants, choć i tak to niemożliwe bo 
             * z uwagi na NullCheck'i poprzedzające wywołanie do tej metody. */
            return new TenantData() { Id = tenantId, EmailAddress = email };
        }
    }
}
