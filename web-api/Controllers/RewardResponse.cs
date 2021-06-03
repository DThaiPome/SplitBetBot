using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class RewardResponse : APIResponse
    {
        public int seconds { get; set; }
        public List<UserRewards> rewards { get; set; }

        public RewardResponse(int seconds, List<UserRewards> rewards) : base(0)
        {
            this.seconds = seconds;
            this.rewards = rewards;
        }
    }
}
