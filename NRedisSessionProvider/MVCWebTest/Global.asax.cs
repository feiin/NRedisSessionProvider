using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCWebTest
{
	public class Global : HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
 			//NRedisProvider.NRedisSessionProvider.
			NRedisProvider.NRedisSessionProvider.InitRedisConfig("localhost:6379", false);
		}
	}
}
