using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Models
{
    public interface ISplitsModel
    {
        /// <summary>
        /// Add a user's bet.
        /// </summary>
        /// <param name="name">username</param>
        /// <param name="bet">one of "behind", "tied", "ahead", or "gold"</param>
        /// <param name="amount">the amount of points being added to the pool</param>
        /// <exception cref="ArgumentException">if either are null, or if bet is an improper string</exception>
        /// <exception cref="InvalidOperationException">if betting is closed, or if the user has already bet</exception>
        void addPlayerBet(string name, string bet, int amount);

        /// <summary>
        /// Produce a list of users and their rewards, if a result has been found. Also resets the betting pool, and
        /// the found result.
        /// </summary>
        /// <returns>a set of pairs of usernames and their point rewards</returns>
        /// <exception cref="InvalidOperationException">if no result has been set yet</exception>
        Dictionary<string, int> rewardBets();

        /// <summary>
        /// Open or close betting
        /// </summary>
        /// <param name="open">true if the betting should be open, false otherwise</param>
        void setBettingOpen(bool open);

        /// <summary>
        /// Set the result of the split
        /// </summary>
        /// <param name="result">one of "behind", "tied", "ahead", or "gold"</param>
        /// <exception cref="ArgumentException">if the result is invalid</exception>
        /// <exception cref="InvalidOperationException">if the result has not yet been cleared</exception>
        void setResult(string result);
    }
}
