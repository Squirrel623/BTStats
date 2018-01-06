using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public interface IMetric
    {
        void HandleLogMessage(TimestampMessage message);
    }
}
