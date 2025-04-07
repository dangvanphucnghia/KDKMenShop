    namespace KDKMenShop.Others
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Logging;
    using System.Data.Common;
    public class LogQueryLocationFilter : IActionFilter
    {
        private readonly ILogger<LogQueryLocationFilter> _logger;

        public LogQueryLocationFilter(ILogger<LogQueryLocationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller.GetType().Name;
            var action = context.ActionDescriptor.DisplayName;
            Console.WriteLine("@@---------------------------------------------------------------@@");
            Console.WriteLine($"@@------Query ở Controller: {controller}, Action: {action}------@@");
            Console.WriteLine("@@---------------------------------------------------------------@@");

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Optional: Log after the action has executed
        }
    }
}
