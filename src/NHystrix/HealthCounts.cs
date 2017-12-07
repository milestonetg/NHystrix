using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix
{
    public class HealthCounts
    {
        public long RequestCount { get; set; }

        public long FailedRequestCount { get; set; }

        public long FailurePercentage { get => (FailedRequestCount / RequestCount) * 100; }
    }
}
