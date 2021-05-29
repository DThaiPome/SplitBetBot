using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class StreaksResponse : APIResponse
    {
        public List<UserStreaks> streaks { get; set; }

        public StreaksResponse(List<UserStreaks> streaks) : base(0)
        {
            this.streaks = streaks;
        }
    }
}
