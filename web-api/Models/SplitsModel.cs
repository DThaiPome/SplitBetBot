using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Models
{
    public class SplitsModel : ISplitsModel
    {
        private Dictionary<string, string> users;
        private bool isOpen;
        private int points;
        private string result;

        private int pointMinimum;
        private string[] validBets;

        public SplitsModel(int pointMinimum)
        {
            this.users = new Dictionary<string, string>();
            this.isOpen = false;
            this.points = 0;
            this.result = "";

            this.pointMinimum = pointMinimum > 0 ? pointMinimum : 0;
            this.validBets = new string[4]
            {
                "behind",
                "tied",
                "ahead",
                "gold"
            };
        }

        public void addPlayerBet(string name, string bet, int amount)
        {
            this.validateBetInputs(name, bet, amount);

            this.points += amount;
            this.users.Add(name, bet);
        }

        private void validateBetInputs(string name, string bet, int amount)
        {
            if (!this.isOpen)
            {
                throw new ArgumentException("Betting is closed!");
            }
            if (name == null)
            {
                throw new ArgumentException("Name cannot be null!");
            }
            if (this.users.ContainsKey(name))
            {
                throw new ArgumentException("User has already made a bet!");
            }
            if (amount < pointMinimum)
            {
                throw new ArgumentException("Cannot bet less than " + this.pointMinimum + " points!");
            }
            if (!this.validBets.Contains(bet))
            {
                throw new ArgumentException("Must be a valid bet!");
            }

            return;
        }

        public Dictionary<string, int> rewardBets()
        {
            Dictionary<string, int> rewards = new Dictionary<string, int>();
            List<string> winners = new List<string>();
            foreach (KeyValuePair<string, string> pair in this.users)
            {
                string user = pair.Key;
                string bet = pair.Value;

                if (bet == this.result)
                {
                    winners.Add(user);
                }

                rewards.Add(user, 0);
            }
            int count = winners.Count;
            int rewardPerWinner = count == 0 ? 0 : this.points / count;

            foreach (string u in winners)
            {
                rewards.Remove(u);
                rewards.Add(u, rewardPerWinner);
            }

            this.users = new Dictionary<string, string>();
            this.points = 0;
            this.result = "";

            return rewards;
        }

        public void setBettingOpen(bool open)
        {
            this.isOpen = open;
        }

        public void setResult(string result)
        {
            validateResultInputs(result);

            this.result = result;
        }

        private void validateResultInputs(string result)
        {
            if (this.result != "")
            {
                throw new InvalidOperationException("Result already exists!");
            }
            if (!this.validBets.Contains(result))
            {
                throw new ArgumentException("Must be a valid bet!");
            }

            return;
        }
    }
}
