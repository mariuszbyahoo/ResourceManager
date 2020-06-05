using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain.Models;
using System;

namespace ResourceManager.Data
{
    public class ManagerDbContext : DbContext
    {
        /// <summary>
        /// Zbiór zasobów dostępny w bazie danych
        /// </summary>
        public DbSet<Resource> Resources { get; set; }

        /// <summary>
        /// Zbiór dzierżawców dostępny w bazie danych
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; }

        public ManagerDbContext(DbContextOptions<ManagerDbContext> options) : base(options)
        {

        }
    }
}
