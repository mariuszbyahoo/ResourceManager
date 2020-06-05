
using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public class TenantsFactory : ITenantsFactory
    {
        /// <summary>
        /// Tworzy nową instancję klasy Tenant
        /// </summary>
        /// <param name="Priority"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITenant CreateInstance(byte Priority, TenantType type)
        {
            switch (type) {
                case TenantType.Tenant:
                    return new Tenant
                    {
                        Priority = Priority
                    };
                default:
                    return null;
            }
        }
    }
}
