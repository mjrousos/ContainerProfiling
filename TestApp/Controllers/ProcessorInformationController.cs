using System;
using System.Runtime;
using System.Threading;
using AKSPerf.Models;
using AKSPerf.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AKSPerf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessorInformationController : ControllerBase
    {
        private ILogger<ProcessorInformationController> Logger { get; }
        public SpinCounterService CounterService { get; }

        public ProcessorInformationController(ILogger<ProcessorInformationController> logger, SpinCounterService counterService)
        {
            CounterService = counterService;
            Logger = logger;
        }


        [HttpGet]
        public ActionResult<ProcessorInformation> Get()
        {
            var information = new ProcessorInformation
            {
                GCLatencyMode = GCSettings.LatencyMode,
                // GCMemoryInfo = GC.GetGCMemoryInfo(),
                GCServer = GCSettings.IsServerGC,
                ProcessorCount = Environment.ProcessorCount,
                SpinCount = CounterService.SpinCount
            };

            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
            information.ThreadPoolMinWorkerThreads = minWorkerThreads;
            information.ThreadPoolMinCompletionPortThreads = minCompletionPortThreads;

            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            information.ThreadPoolMaxWorkerThreads = maxWorkerThreads;
            information.ThreadPoolMaxCompletionPortThreads = maxCompletionPortThreads;

            return Ok(information);
        }
    }
}
