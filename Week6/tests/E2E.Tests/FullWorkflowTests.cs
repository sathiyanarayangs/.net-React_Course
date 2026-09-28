using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace E2E.Tests
{
    public class FullWorkflowTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://localhost:5173"; 
        
        public FullWorkflowTests()
        {
            var options = new ChromeOptions();
            // options.AddArgument("--headless=new"); // Commented out so you can see it!
            // options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        private void WaitUntilElementVisible(By by, int timeoutInSeconds = 10)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(d => {
                try {
                    return d.FindElement(by).Displayed;
                }
                catch (NoSuchElementException) {
                    return false;
                }
            });
        }

        [Fact]
        public void Workflow_Register_Login_Logout()
        {
            // 1. Navigate to register
            _driver.Navigate().GoToUrl($"{_baseUrl}/register");
            WaitUntilElementVisible(By.CssSelector("input[type='text']")); // Username field

            var username = "TestTeacher_" + Guid.NewGuid().ToString().Substring(0, 5);

            _driver.FindElement(By.CssSelector("input[type='text']")).SendKeys(username);
            _driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("Password123!");
            _driver.FindElement(By.CssSelector("select")).SendKeys("Teacher"); 
            
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Wait for navigation
            System.Threading.Thread.Sleep(2000);
            
            // 2. Login
            WaitUntilElementVisible(By.CssSelector("input[type='text']"));
            _driver.FindElement(By.CssSelector("input[type='text']")).SendKeys(username);
            _driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("Password123!");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Wait for navigation to /students
            System.Threading.Thread.Sleep(2000);
            WaitUntilElementVisible(By.CssSelector(".table-container"));
            Assert.Contains("Student", _driver.PageSource);

            // 3. Logout
            _driver.FindElement(By.CssSelector("button.danger")).Click(); 
            System.Threading.Thread.Sleep(1000);
            WaitUntilElementVisible(By.CssSelector("form"));
            Assert.Contains("Welcome Back", _driver.PageSource);
        }

        [Fact]
        public void Workflow_StudentWriteAttempt_ShouldBeRejected()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/register");
            WaitUntilElementVisible(By.CssSelector("input[type='text']"));

            var username = "TestStudent_" + Guid.NewGuid().ToString().Substring(0, 5);

            _driver.FindElement(By.CssSelector("input[type='text']")).SendKeys(username);
            _driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("Password123!");
            _driver.FindElement(By.CssSelector("select")).SendKeys("Student");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            System.Threading.Thread.Sleep(2000);
            
            WaitUntilElementVisible(By.CssSelector("input[type='text']"));
            _driver.FindElement(By.CssSelector("input[type='text']")).SendKeys(username);
            _driver.FindElement(By.CssSelector("input[type='password']")).SendKeys("Password123!");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            System.Threading.Thread.Sleep(2000);
            WaitUntilElementVisible(By.CssSelector(".table-container"));

            var deleteButtons = _driver.FindElements(By.CssSelector("button.delete-button")); 
            Assert.Empty(deleteButtons);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
