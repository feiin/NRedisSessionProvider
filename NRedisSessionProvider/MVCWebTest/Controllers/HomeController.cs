using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace MVCWebTest.Controllers
{
	//[Serializable]
	public class User
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public int Sex { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var mvcName = typeof(Controller).Assembly.GetName();
			var isMono = Type.GetType("Mono.Runtime") != null;

			ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
			ViewData["Runtime"] = isMono ? "Mono" : ".NET";
			ViewData["Info"] = "";

			if (Session["user"] == null)
			{
				var u = new User();
				u.UserName = "test";
				u.Password = "pwd";
				u.Sex = 1;
				u.CreatedAt = DateTime.Now;
				Session["user"] = u;

			}
			else {
				var u = (User)Session["user"];
				ViewData["Info"] =  u.UserName + " CreatedAt" + u.CreatedAt;
			}
			Session["x"] = "hello";


			return View();
		}


	}
}

