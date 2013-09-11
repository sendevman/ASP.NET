﻿using System;
using Abp.Startup;

namespace Taskever.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private AbpBootstrapper _bootstrapper;

        protected void Application_Start()
        {
            //TODO: Make a new assembly Abp.Web and create a HttpApplication class to make it there and dont write these code here!
            _bootstrapper = new AbpBootstrapper();
            _bootstrapper.Initialize();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            _bootstrapper.Dispose();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }
    }
}