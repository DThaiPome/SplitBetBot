using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class UserStreaks : UserResponse
    {
        public string user { get; set; }
        public int streak { get; set; }

        public UserStreaks(string user, int streak) : base(user)
        {
            this.user = user;
            this.streak = streak;
        }
    }
}
