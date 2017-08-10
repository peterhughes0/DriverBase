﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.PhantomJS;
using System.Collections.Specialized;
using System.Configuration;

namespace DriverFramework
{
    public class Driver
    {
        public static IWebDriver driver { get; set; }
        
        public static string rootLocation = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;

        public static void Initialize(Browser browser, NUnit.Framework.TestContext testContext)
        {
            //BrowserStack
            DesiredCapabilities capability = null;


            switch (browser)
            {
                case Browser.Chrome:
                    ChromeOptions option = new ChromeOptions();
                    option.AddArgument("--test-type");
                    option.AddArgument("--disable-extensions");
                    option.AddArgument("start-maximized");
                    driver = new ChromeDriver(rootLocation, option, TimeSpan.FromMinutes(3.0));

                    break;

                case Browser.Headless_Chrome:
                    ChromeOptions headless_option = new ChromeOptions();
                    headless_option.AddArgument("--headlesss");
                    headless_option.AddArgument("--disable-gpu");
                    driver = new ChromeDriver(rootLocation, headless_option, TimeSpan.FromMinutes(3.0));

                    break;
                case Browser.PhantomJS:
                    PhantomJSOptions optionsPJS = new PhantomJSOptions();
                    PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService(rootLocation);
                    service.IgnoreSslErrors = true;
                    service.AddArgument("logfile.txt 2>&1");
                    //optionsPJS.AddAdditionalCapability("phantomjs.page.settings.userAgent", "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
                    DesiredCapabilities phantomCapabilites = DesiredCapabilities.PhantomJS();
                    optionsPJS.AddAdditionalCapability("phantomjs.page.settings.userAgent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:16.0) Gecko/20121026 Firefox/16.0");
                    phantomCapabilites.IsJavaScriptEnabled = true;   
                    service.CookiesFile = "cookies.txt";        
                    service.LocalStoragePath = rootLocation;
                    driver = new PhantomJSDriver(service, optionsPJS, TimeSpan.FromMinutes(3.0));

                    break;
                case Browser.Firefox:
                    driver = new FirefoxDriver();
                    break;
                case Browser.IE:
                    var optionsIE = new InternetExplorerOptions { EnableNativeEvents = false };
                    driver = new InternetExplorerDriver(rootLocation, optionsIE);
                    break;
                case Browser.Edge:
                    var optionsEdge = new EdgeOptions();
                    optionsEdge.PageLoadStrategy = EdgePageLoadStrategy.Eager;
                    driver = new EdgeDriver(rootLocation, optionsEdge);
                    break;
                case Browser.BSIE9:
                    capability = DesiredCapabilities.InternetExplorer();
                    SetCapabilities(capability);
                    capability.SetCapability("version", 9.0);
                    capability.SetCapability(CapabilityType.Platform, new Platform(PlatformType.Vista));
                    driver = new WebdriverExtensions.ScreenShotRemoteWebDriver(new Uri("http://hub.browserstack.com/wd/hub/"), capability);
                    break;
                case Browser.BSIE10:
                    capability = DesiredCapabilities.InternetExplorer();
                    SetCapabilities(capability);
                    capability.SetCapability("version", 10.0);
                    driver = new WebdriverExtensions.ScreenShotRemoteWebDriver(new Uri("http://hub.browserstack.com/wd/hub/"), capability);
                    break;
                case Browser.BSIE11:
                    capability = DesiredCapabilities.InternetExplorer();
                    SetCapabilities(capability);
                    capability.SetCapability("version", 11.0);
                    capability.SetCapability(CapabilityType.Platform, "Win8");
                    driver = new WebdriverExtensions.ScreenShotRemoteWebDriver(new Uri("http://hub.browserstack.com/wd/hub/"), capability);
                    break;

            }
        }

        public static void Refresh()
        {
            driver.Navigate().Refresh();
        }

        public static void resize()
        {
            if (driver.GetType() != typeof(PhantomJSDriver))
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                js.ExecuteScript("window.resizeTo(1000, 1000);");
            }
        }

