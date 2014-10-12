using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcTNPT.Models;

using MongoDB.Bson;

namespace MvcTNPT.Controllers
{
    public class RemarkController : Controller
    {
        public ActionResult Index(string id)
        {
            // Get remarks by id
            var user = MongoWrapper.GetDatabase().
                            GetCollection("users").
                            FindOneByIdAs<User>(ObjectId.Parse(id));

            return View(user);
        }

        [HttpPost]
        public ActionResult Index(string id, string newRemark)
        {
            var users = MongoWrapper.GetDatabase().GetCollection("users");
            var user = users.FindOneById(ObjectId.Parse(id));

            var remark = new BsonDocument().
                                Add("content", newRemark).
                                Add("date", DateTime.Now);

            if (user.Contains("remarks"))
            {
                user["remarks"].AsBsonArray.Add(BsonValue.Create(remark));
            }
            else
            {
                user["remarks"] = new BsonArray().Add(BsonValue.Create(remark));
            }

            users.Save(user);

            return RedirectToAction("Index", new { id = id });
        }
    }
}
