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
        public DbSet<PoolHashrate> PoolHashrates { get; set; }

        public ApiConfiguration Configuration => Configurations.OrderBy(c => c.ID).FirstOrDefault();

        public IQueryable<BidEntry> DataReadOnly => Data.AsNoTracking();
        public ApiConfiguration ConfigurationReadOnly => Configurations.AsNoTracking().OrderBy(c => c.ID).FirstOrDefault();
        public IQueryable<ConditionSetting> ConditionSettingsReadOnly => ConditionSettings.AsNoTracking();
        public IQueryable<BidAudit> OrdersAuditsReadOnly => OrdersAudit.AsNoTracking();
        public IQueryable<PoolHashrate> PoolHashratesReadOnly => PoolHashrates.AsNoTracking();

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
                        EnableAudit = true,
                        TotalHashThreshold = 0.8,
                    }
                };

                AddRange(config);
            }

            foreach (var condition in Registry.GetConditions())
            {
                var name = condition.Name;
                var priority = Registry.GetPriority(condition);
                var dbCondition = ConditionSettings.FirstOrDefault(c => c.ConditionName == name);
                if (dbCondition == null)
                {
                    var setting = new ConditionSetting()
                    {
                        ConditionID = priority,
                        ConditionName = name,
                        Enabled = true
                    };

                    Add(setting);
                }
                else if (dbCondition.ConditionID != priority)
                {
                    dbCondition.ConditionID = priority;
                }
            }

            SaveChanges();
        }
    }
}