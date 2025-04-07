using Microsoft.AspNetCore.Mvc;

namespace KDKMenShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleErrorController : Controller
    {
        // GET: Admin/ErrorRole
        public ActionResult KhongCoQuyen()
        {
            return View();
        }
        public ActionResult LoiRangBuoc()
        {
            return View();
        }
        public ActionResult SuccessSendPassword()
        {
            return View();
        }
    }
}
