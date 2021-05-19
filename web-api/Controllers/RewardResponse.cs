using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class RewardResponse : APIResponse
    {
        public List<UserRewards> rewards { get; set; }

        public RewardResponse(List<UserRewards> rewards) : base(0)
        {
            this.rewards = rewards;
        }
    }
}
