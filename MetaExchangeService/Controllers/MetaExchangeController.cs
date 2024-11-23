using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetaExchangeService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetaExchangeController : ControllerBase
    {

        private readonly ILogger<MetaExchangeController> _logger;

        public MetaExchangeController(ILogger<MetaExchangeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public string Post(string type, decimal ammount)
        {
            return "haha " + type + " " + ammount;
        }
    }
}
