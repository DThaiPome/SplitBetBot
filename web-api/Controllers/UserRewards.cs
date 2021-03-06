using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class UserRewards : UserResponse
    {
        public string user { get; set; }
        public int points { get; set; }

        public UserRewards(string user, int points) : base(user)
        {
            this.user = user;
            this.points = points;
        }
    }
}
