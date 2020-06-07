using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data.Repos;
using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    /// <summary>
    /// Interfejs zarządzający rekordami tabeli zawierającej dane zasobów i dzierżawców
    /// </summary>
    public interface ILeasingDataService
    {
        /// <summary>
        /// Prosty getter
        /// </summary>
        /// <param name="tenantsId">ID dzierżawcy, o którego email wniesiono</param>
        /// <returns></returns>
        Task<IActionResult> GetTenantsEmail(Guid tenantsId);

        /// <summary>
        /// Metoda dodaje nowy rekord do puli posiłkowej
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IActionResult> AddResource(Guid resourceId, DateTime date);

        /// <summary>
        /// Metoda usuwa istniejący rekord z puli posiłkowej
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IActionResult> WithdrawResource(Guid resourceId, DateTime date);

        /// <summary>
        /// Ustawia dane zasobu o podanym ID jako wydzierżawiony.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="tenantsId"></param>
        /// <param name="date">Data, od którego włącznie ten zasób ma być wydzierżawiony</param>
        /// <returns></returns>
        Task<IActionResult> LeaseResource(Guid resourceId, Guid tenantsId, DateTime date);

        /// <summary>
        /// Ustawia dane zasobu o podanym ID jako wolny.
        /// </summary>
        /// <param name="resourceId">ID zasobu </param>
        /// <param name="tenantsId">ID dzierżawcy</param>
        /// <param name="date">Data, od którego włącznie ten zasób ma być wolny</param>
        /// <returns></returns>
        Task<IActionResult> FreeResource(Guid resourceId, Guid tenantsId, DateTime date);
    }
}
