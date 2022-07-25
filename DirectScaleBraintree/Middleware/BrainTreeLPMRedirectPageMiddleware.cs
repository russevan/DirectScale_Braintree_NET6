using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Middleware
{
    internal class BraintreeLPMRedirectPageMiddleware
    {

        public BraintreeLPMRedirectPageMiddleware(RequestDelegate next)
        {

        }
        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: Implement RedirectPage.
            //var result = new
            //{
            //    Version = Assembly.GetEntryAssembly().GetName().Version
            //};

            //string resultBody = JsonConvert.SerializeObject(result);

            //context.Response.StatusCode = 200;
            //context.Response.ContentType = "application/json";
            //await context.Response.WriteAsync(resultBody, Encoding.UTF8);
        }
    }
}
