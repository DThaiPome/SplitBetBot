using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Models
{
    public interface ISegmentSplitsModelWithPeriod : ISegmentSplitsModel
    {
        /// <summary>
        /// Set or get the length of the current betting period, in milliseconds. Initially -1.
        /// -1 Means there is no limit on length.
        /// </summary>
        long bettingPeriodLength { get; set; }
    }
}
