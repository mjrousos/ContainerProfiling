using System;
using System.Runtime;

namespace AKSPerf.Models
{
    public class ProcessorInformation
    {
        public int ProcessorCount { get; set; }
        public int ThreadPoolMinWorkerThreads {get;set;}
        public int ThreadPoolMinCompletionPortThreads {get;set;}
        public int ThreadPoolMaxWorkerThreads { get; set; }
        public int ThreadPoolMaxCompletionPortThreads { get; set; }
        // public GCMemoryInfo GCMemoryInfo {get;set;}
        public bool GCServer { get; set;}
        public GCLatencyMode GCLatencyMode {get;set;}
        public int SpinCount {get;set;}
    }
}