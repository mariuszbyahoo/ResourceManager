
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
        public ITenant CreateInstance(Guid Id, byte Priority, string typeName)
        {
            switch (typeName.ToLower()) {
                case "tenant":
                    return new Tenant
                    {
                        Id = Id,
                        Priority = Priority
                    };
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
