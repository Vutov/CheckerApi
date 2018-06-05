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
            if (!Configurations.Any())
            {
                var config = new List<ApiConfiguration>
                {
                    new ApiConfiguration()
                    {
                        AcceptedSpeed = 0.02,
                        LimitSpeed = 11,
                        PriceThreshold = 0.04,
                        LastNotification = DateTime.UtcNow.AddMinutes(-15),
                        MinimalAcceptedSpeed = 0.003,
                        AcceptedPercentThreshold = 0.1
                    }
                };

                AddRange(config);
            }

            foreach (var condition in Registry.Conditions)
            {
                var name = condition.Value.Name;
                if (ConditionSettings.FirstOrDefault(c => c.ConditionName == name) == null)
                {
                    var setting = new ConditionSetting()
                    {
                        ConditionID = condition.Key,
                        ConditionName = name,
                        Enabled = true
                    };

                    Add(setting);
                }
            }
            
            SaveChanges();
        }
    }
}