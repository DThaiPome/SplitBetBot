using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class InvalidPointsResponse : APIResponse
    {
        public int points { get; set; }

        public InvalidPointsResponse(int points) : base(3)
        {
            this.points = points;
        }
    }
}
