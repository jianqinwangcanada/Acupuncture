using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Acupuncture.CommonFunction.AttributeFuction
{
    public class AjaxAccessOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            //
            return routeContext.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest";
        }
    }
}
