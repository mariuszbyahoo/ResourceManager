using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public interface ITenantDataFactory
    {
        ITenantData CreateInstance(Guid tenantId, string email);
    }
}
