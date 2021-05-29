using System;
using System.Collections.Generic;
using System.Text;
using SplitBetBotCore.Models;
using NUnit.Framework;

namespace SplitBetBotTests
{
    public class ModelTests
    {
        private ISegmentSplitsModel model;
        private ISegmentSplitsModel closedModel;
        private List<UserBet> validBets = new List<UserBet>()
        {
            new UserBet("dthai", 120, 100),
            new UserBet("hickory", 240, 150),
            new UserBet("pentsworth", 120, 250),
            new UserBet("abba", 20, 50)
        };
        private List<UserBet> invalidBets = new List<UserBet>()
        {
            new UserBet("zerop", 100, 0),
            new UserBet("fewp", 100, 10),
            new UserBet("negb", -20, 500),
        };

        [SetUp]
        public void Setup()
        {
            this.model = new SegmentSplitsModel(50);
            this.closedModel = new SegmentSplitsModel(50);
            this.model.setBettingOpen(true);
        }

        private static void PlaceBet(ISegmentSplitsModel model, UserBet bet)
        {
            model.addPlayerBet(bet.user, bet.bet, bet.points);
        }

        private static void PlaceBets(ISegmentSplitsModel model, List<UserBet> bets)
        {
            foreach(UserBet bet in bets)
            {
                PlaceBet(model, bet);
            }
        }

        private static Dictionary<string, int> OnSplit(ISegmentSplitsModel model, int result)
        {
            model.setResult(result);
            return model.rewardBets();
        }

        private static int SumBets(params UserBet[] bets)
        {
            int sum = 0;
            foreach(UserBet bet in bets)
            {
                sum += bet.points;
            }
            return sum;
        }

        [Test]
        public void NoBets()
        {
            Dictionary<string, int> rewards = OnSplit(this.model, 100);
            Assert.AreEqual(0, rewards.Count);
        }

