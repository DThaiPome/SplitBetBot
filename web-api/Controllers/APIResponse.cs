using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Controllers
{
    public abstract class APIResponse
    {
        /// <summary>
        /// Potential errors:
        /// 
        ///-1: UnhandledError - misc.
        /// 0: None - No error
        /// 1: InvalidBettingState - Betting is closed/open
        /// 2: UserExists - User has a bet recorded already
        /// 3: InvalidPoints - Invalid number of points
        /// 4: InvalidBet - Bet string is invalid
        /// 5: InvalidSplitState- a split result already exists/does not exist
        /// </summary>
        public int error { get; set; }

        public APIResponse(int errorCode)
        {
            this.error = errorCode;
        }
    }
}
