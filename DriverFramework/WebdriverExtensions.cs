using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System;
using System.Drawing.Imaging;
using OpenQA.Selenium.Remote;
using System.IO;
using System.Xml;

namespace DriverFramework
{
    public static class WebdriverExtensions
    {

        public class ScreenShotRemoteWebDriver : RemoteWebDriver, ITakesScreenshot
        {

            public ScreenShotRemoteWebDriver(Uri RemoteAdress, DesiredCapabilities capabilities)
                : base(RemoteAdress, capabilities)
            {
                WebdriverExtensions.SiD = GetSessionId();
            }

            public string GetSessionId()
            {
                return base.SessionId.ToString();
            }

            public new Screenshot GetScreenshot()
            {
                // Get the screenshot as base64. 
                Response screenshotResponse = this.Execute(DriverCommand.Screenshot, null);
                string base64 = screenshotResponse.Value.ToString();
                // ... and convert it. 
                return new Screenshot(base64);
            }
        }

        private static string SiD = "";
               
    }


}
