using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.DTO;
using Microsoft.EntityFrameworkCore;

namespace CheckerApi
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }

        public DbSet<DataDB> Data { get; set; }
        public DbSet<Configuration> Configurations { get; set; }

        public void Seed()
        {
            if (!this.Configurations.Any())
            {
                var config = new List<Configuration>
                {
                    new Configuration()
                    {
                        AcceptedSpeed = 0.02,
                        LimitSpeed = 11,
                        PriceThreshold = 0.03,
                        LastNotification = DateTime.UtcNow.AddMinutes(-5)
                    }
                };

                this.AddRange(config);
                this.SaveChanges();
            }
        }
    }
}