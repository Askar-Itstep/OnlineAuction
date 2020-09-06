using OnlineAuction.Entities;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;

namespace OnlineAuction.Controllers
{
    public class HomeController : Controller
    {
        private Model1 db = new Model1();
        public async Task<ActionResult> Index()
        {
            //var auctions = db.Auctions.Include(a => a.Actor).Include(a => a.Product).Include(a => a.Winner);
            //return View(await auctions.ToListAsync());
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}