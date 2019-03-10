using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CheckerApi.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    [Produces("application/json")]
    [Route("PoolHashrate")]
    public class PoolHashrateController: BaseController
    {
        private readonly IMapper _mapper;

        public PoolHashrateController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _mapper = serviceProvider.GetService<IMapper>();
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(Dictionary<string, IEnumerable<PoolHashrateDTO>>), 200)]
        public IActionResult GetHashrateData()
        {
            var data = Context.PoolHashratesReadOnly
                .ToList()
                .GroupBy(h => h.Name)
                .ToDictionary(
                    k => k.Key,
                    v => v.Select(_mapper.Map<PoolHashrateDTO>)
                        .OrderByDescending(d => d.Date)
                        .AsEnumerable()
                );

            return Ok(data);
        }
    }
}
