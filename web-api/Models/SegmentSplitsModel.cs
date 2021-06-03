using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Models
{
    public class SegmentSplitsModel : ISegmentSplitsModel
    {
        private Dictionary<string, int> userBets;
        private Dictionary<string, int> userStreaks;
        private int result;
        private bool resultSet;

        private int points;

        public int splitResult
        {
            get
            {
                if (!this.resultSet)
                {
                    throw new InvalidOperationException("5");
                } else
                {
                    return this.result;
                }
            }
            set
            {
                this.setResult(value);
            }
        }

        public int pointPool { 
            get {
                return this.points;
            } 
            private set {
                this.points = value;
            } 
        }

        public Dictionary<string, int> streaks
        {
            get
            {
                return new Dictionary<string, int>(this.userStreaks);
            }
            private set
            {
                this.userStreaks = value; 
            }
        }

        public bool isBettingOpen { get; set; }

        private int pointMinimum;
        
        public SegmentSplitsModel(int pointMinimum)
        {
            this.userBets = new Dictionary<string, int>();
            this.userStreaks= new Dictionary<string, int>();
            this.isBettingOpen = false;
            this.pointPool = 0;
            this.resultSet = false;

            this.pointMinimum = pointMinimum;
        }

        public void addPlayerBet(string name, int bet, int amount)
        {
            this.validateBetInputs(name, bet, amount);

            this.points += amount;
            this.userBets.Add(name, bet);
        }

        private void validateBetInputs(string name, int bet, int amount)
        {
            if (!this.isBettingOpen)
            {
                throw new InvalidOperationException("1"); // Betting closed
            }
            if (name == null)
            {
                throw new ArgumentException("-1"); // Name is null
            }
            if (this.userBets.ContainsKey(name))
            {
                throw new ArgumentException("2"); // User exists
            }
            if (amount < pointMinimum)
            {
                throw new ArgumentException("3"); // Too few points
            }
            if (bet < 0)
            {
                throw new ArgumentException("4");
            }

            return;
        }

        //TODO: Deprecated
        public void addPlayerBet(string name, string bet, int amount)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, int> rewardBets()
        {
            if (!this.resultSet)
            {
                throw new InvalidOperationException("5");
            }
            List<string> winningUsers = this.getWinningUsers();
            this.userStreaks = this.NewStreaks(winningUsers);
            Dictionary<string, int> rewards = this.GetRewards(winningUsers);

            this.result = -1;
            this.resultSet = false;
            this.points = 0;
            this.userBets = new Dictionary<string, int>();
            return rewards;
        }

        private Dictionary<string, int> NewStreaks(List<string> winners)
        {
            Dictionary<string, int> old = this.userStreaks;
            Dictionary<string, int> newStreaks = new Dictionary<string, int>();
            foreach(string winner in winners)
            {
                if (old.TryGetValue(winner, out int streak))
                {
                    newStreaks.Add(winner, streak + 1);
                } else
                {
                    newStreaks.Add(winner, 1);
                }
            }

            return newStreaks;
        }

        private Dictionary<string, int> GetRewards(List<string> users)
        {
            Dictionary<string, int> rewards = new Dictionary<string, int>();
            if (users.Count == 0)
            {
                return rewards;
            }
            int pointsPerUser = this.points / users.Count;
            foreach(string user in users)
            {
                int reward = pointsPerUser;
                if (this.userStreaks.TryGetValue(user, out int streak))
                {
                    reward += this.GetPointBonus(streak);
                }
                rewards.Add(user, reward);
            }
            return rewards;
        }

        private int GetPointBonus(int streak)
        {
            return 20 * (streak - 1);
        }

        private List<string> getWinningUsers()
        {
            int closestDiff = Abs(this.result - this.userBets.Values.Aggregate(-1, (acc, x) =>
                this.getClosestTime(this.result, acc, x)));
            return (this.userBets.Keys.Where(x =>
                this.userBets.TryGetValue(x, out int n) && Abs(this.result - n) == closestDiff)).ToList();
        }

        private int getClosestTime (int segment, int closest, int x)
        {
            if (closest == -1)
            {
                return x;
            } else
            {
                int a = Abs(segment - closest);
                int b = Abs(segment - x);
                return b < a ? x : closest;
            }
        }

        private static int Abs(int i)
        {
            return i < 0 ? -i : i;
        }

        public void setBettingOpen(bool open)
        {
            this.isBettingOpen = open;
        }

        private void setResult(int result)
        {
            validateResultInputs(result);

            this.result = result;
            this.resultSet = true;
        }
        
        private void validateResultInputs(int result)
        {
            if (this.resultSet)
            {
                throw new InvalidOperationException("5");
            }
            if (result < 0)
            {
                throw new ArgumentException("4");
            }
        }

        //TODO: Deprecated
        public void setResult(string result)
        {
            throw new NotImplementedException();
        }
    }
}
