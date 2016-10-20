using System.Web;
using System.Web.Mvc;

namespace Pentamic.Integration.Ibms
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
