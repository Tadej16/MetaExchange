using MetaExchangeConsole.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string Post(string type, decimal ammount, string path = null)
        {
            try
            {
                var ret = MetaExhangeCalculator.Instance.FindBestAll(type, ammount, null, path);
                return JsonConvert.SerializeObject(ret, Formatting.Indented);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return e.Message;
            }
        }
    }
}
