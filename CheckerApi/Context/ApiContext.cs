using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Data;
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
        public DbSet<ConditionSetting> ConditionSettings { get; set; }

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
                        PriceThreshold = 0.04,
                        LastNotification = DateTime.UtcNow.AddMinutes(-15),
                        MinimalAcceptedSpeed = 0.003
                    }
                };

                this.AddRange(config);

                var settings = new List<ConditionSetting>();
                foreach (ConditionNames name in Enum.GetValues(typeof(ConditionNames)))
                {
                    settings.Add(new ConditionSetting()
                    {
                        ConditionID = (int)name,
                        ConditionName = Enum.GetName(typeof(ConditionNames), (int)name),
                        Enabled = true
                    });
                }

                this.AddRange(settings);
                
                this.SaveChanges();
            }
        }
    }
}