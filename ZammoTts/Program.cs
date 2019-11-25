using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bogus.DataSets;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace ZammoTts
{
    class Program
    {
        static void Main(string[] args)
        {
                //var question = "hello world";

                //var sessionCookie = string.Empty;
                //var cookies = JsonConvert.DeserializeObject<List<MyCookie>>(File.ReadAllText("amazon.json"));
                //foreach (MyCookie cookie in cookies)
                //{
                //    sessionCookie = $"{cookie.Name}={cookie.Value};{sessionCookie}";
                //}


                //var request = new HttpRequestMessage(HttpMethod.Post, "https://developer.amazon.com/alexa/console/ask/test/getTTS");
                //request.Headers.Add("Cookie", sessionCookie);
                //request.Headers.Add("Accept", "application/json");

                //var _client = new HttpClient();
                //request.Content = AsJson(new {audioFormat = "MP3", contentType = "SSML", text = Base64Encode($"<speak>{question}</speak>"), locale = "en-US"});
                //var response = _client.SendAsync(request).Result;
            // set
            var cookies = GetAmazonCookies();

            //var json = JsonConvert.SerializeObject(cookies);



        }

        
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes);
        }
        private static StringContent AsJson(object o) => new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");

        private static List<Cookie> GetAmazonCookies()
        {
            var driver = CreateWebDriver();
            try
            {                
                driver.Navigate().GoToUrl("https://www.google.com");

                TakeScreenshot(driver);

                return new List<Cookie>();

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // click sign in button
                //var signInButton = wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("Sign In")));
                //signInButton.Click();

                // put in username
                var emailTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ap_email")));

                driver.ExecuteJavaScript(@"// overwrite the `languages` property to use a custom getter
const setProperty = function() {
    Object.defineProperty(window.navigator, 'languages', {
      value: ['en-US', 'en'],
      writable: true
    });

    // Overwrite the `plugins` property to use a custom getter.
    Object.defineProperty(window.navigator, 'plugins', {
      value: [1,2,3,4,5],
      writable: true
    });

    // Pass the Webdriver test
    Object.defineProperty(window.navigator, 'webdriver', {
      value: false,
      writable: true
    });
};
setProperty();");
                emailTextBox.SendKeys("james.mann@bcs.org");

                // put in password
                var passwordTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ap_password")));
                passwordTextBox.SendKeys("pass");

                // click form sign-in button
                var formSignInButton = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("signInSubmit")));
                formSignInButton.Click();

                try
                {
                    var developerConsoleLink = wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("Developer Console")));
                }
                catch (Exception ex)
                {
                    File.WriteAllText("captcha.html", driver.PageSource);
                    // likely stuck in captcha
                    passwordTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ap_password")));

                    driver.ExecuteJavaScript("document.getElementById('ap_password').setAttribute('type', 'text');");
                    driver.ExecuteJavaScript(@"// overwrite the `languages` property to use a custom getter
const setProperty = function() {
    Object.defineProperty(window.navigator, 'languages', {
      value: ['en-US', 'en'],
      writable: true
    });

    // Overwrite the `plugins` property to use a custom getter.
    Object.defineProperty(window.navigator, 'plugins', {
      value: [1,2,3,4,5],
      writable: true
    });

    // Pass the Webdriver test
    Object.defineProperty(window.navigator, 'webdriver', {
      value: false,
      writable: true
    });
};
setProperty();");
                    passwordTextBox.SendKeys("pass");

                    TakeScreenshot(driver);
                    var captchaImage = driver.FindElement(By.Id("auth-captcha-image"));
                    var imageUrl = captchaImage.GetAttribute("src");

                    Console.WriteLine(imageUrl);

                    Console.WriteLine("Enter captcha");
                    var captcha = Console.ReadLine();

                    var captchaText = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("auth-captcha-guess")));
                    captchaText.SendKeys(captcha);
                    TakeScreenshot(driver);
                    formSignInButton = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("signInSubmit")));
                    formSignInButton.Click();
                    wait.Until(ExpectedConditions.ElementIsVisible(By.PartialLinkText("Dashboard")));
                }
                //developerConsoleLink.Click();
             //   driver.Navigate().GoToUrl("https://developer.amazon.com/settings/console/home");

                wait.Until(ExpectedConditions.ElementIsVisible(By.PartialLinkText("Dashboard")));

                // we are logged in, grab the cookies
                var cookies = driver.Manage().Cookies.AllCookies.ToList();

                driver.Close();

                return cookies;
            }
            catch (Exception ex)
            {
                TakeScreenshot(driver);
                throw;
            }
        }

        private static IWebDriver CreateWebDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument(@"--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");
            //options.AddArgument("--headless");
            options.AddArgument(@"--start-maximized");
            options.AddArgument("--no-sandbox");
            //options.AddAdditionalCapability("browserless.token", "8ecac86c-c576-4a7c-81a7-89a3a57df57a", true);

            //var driver = new RemoteWebDriver(new Uri("https://chrome.browserless.io/webdriver"), options);

           var driver = new ChromeDriver(options);
           //driver.Manage().Window
            return driver;
        }

        private static void TakeScreenshot(IWebDriver webDriver, string loggingTag = "")
        {
            string filename = $"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}-screenshot.png";
            ((ITakesScreenshot)webDriver).GetScreenshot().SaveAsFile(filename);
        }
    }

    
    class MyCookie {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("expiry")]
        public long Expiry { get; set; }

        [JsonProperty("httpOnly")]
        public bool IsHttpOnly { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("secure")]
        public bool Secure { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public DateTime GetExpiryDateTime()
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Expiry).ToLocalTime();

            return dtDateTime;
        }

        // public Cookie (String name, String value, String domain, String path, Nullable<DateTime> expiry);
        public Cookie GetCookie()
        {
            return new Cookie(Name, Value, Domain, Path, GetExpiryDateTime());
        }
    }
}
