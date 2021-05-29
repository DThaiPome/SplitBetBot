using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class BettingOpenResponse : APIResponse
    {
        public bool open { get; set; }

        public BettingOpenResponse(bool open) : base(0)
        {
            this.open = open;
        }
    }
}
