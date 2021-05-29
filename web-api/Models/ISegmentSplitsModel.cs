using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplitBetBotCore.Models
{
    public interface ISegmentSplitsModel : ISplitsModel
    {
        /// <summary>
        /// Add a user's bet.
        /// </summary>
        /// <param name="name">username</param>
        /// <param name="bet">the segment time in seconds</param>
        /// <param name="amount">the amount of points being added to the pool</param>
        /// <exception cref="ArgumentException">if either are null, or if bet is an improper string</exception>
        /// <exception cref="InvalidOperationException">if betting is closed, or if the user has already bet</exception>
        void addPlayerBet(string name, int bet, int amount);

        /// <summary>
        /// Set the result of the split
        /// </summary>
        /// <param name="result">the segment time in seconds</param>
        /// <exception cref="ArgumentException">if the result is invalid</exception>
        /// <exception cref="InvalidOperationException">if the result has not yet been cleared</exception>
        void setResult(int result);

        /// <summary>
        /// Get the current point pool amount
        /// </summary>
        /// <returns>current point amount</returns>
        /// <exception cref="InvalidOperationException">if the betting is currently closed</exception>
        int pointPool { get; }

        Dictionary<string, int> streaks { get; }
    }
}
