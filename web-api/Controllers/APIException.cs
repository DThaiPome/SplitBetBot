using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class APIException : Exception
    {
        public APIResponse response { get; set; }

        public APIException(APIResponse response)
        {
            this.response = response;
        }
    }
}