        public static void ExecuteResultScript(bool result)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("sauce:job-result=" + (result ? "passed" : "failed"));
        }

        public static void ExecuteBuildNumberScript(string buildName)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("sauce:job-build="+buildName+"");
        }

        public static void ExecuteJobVisibility()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("sauce: job-info={'public':'team'}");
        }

        // Accept JS alert if present
        public static void AcceptAlert()
        {
            Wait(2);
            try
            {

                driver.SwitchTo().Alert().Accept();
            }
            catch { throw new Exception("Failed to accept alert message"); }
            driver.SwitchTo().DefaultContent();
        }


        //closes the browser and finishes the test session
        public static void Close()
        {
            var windowHandles = driver.WindowHandles;

            if (windowHandles.Count > 1)
            {
                driver.SwitchTo().Window(windowHandles[0]);
            }
            
            //driver.Dispose();
            driver.Quit();

            try
            {
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("phantomjs"))
                {
                    process.Kill();
                }

                foreach (var process in System.Diagnostics.Process.GetProcessesByName("chromedriver"))
                {
                    process.Kill();
                }
            }

            catch (Exception) { }

        }



        private static void SetCapabilities(DesiredCapabilities capabilities, string profile = null, string environment = null)
        {            
            //NameValueCollection caps = ConfigurationManager.GetSection("capabilities/" + profile) as NameValueCollection;
            //NameValueCollection settings = ConfigurationManager.GetSection("environments/" + environment) as NameValueCollection;

            //DesiredCapabilities capability = new DesiredCapabilities();

            //foreach (string key in caps.AllKeys)
            //{
            //    capability.SetCapability(key, caps[key]);
            //}

            //foreach (string key in settings.AllKeys)
            //{
            //    capability.SetCapability(key, settings[key]);
            //}

            //String username = Environment.GetEnvironmentVariable("james@clicksco.com");
            //if (username == null)
            //{
            //    username = ConfigurationManager.AppSettings.Get("user");
            //}

            //String accesskey = Environment.GetEnvironmentVariable("7Y@mo4Dei2,VYvp%vQQc7RQfFh}Mgc*ffutwKHdKdB22&RmTcH");
            //if (accesskey == null)
            //{
            //    accesskey = ConfigurationManager.AppSettings.Get("key");
            //}

            //capability.SetCapability("browserstack.user", username);
            //capability.SetCapability("browserstack.key", accesskey);

            ////if (capability.GetCapability("browserstack.local") != null && capability.GetCapability("browserstack.local").ToString() == "true")
            ////{
            ////    browserStackLocal = new Local();
            ////    List<KeyValuePair<string, string>> bsLocalArgs = new List<KeyValuePair<string, string>>() {
            ////    new KeyValuePair<string, string>("key", accesskey)};

            ////    browserStackLocal.start(bsLocalArgs);
            ////}


            string[] BrowserStackCredentials = { "browserstack.user", "****", "browserstack.key", "******" };
            capabilities.SetCapability(BrowserStackCredentials[0], BrowserStackCredentials[1]);
            capabilities.SetCapability(BrowserStackCredentials[2], BrowserStackCredentials[3]);

        }

        public int Counter { get; set; }

        public static void Maximise()
        {
            driver.Manage().Window.Maximize();
        }

        public static void MobileResize(int width = 375, int height = 669)
        {
            driver.Manage().Window.Size = new System.Drawing.Size(width, height);        
        }

        public static void DesktopResize(int width = 1366, int height = 821)
        {
            driver.Manage().Window.Size = new System.Drawing.Size(width, height);
        }

        public enum Browser
        {
            TEAMCITY_SAUCE1,
            TEAMCITY_SAUCE2,
            IE11_Windows8_SL,
            IE11_Windows7_SL,
            Chrome,
            Headless_Chrome,
            Firefox,
            IE,
            BSIE9,
            BSIE10,
            BSIE11,
            WinXPSafari,
            Win7Safari,
            MacSafari,
            PhantomJS,
            Edge
        };


        public string foundHotelsUrl { get; set; }

        public static string Title
        {
            get { return driver.Title; }
        }

        public static string URL
        {
            get { return driver.Url; }
        }


        public static string PageSource
        {
            get { return driver.PageSource; }
        }

        /// <summary>
        /// Wait for a specified time
        /// </summary>
        /// <param name="seconds"></param>
        public static void Wait(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public static void GoTo(string url)
        {
            //Maximise();
            Wait(2);
             driver.Navigate().GoToUrl(url);

        }

        public static void OpenNewTab()
        {
            IWebElement body = driver.FindElement(By.TagName("body"));
            body.SendKeys(Keys.Control + 't');
            Wait(1);
        }

        /// <summary>
        /// Navigates to the requested page, the pagesource will be checked for the presence of the check
        /// </summary>        
        public static void GoTo(string url, string check)
        {
            //Maximise();


            driver.Navigate().GoToUrl(url); // Live Site


            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            try
            {
                wait.Until((d) => { return d.PageSource.Contains(check); });
            }

            catch
            {
                throw new Exception(string.Format("Failed to access requested page {0}, with specified text: {1}",url,check));
            }

        }



        #region selectElements

        public static void SelectElementByLinkText(string linkText)
        {
            IWebElement link = driver.FindElement(By.LinkText(linkText));
            link.Click();
        }

        public static void SelectElementByClassName(string className)
        {
            IWebElement link = driver.FindElement(By.ClassName(className));
            link.Click();
        }

        public static void SelectElementByCssSelector(string css)
        {
            IWebElement link = driver.FindElement(By.CssSelector(css));
            link.Click();
        }

        public static void SelectElementByXpath(string path)
        {
            IWebElement link = driver.FindElement(By.XPath(path));
            link.Click();
        }


        public static void SelectElementByID(string ID)
        {
            IWebElement link = driver.FindElement(By.Id(ID));
            link.Click();
        }

        public static void SelectBack()
        {
            driver.Navigate().Back();
        }


        public static void SelectElementsInMultipleCollections(string Class, int item)
        {
            var collection = driver.FindElements(By.ClassName(Class));
            IWebElement link = collection[item];
            link.Click();
        }

        public static void SelectLinkInMultipleCollections(string Class, int item)
        {
            var collection = driver.FindElements(By.ClassName(Class));
            IWebElement link = collection[item].FindElement(By.TagName("a"));
            link.Click();
        }

        #endregion

        #region containsElement

        public static bool IDElementContainsText(string elementName, string text, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.FindElement(By.Id(elementName)).Text.Contains(text); });

                return true;

            }
            catch
            {
                return false;
            }
        }

        // Function  to check for the existence of an element by ID
        public static bool ContainsElementByID(string id)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.FindElements(By.Id(id)).Count > 0; });
            }
            catch
            {
                return false;
            }

            return true;
        }

        // Overloaded for custom wait time
        public static bool ContainsElementByID(string id, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.FindElements(By.Id(id)).Count > 0; });
            }
            catch
            {
                return false;
            }

            return true;
        }

        // Function to check for the existence of element by its CSS classname
        public static bool ContainsElementByCSSClass(string className)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.FindElements(By.ClassName(className)).Count > 0; });
            }

            catch
            {
                return false;
            }

            return true;
        }

        // Overloaded for custom wait time
        public static bool ContainsElementByCSSClass(string className, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.FindElements(By.ClassName(className)).Count > 0; });
            }

            catch
            {
                return false;
            }

            return true;
        }

        // Function to check the existence of an element by its name
        public static bool ContainsElementByName(string name)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.FindElements(By.Name(name)).Count > 0; });
            }

            catch
            {
                return false;
            }

            return true;
        }

        // Overloaded for custom wait time
        public static bool ContainsElementByName(string name, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.FindElements(By.Name(name)).Count > 0; });
            }

            catch
            {
                return false;
            }

            return true;
        }


        // Function to check for a specific attribute value in a specified element
        public static bool ElementAttributeIs(string element, string attribute, string attributeIs)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.FindElement(By.ClassName(element)); });

                IWebElement theElement = driver.FindElement(By.ClassName(element));
                var attributeResult = theElement.GetAttribute(attribute);
                if (attributeIs != attributeResult)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        // Function to switch to a particular frame ie: an Iframe
        public static void SwitchToFrame(string content, int number)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.FindElements(By.TagName(content)).Count > number; });
            var elements = driver.FindElements(By.TagName(content));
            var wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            // wait2.Until((d) => { return d.SwitchTo().Frame()})
            IWebElement containerFrame = driver.FindElements(By.TagName(content))[number];

            driver.SwitchTo().Frame(containerFrame);
        }

        // Function to force a switch to a newly created window
        public static void SwitchToNewWindow()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.WindowHandles.Count > 0; });
            List<string> handles = new List<string>(driver.WindowHandles);

            try
            {
                driver.SwitchTo().Window(handles[1]);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Unable to switch to new window");
            }
            Wait(2);
            Maximise();
            Wait(2);
        }

        public static void SwitchToNewWindow(int windowIndex)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.WindowHandles.Count > 0; });
            List<string> handles = new List<string>(driver.WindowHandles);

            try
            {
                driver.SwitchTo().Window(handles[windowIndex]);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Unable to switch to new window");
            }
            Wait(2);
            Maximise();
            Wait(2);
        }


        public static void CloseNewWindow()
        {
            Driver.Close();
            List<string> handles = new List<string>(driver.WindowHandles);
            driver.SwitchTo().Window(handles[0]);
        }

        // Function to check for the specified Link Text
        public static bool ContainsLinkText(string criteria)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.FindElements(By.LinkText(criteria)).Count > 0; });
            }

            catch
            {
                return false;
            }

            return true;
        }

        // Overloaded for custom wait time
        public static bool ContainsLinkText(string criteria, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.FindElements(By.LinkText(criteria)).Count > 0; });
            }

            catch
            {
                return false;
            }

            return true;
        }


        // General function to check that the current Pagesource contains the given text
        public static bool ContainsText(string text)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.PageSource.Contains(text); });
            }
            catch
            {
                return false;
            }

            return driver.PageSource.Contains(text);
        }

        // General function to check that the current Pagesource contains the given text
        // Overloaded with option to add custom time to wait
        public static bool ContainsText(string text, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.PageSource.Contains(text); });
            }
            catch
            {
                return false;
            }

            return driver.PageSource.Contains(text);
        }

        public static bool ContainsCssSelector(string cssSelector)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until((d) => { return d.FindElements(By.CssSelector(cssSelector)).Count > 0; });
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool ContainsCssSelector(string cssSelector, int time)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                wait.Until((d) => { return d.FindElements(By.CssSelector(cssSelector)).Count > 0; });
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        public static void WriteBadUrl()
        {
            System.IO.StreamWriter swr = System.IO.File.AppendText("BAD.txt");
            using (swr)
            {
                swr.WriteLine(driver.Url.ToString());
            }
        }

        public static void WriteGoodUrl()
        {
            System.IO.StreamWriter swr = System.IO.File.AppendText("GOOD.txt");
            using (swr)
            {
                swr.WriteLine(driver.Url.ToString());
            }

        }



        public static void ScrollToTopOfPageUsingJS()
        {
            if (driver.GetType() != typeof(PhantomJSDriver))
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                js.ExecuteScript("window.scrollTo(0,0)", "");
            }
        }

        public static void ScrollToBottomOfPageUsingJS()
        {
            if (driver.GetType() != typeof(PhantomJSDriver))
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                js.ExecuteScript("window.scrollTo(0,2000)", "");
            }
        }





    }
}