using System.Threading.Tasks;
using System.Web.Mvc;

namespace sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // START HERE


            return View();
        }

        public async Task DownloadAsync()
        {
            
        }
    }
}