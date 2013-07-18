using HelloAngularJs.Models;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelloAngularJs.Controllers
{
    public class HomeController : Controller
    {
        private IOAuthCredentials credentials = new SessionStateCredentials();
        private MvcAuthorizer auth;
        private TwitterContext twitterCtx;

        public ActionResult Index()
        {
            var unAuthorized = Authorize();
            if (unAuthorized == null)
            {
                return View("Index");
            }
            else
            {
                return unAuthorized;
            }
        }

        private ActionResult Authorize()
        {
            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                credentials.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
            }

            auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri(Request.Url.ToString());
                return auth.BeginAuthorization(specialUri);
            }
            ViewBag.User = auth.Credentials.ScreenName;
            return null;
        }

        private string GetTweetMediaUrl(Status status)
        {
            if (status.Entities != null && status.Entities.MediaEntities.Count > 0)
            {
                return status.Entities.MediaEntities[0].MediaUrlHttps;
            }
            return "";
        }


        [HttpGet]
        public JsonResult Tweet()
        {
            Authorize();
            string screenName = ViewBag.User;
            IEnumerable<TweetViewModel> friendTweets = new List<TweetViewModel>();
            if (string.IsNullOrEmpty(screenName))
            {
                return Json(friendTweets, JsonRequestBehavior.AllowGet);
            }
            twitterCtx = new TwitterContext(auth);
            friendTweets =
                (from tweet in twitterCtx.Status
                 where tweet.Type == StatusType.Home &&
                       tweet.ScreenName == screenName &&
                       tweet.IncludeEntities == true
                 select new TweetViewModel
                 {
                     ImageUrl = tweet.User.ProfileImageUrl,
                     ScreenName = tweet.User.Identifier.ScreenName,
                     MediaUrl = GetTweetMediaUrl(tweet),
                     Tweet = tweet.Text
                 })
                .ToList();
            return Json(friendTweets, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult Tweet(string tweet)
        {
            Authorize();
            twitterCtx = new TwitterContext(auth);
            try
            {
                Status stat = twitterCtx.UpdateStatus(tweet);
                if (stat != null)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, errorMessage = "Unknown Error" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage= ex.Message });
            }
        }
    }
}
