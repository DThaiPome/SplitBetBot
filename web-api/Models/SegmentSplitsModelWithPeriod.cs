using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Models
{
    public class SegmentSplitsModelWithPeriod : SegmentSplitsModel, ISegmentSplitsModelWithPeriod
    {
        private long bettingPeriod; // milliseconds
        private long betPeriodStartTimestamp; // milliseconds
        private float maxBonusMultiplier;

        public long bettingPeriodLength { get => this.bettingPeriod; set
            {
                if (value < 1 && value != -1)
                {
                    throw new ArgumentException("4");
                }
                this.bettingPeriod = value;
            }
        }

        public override int splitResult { get => base.splitResult; 
            set {
                base.splitResult = value;
                this.betPeriodStartTimestamp = GetMsNow();
            } 
        }

        public SegmentSplitsModelWithPeriod(int pointMinimum, float maxBonusMultiplier) : base(pointMinimum)
        {
            if (maxBonusMultiplier < 1)
            {
                throw new ArgumentException("Bonus multiplier cannot be less than 1");
            }
            this.maxBonusMultiplier = maxBonusMultiplier;

            this.bettingPeriod = -1;
        }

        public override void addPlayerBet(string name, int bet, int amount)
        {
            if (this.bettingPeriod != -1)
            {
                long ms = GetMsNow();
                long sinceStart = ms - this.betPeriodStartTimestamp;
                float ratio = (float)sinceStart / (float)this.bettingPeriod;
                ratio = ratio > 1 ? 1 : ratio;
                float multiplier = this.maxBonusMultiplier == 1 ? 1 : ((1 - ratio) * (this.maxBonusMultiplier - 1)) + 1;
                amount = (int)(amount * multiplier);
            }
            
            base.addPlayerBet(name, bet, amount);
        }

        public override bool isBettingOpen { get => base.isBettingOpen; 
            set 
            {
                base.isBettingOpen = value; 
                if (value)
                {
                    this.betPeriodStartTimestamp = GetMsNow();
                }
            }
        }

        private static long GetMsNow()
        {
            return (long)new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;
        }
    }
}
