using System;
using System.Linq;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    [Produces("application/json")]
    [Route("")]
    public class ConfigurationController : BaseController
    {
        public ConfigurationController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [HttpGet]
        [Route("{setting}/{rate}/{password?}")]
        public IActionResult SetSetting(string setting, double rate, string password = "")
        {
            if (Password != password || string.IsNullOrEmpty(setting))
                return NotFound();

            var (config, settings) = GetConfig();

            var settingProp = settings.FirstOrDefault(s => s.Name.ToLower() == setting.ToLower());
            if (settingProp == null)
                return NotFound();

            settingProp.SetValue(config, rate, null);
            config.AcceptedSpeed = rate;
            Context.Update(config);
            Context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("Condition/{condition}/{enabled}/{password?}")]
        public IActionResult SetSetting(string condition, bool enabled, string password = "")
        {
            if (Password != password || string.IsNullOrEmpty(condition))
                return NotFound();

            var conditionEntry = Context.ConditionSettings.FirstOrDefault(c => c.ConditionName == condition);
            if (conditionEntry == null)
                return NotFound();

            conditionEntry.Enabled = enabled;
            Context.Update(conditionEntry);
            Context.SaveChanges();

            return Ok(enabled);
        }

        [HttpGet]
        [Route("testnotifications/{password?}")]
        public IActionResult TestNotifications(string password = "")
        {
            if (Password != password)
                return NotFound();

            var notification = ServiceProvider.GetService<INotificationManager>();
            var result = notification.TriggerHook("Manual notification test, please ignore");
            if (result.HasFailed())
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        [Route("clearConditionsCache/{password?}")]
        public IActionResult ClearConditionsCache(string password = "")
        {
            if (Password != password)
                return NotFound();

            var conditions = Context.ConditionSettings.ToList();
            Context.ConditionSettings.RemoveRange(conditions);
            Context.SaveChanges();

            Context.Seed();

            return Ok(Context.ConditionSettings.ToList());
        }
    }
}