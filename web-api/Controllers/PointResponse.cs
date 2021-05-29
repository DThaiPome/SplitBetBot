using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class PointResponse : APIResponse
    {
        public int points {get; set;}

        public PointResponse(int points) : base(0)
        {
            this.points = points;
        }
    }
}