        [Test]
        public void OneBet()
        {
            PlaceBet(this.model, this.validBets[0]);
            Dictionary<string, int> rewards = OnSplit(this.model, 10000);
            Assert.AreEqual(1, rewards.Count);
            int points;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points));
            Assert.AreEqual(this.validBets[0].points, points);
        }

        [Test]
        public void OneBetThreeStreak()
        {
            PlaceBet(this.model, this.validBets[1]);
            Dictionary<string, int> rewards = OnSplit(this.model, 100320);
            Assert.AreEqual(1, rewards.Count);
            int points;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points));
            Assert.AreEqual(this.validBets[1].points, points);

            PlaceBet(this.model, this.validBets[1]);
            rewards = OnSplit(this.model, 234);
            Assert.AreEqual(1, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points));
            Assert.AreEqual(this.validBets[1].points + 20, points);

            PlaceBet(this.model, this.validBets[1]);
            rewards = OnSplit(this.model, 3);
            Assert.AreEqual(1, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points));
            Assert.AreEqual(this.validBets[1].points + 40, points);
        }

        [Test]
        public void OneBetSkipStreak()
        {
            PlaceBet(this.model, this.validBets[1]);
            Dictionary<string, int> rewards = OnSplit(this.model, 100320);
            Assert.AreEqual(1, rewards.Count);
            int points;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points));
            Assert.AreEqual(this.validBets[1].points, points);

            PlaceBet(this.model, this.validBets[1]);
            rewards = OnSplit(this.model, 234);
            Assert.AreEqual(1, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points));
            Assert.AreEqual(this.validBets[1].points + 20, points);

            rewards = OnSplit(this.model, 3);
            Assert.AreEqual(0, rewards.Count);

            PlaceBet(this.model, this.validBets[1]);
            rewards = OnSplit(this.model, 53);
            Assert.AreEqual(1, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points));
            Assert.AreEqual(this.validBets[1].points, points);
        }

        [Test]
        public void TwoBetsDiff()
        {
            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[1]);
            int pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[1]), pointPool);
            Dictionary<string, int> rewards = OnSplit(this.model, this.validBets[0].bet);
            Assert.AreEqual(1, rewards.Count);
            int points;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points));
            Assert.AreEqual(pointPool, points);
        }

        [Test]
        public void TwoBetsTie()
        {
            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            int pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            Dictionary<string, int> rewards = OnSplit(this.model, 250);
            Assert.AreEqual(2, rewards.Count);
            int points1;
            int points2;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2, points1);
            Assert.AreEqual(pointPool / 2, points2);
        }

        [Test]
        public void TwoStreaksParallel()
        {
            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            int pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            Dictionary<string, int> rewards = OnSplit(this.model, this.validBets[0].bet);
            Assert.AreEqual(2, rewards.Count);
            int points1;
            int points2;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2, points1);
            Assert.AreEqual(pointPool / 2, points2);

            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            rewards = OnSplit(this.model, this.validBets[0].bet);
            Assert.AreEqual(2, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2 + 20, points1);
            Assert.AreEqual(pointPool / 2 + 20, points2);

            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            rewards = OnSplit(this.model, this.validBets[0].bet);
            Assert.AreEqual(2, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2 + 40, points1);
            Assert.AreEqual(pointPool / 2 + 40, points2);
        }

        [Test]
        public void TwoStreaksDiffStreaks()
        {
            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            int pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            Dictionary<string, int> rewards = OnSplit(this.model, this.validBets[0].bet);
            Assert.AreEqual(2, rewards.Count);
            int points1;
            int points2;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2, points1);
            Assert.AreEqual(pointPool / 2, points2);

            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            rewards = OnSplit(this.model, this.validBets[0].bet);
            Assert.AreEqual(2, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2 + 20, points1);
            Assert.AreEqual(pointPool / 2 + 20, points2);

            PlaceBet(this.model, this.validBets[1]);
            PlaceBet(this.model, this.validBets[2]);
            pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[1], this.validBets[2]), pointPool);
            rewards = OnSplit(this.model, this.validBets[2].bet);
            Assert.AreEqual(1, rewards.Count);
            Assert.IsFalse(rewards.TryGetValue(this.validBets[1].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool + 40, points2);

            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[2]);
            pointPool = this.model.pointPool;
            Assert.AreEqual(SumBets(this.validBets[0], this.validBets[2]), pointPool);
            rewards = OnSplit(this.model, this.validBets[2].bet);
            Assert.AreEqual(2, rewards.Count);
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points2));
            Assert.AreEqual(pointPool / 2, points1);
            Assert.AreEqual(pointPool / 2 + 60, points2);
        }

        [Test]
        public void BetAfterInvalidBet()
        {
            try
            {
                PlaceBet(this.model, this.invalidBets[0]);
            } catch(Exception e)
            {
                PlaceBet(this.model, this.validBets[3]);
                Dictionary<string, int> rewards = OnSplit(this.model, 200);
                Assert.AreEqual(1, rewards.Count);
                int points;
                Assert.IsTrue(rewards.TryGetValue(this.validBets[3].user, out points));
                Assert.AreEqual(this.validBets[3].points, points);
            }
        }

        [Test]
        public void BetWhenClosed()
        {
            try
            {
                PlaceBet(this.closedModel, this.validBets[2]);
                Assert.Fail();
            } catch(InvalidOperationException e)
            {
                Assert.AreEqual("1", e.Message);
            }
        }

        [Test]
        public void InvalidBets()
        {
            foreach(UserBet bet in this.invalidBets)
            {
                try
                {
                    PlaceBet(this.model, bet);
                    Assert.Fail();
                } catch (ArgumentException e)
                {
                    continue;
                }
            }
        }

        [Test]
        public void DuplicateBets()
        {
            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[1]);
            try
            {
                PlaceBet(this.model, this.validBets[0]);
                Assert.Fail();
            } catch (ArgumentException e)
            {
                Assert.AreEqual("2", e.Message);
            }
        }

        [Test]
        public void TieWithDiffBets()
        {
            PlaceBet(this.model, this.validBets[0]);
            PlaceBet(this.model, this.validBets[1]);
            int points = this.model.pointPool;
            Dictionary<string, int> rewards = OnSplit(this.model, (this.validBets[0].bet + this.validBets[1].bet) / 2);
            Assert.AreEqual(2, rewards.Count);
            int points1;
            int points2;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points2));
            Assert.AreEqual(points / 2, points1);
            Assert.AreEqual(points / 2, points2);
        }

        [Test]
        public void ManyBets()
        {
            PlaceBets(this.model, this.validBets);
            int points = this.model.pointPool;
            Dictionary<string, int> rewards = OnSplit(this.model, 200);
            Assert.AreEqual(1, rewards.Count);
            int points1;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points1));
            Assert.IsFalse(rewards.TryGetValue(this.validBets[0].user, out int x));
            Assert.IsFalse(rewards.TryGetValue(this.validBets[2].user, out int y));
            Assert.IsFalse(rewards.TryGetValue(this.validBets[3].user, out int z));

            Assert.AreEqual(points, points1);

            PlaceBets(this.model, this.validBets);
            points = this.model.pointPool;
            rewards = OnSplit(this.model, 180);
            Assert.AreEqual(3, rewards.Count);
            int points2;
            int points3;
            Assert.IsTrue(rewards.TryGetValue(this.validBets[1].user, out points1));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[0].user, out points2));
            Assert.IsTrue(rewards.TryGetValue(this.validBets[2].user, out points3));
            Assert.IsFalse(rewards.TryGetValue(this.validBets[3].user, out int w));

            Assert.AreEqual(points / 3, points2);
            Assert.AreEqual(points / 3, points3);
            Assert.AreEqual(points / 3 + 20, points1);
        }
    }
}
