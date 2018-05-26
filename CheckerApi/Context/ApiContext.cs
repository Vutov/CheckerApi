using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CheckerApi.Context
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }

        public DbSet<BidEntry> Data { get; set; }
        public DbSet<ApiConfiguration> Configurations { get; set; }

        public void Seed()
        {
            if (!this.Configurations.Any())
            {
                var config = new List<ApiConfiguration>
                {
                    new ApiConfiguration()
                    {
                        AcceptedSpeed = 0.02,
                        LimitSpeed = 11,
                        PriceThreshold = 0.03,
                        LastNotification = DateTime.UtcNow.AddMinutes(-15)
                    }
                };

                this.AddRange(config);
                this.SaveChanges();
            }
        }
    }
}