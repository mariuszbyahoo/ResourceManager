using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain.Implementations;
using System;

namespace ResourceManager.Data
{
    public class ResourceManagerDbContext : DbContext
    {
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        public ResourceManagerDbContext(DbContextOptions<ResourceManagerDbContext> options) : base(options)
        {

        }
    }
}
