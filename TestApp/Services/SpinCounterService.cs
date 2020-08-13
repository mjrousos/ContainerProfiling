using System.Threading;
using Microsoft.Extensions.Logging;

namespace AKSPerf.Services
{
    public class SpinCounterService
    {
        public int spinCount;
        public int SpinCount => spinCount;
        public ILogger<SpinCounterService> Logger { get; }


        public SpinCounterService(ILogger<SpinCounterService> logger)
        {
            spinCount = 0;
            Logger = logger;
        }

        public int IncrementSpinCount() => Interlocked.Increment(ref spinCount);
    }
}