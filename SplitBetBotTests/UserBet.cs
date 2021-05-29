using System;
using System.Collections.Generic;
using System.Text;

namespace SplitBetBotTests
{
    class UserBet
    {
        public string user { get; set; }
        public int bet { get; set; }
        public int points { get; set; }

        public UserBet(string user, int bet, int points)
        {
            this.user = user;
            this.bet = bet;
            this.points = points;
        }
    }
}
