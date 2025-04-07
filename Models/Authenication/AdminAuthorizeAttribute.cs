using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;

namespace KDKMenShop.Models.Authentication
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public int MaChucNang { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            var sessionUser = context.HttpContext.Session.GetString("Users");


            if (string.IsNullOrEmpty(sessionUser))
            {
                HandleUnauthorized(context);
                return;
            }

            try
            {
                // Deserialize session JSON to TaiKhoan object
                var taiKhoan = JsonConvert.DeserializeObject<TaiKhoan>(sessionUser);

                if (taiKhoan.LoaiTk?.Trim().ToLower() == "admin")
                {
                    return; // Admin has access to everything
                }

                var data = context.HttpContext.RequestServices.GetService(typeof(ThoiTrangNamKDKContext)) as ThoiTrangNamKDKContext;


                if (data != null)
                {
                    var count = data.PhanQuyenNhanViens.Count(m => m.IdNhanVien == taiKhoan.Id && m.MaChucNang == MaChucNang);

                    if (count != 0)
                    {
                        return; // User has permission
                    }
                    else
                    {
                        ErrorUnauthorized(context);
                        return;
                    }
                }
                HandleUnauthorized(context);

                


            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing TaiKhoan from JSON: {ex.Message}");
                HandleInternalServerError(context);
            }
        }

        private void HandleUnauthorized(ActionExecutingContext context)
        {
            string returnUrl = context.HttpContext.Request.Path;
            context.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Home" },
                { "action", "DangNhap" },
                { "area", "Admin" },
                { "returnUrl", returnUrl }
            });
        }
        private void ErrorUnauthorized(ActionExecutingContext context)
        {
            string returnUrl = context.HttpContext.Request.Path;
            context.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "RoleError" },
                { "action", "KhongCoQuyen" },
                { "area", "Admin" },
                { "returnUrl", returnUrl }
            });
        }

        private void HandleInternalServerError(ActionExecutingContext context)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
