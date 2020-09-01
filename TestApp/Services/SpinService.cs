using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AKSPerf.Services
{
    public class SpinService : BackgroundService
    {
        public int aStatic = 0;
        public ILogger<SpinService> Logger { get; }
        public SpinCounterService CountService { get; }

        public SpinService(ILogger<SpinService> logger, SpinCounterService countService)
        {
            Logger = logger;
            CountService = countService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            Task.Run(() =>
            {
                Logger.LogInformation("Spin service is running");

                while (!stoppingToken.IsCancellationRequested)
                {
                    RecSpin(20);
                }
            });

        // Spin for 'timeSec' seconds.   We do only 1 second in this
        // method, doing the rest in the helper.   
        void RecSpin(int timeSec)
        {
            AllocateStrings();
            if (timeSec <= 0)
                return;
            --timeSec;
            SpinForASecond();
            RecSpinHelper(timeSec);
        }

        // RecSpinHelper is a clone of RecSpin.   It is repeated 
        // to simulate mutual recursion (more interesting example)
        void RecSpinHelper(int timeSec)
        {
            if (timeSec <= 0)
                return;
            --timeSec;
            SpinForASecond();
            RecSpin(timeSec);
        }

        // SpingForASecond repeatedly calls DateTime.Now until for
        // 1 second.  It also does some work of its own in this
        // methods so we get some exclusive time to look at.  
        void SpinForASecond()
        {
            CountService.IncrementSpinCount();
            aStatic = 0;
            DateTime start = DateTime.Now;
            for (; ; )
            {
                if ((DateTime.Now - start).TotalSeconds > 1)
                    break;

                // Do some work in this routine as well.   
                for (int i = 0; i < 10; i++)
                    aStatic += i;
            }
        }

        void AllocateStrings()
        {
            var fileName = "AKSPerf.deps.json";
            if (File.Exists(fileName))
            {
                var s = File.ReadAllText(fileName);
            }
        }
    }
}
