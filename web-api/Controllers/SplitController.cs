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
        private ISplitsModel model;

        public SplitController()
        {
            this.model = Program.model;
        }

        [HttpPost]
        public string AddBet(string user, string bet, int points)
        {
            try
            {
                this.model.addPlayerBet(user, bet, points);
                return "Bet added!";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpGet]
        public List<UserRewards> GetRewards()
        {
            try
            {
                Dictionary<string, int> modelRewards;
                List<UserRewards> userRewards;
                modelRewards = this.model.rewardBets();
                userRewards = ConvertRewards(modelRewards);
                return userRewards;
            }
            catch (Exception e)
            {
                return new List<UserRewards>();
            }
        }

        [HttpPost]
        public string SetBettingOpen(bool open)
        {
            this.model.setBettingOpen(open);
            return open ? "Betting open!" : "Beting closed!";
        }

        public class UserRewards
        {
            public string user { get; set; }
            public int points { get; set; }
        }

        [HttpPost]
        public string OnSplit(string result)
        {
            try
            {
                this.model.setResult(result);
                return "Result set";
            }
            catch (Exception e)
            {
                return "Result already set";
            }
        }

        private List<UserRewards> ConvertRewards(Dictionary<string, int> modelRewards)
        {
            List<UserRewards> userRewards = new List<UserRewards>();
            foreach (KeyValuePair<string, int> pair in modelRewards)
            {
                UserRewards u = new UserRewards();
                u.user = pair.Key;
                u.points = pair.Value;
                userRewards.Add(u);
            }
            return userRewards;
        }

        /*

        // GET: SplitController
        public ActionResult Index()
        {
            return View();
        }

        // GET: SplitController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SplitController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SplitController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SplitController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SplitController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SplitController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SplitController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        */
    }
}
