using ResourceManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManager.Controllers
{
    /// <summary>
    /// Interfejs kontrolera, którego zadaniem jest zarządanie dzierżawcami dostępnymi dla aplikacji
    /// </summary>
    public interface ITenantsController
    {
        /// <summary>
        /// Dodaje dzierżawcę do puli
        /// </summary>
        /// <param name="Tenant">Nowy dzierżawca</param>
        void AddTenant(ITenant Tenant);

        /// <summary>
        /// Usuwa dzierżawcę z puli
        /// </summary>
        /// <param name="Id"></param>
        void RemoveTenant(Guid Id);

        /// <summary>
        /// Zmienia priorytet dzierżawcy, np. w przypadku gdy ten wykupił droższy abonament, lub odwrotnie.
        /// </summary>
        /// <param name="Id">ID dzierżawcy</param>
        /// <param name="NewPiority">Nowy priorytet dla dzierżawcy</param>
        void SetTenantsPriority(Guid Id, byte NewPiority);
    }
}
