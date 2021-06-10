using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SplitBetBotCore.Models;

namespace SplitBetBotCore.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SplitController : Controller
    {
        private ISegmentSplitsModel model;

        private enum ErrorCode
        {
            UnhandledError = -1, None,
            InvalidBettingState, UserExists,
            InvalidPoints, InvalidBet,
            InvalidSplitState
        }

        public SplitController()
        {
            this.model = Program.model;
        }

        [HttpPost]
        public APIResponse AddBet(string user, int bet, int points)
        {
            try
            {
                this.model.addPlayerBet(user, bet, points);
                return new EmptyResponse();
            }
            catch (Exception e)
            {
                ErrorCode code = this.convertCode(e.Message);
                if (code == ErrorCode.InvalidPoints)
                {
                    return new InvalidPointsResponse(50);
                } else
                {
                    return new EmptyResponse((int)this.convertCode(e.Message));
                }
            }
        }

        [HttpGet]
        public APIResponse GetRewards()
        {
            try
            {
                int result = this.model.splitResult;
                Dictionary<string, int> modelRewards;
                List<UserRewards> userRewards;
                modelRewards = this.model.rewardBets();
                userRewards = ConvertRewards(modelRewards);
                return new RewardResponse(result, userRewards);
            }
            catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        [HttpPost]
        public APIResponse SetBettingOpen(bool open)
        {
            try
            {
                this.model.isBettingOpen = open;
                return new EmptyResponse();
            } catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        [HttpGet]
        public APIResponse GetBettingOpen()
        {
            try
            {
                return new BettingOpenResponse(this.model.isBettingOpen);
            } catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        [HttpPost]
        public APIResponse OnSplit(int result)
        {
            try
            {
                this.model.splitResult = result;
                return new EmptyResponse();
            }
            catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        private List<UserRewards> ConvertRewards(Dictionary<string, int> modelRewards)
        {
            List<UserRewards> userRewards = new List<UserRewards>();
            foreach (KeyValuePair<string, int> pair in modelRewards)
            {
                if (pair.Value > 0)
                {
                    UserRewards u = new UserRewards(pair.Key, pair.Value);
                    userRewards.Add(u);
                }
            }
            return userRewards;
        }

        private List<UserStreaks> ConvertStreaks(Dictionary<string, int> modelStreaks)
        {
            List<UserStreaks> userStreaks = new List<UserStreaks>();
            foreach(KeyValuePair<string, int> pair in modelStreaks)
            {
                if (pair.Value > 0)
                {
                    UserStreaks u = new UserStreaks(pair.Key, pair.Value);
                    userStreaks.Add(u);
                }
            }
            return userStreaks;
        }

        [HttpGet]
        public APIResponse PointPool()
        {
            try
            {
                return new PointResponse(this.model.pointPool);
            } catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        [HttpGet]
        public APIResponse UserStreaks()
        {
            try
            {
                return new StreaksResponse(this.ConvertStreaks(this.model.streaks));
            } catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        [HttpPost]
        public APIResponse SetBettingPeriod(int time)
        {
            try
            {
                ISegmentSplitsModelWithPeriod periodModel = (ISegmentSplitsModelWithPeriod)this.model;
                periodModel.bettingPeriodLength = 1000 * time;
                return new EmptyResponse(); 
            } catch (Exception e)
            {
                return new EmptyResponse((int)this.convertCode(e.Message));
            }
        }

        private ErrorCode convertCode(string code)
        {
            if (Int32.TryParse(code, out int x))
            {
                return (ErrorCode)x;
            } else
            {
                return ErrorCode.UnhandledError;
            }
        }
    }
}
