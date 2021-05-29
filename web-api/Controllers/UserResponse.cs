using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class UserResponse
    {
        public string user { get; set; }
        
        public UserResponse(string user)
        {
            this.user = user;
        }
    }
}
