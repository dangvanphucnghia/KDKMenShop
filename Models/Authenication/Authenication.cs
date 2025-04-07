//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc.Infrastructure;

//namespace KDKMenShop.Models.Authenication
//{
//    public class Authenication : ActionFilterAttribute
//    {
//        public override void OnActionExecuting(ActionExecutingContext context)
//        {
//            if (context.HttpContext.Session.GetString("TaiKhoan") == null)
//            {
//                context.Result = new RedirectToRouteResult(
//                    new RouteValueDictionary
//                    {
//                        {"Controller","Account"},
//                        {"Action", "DangNhap" }
//                    });
//            }
//        }
//    }
//}
