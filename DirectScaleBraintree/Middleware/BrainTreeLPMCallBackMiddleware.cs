﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DirectScaleBraintree.Middleware
{
    internal class BraintreeLPMCallBackMiddleware
    {
        public BraintreeLPMCallBackMiddleware(RequestDelegate next)
        {

        }
        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: Implement Callback Logic.
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
