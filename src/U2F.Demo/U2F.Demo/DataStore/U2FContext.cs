using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using U2F.Demo.Models;

namespace U2F.Demo.DataStore
{
    public sealed class U2FContext : IdentityDbContext<User>
    {
        private readonly ILogger<U2FContext> _logger;

        public U2FContext(DbContextOptions<U2FContext> options, ILogger<U2FContext> logger)
           : base(options)
        {
            _logger = logger;
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                // zero means no items were saved to the DB
                return 0;
            }
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        public DbSet<AuthenticationRequest> AuthenticationRequests { get; set; }        
    }
}