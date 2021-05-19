using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public class EmptyResponse : APIResponse
    {
        public EmptyResponse(int errorCode) : base(errorCode) { }
        public EmptyResponse() : this(0) { }
    }
}
