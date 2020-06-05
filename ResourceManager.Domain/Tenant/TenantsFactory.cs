using ResourceManager.Domain.Implementations;
using ResourceManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain
{
    public class TenantsFactory
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
