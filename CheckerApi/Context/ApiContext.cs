using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.Entities;
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
        public DbSet<BidAudit> OrdersAudit { get; set; }

        public ApiConfiguration Configuration => Configurations.OrderBy(c => c.ID).FirstOrDefault();

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
                        AcceptedPercentThreshold = 0.1,
                        EnableAudit = true
                    }
                };

                AddRange(config);
            }
            
            foreach (var condition in Registry.GetConditions())
            {
                var name = condition.Name;
                if (ConditionSettings.FirstOrDefault(c => c.ConditionName == name) == null)
                {
                    var priority = Registry.GetPriority(condition);
                    var setting = new ConditionSetting()
                    {
                        ConditionID = priority,
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