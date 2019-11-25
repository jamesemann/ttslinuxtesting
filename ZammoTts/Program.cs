using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace ZammoTts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cookies = GetAmazonCookies();

            var json = JsonConvert.SerializeObject(cookies);

            File.WriteAllText("cookies.json", json);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes);
        }

        private static StringContent AsJson(object o)
        {
            return new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
        }

        private static List<Cookie> GetAmazonCookies()
        {
            var driver = CreateWebDriver();
            try
            {
                driver.Navigate().GoToUrl("https://developer.amazon.com/settings/console/registration?return_to=/settings/console/home");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // put in username
                var emailTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ap_email")));

                emailTextBox.SendKeys("conversational.developers.dev@zammo.ai");

                // put in password
                var passwordTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ap_password")));
                passwordTextBox.SendKeys("57W$8gt#Sr&f");

                // click form sign-in button
                var formSignInButton = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("signInSubmit")));
                formSignInButton.Click();

                var developerConsoleLink = wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("Developer Console")));

                // we are logged in, grab the cookies

                Console.WriteLine("success");
                var cookies = driver.Manage().Cookies.AllCookies.ToList();


                return cookies;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TakeScreenshot(driver);
                throw;
            }
            finally
            {
                driver.Close();
            }
        }

        private static IWebDriver CreateWebDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument(@"--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");
            options.AddArgument(@"--start-maximized");
            options.AddArgument("--no-sandbox");
            var driver = new ChromeDriver(options);
            return driver;
        }

        private static void TakeScreenshot(IWebDriver webDriver, string loggingTag = "")
        {
            var filename = $"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}-screenshot.png";
            ((ITakesScreenshot) webDriver).GetScreenshot().SaveAsFile(filename);
        }
    }
}