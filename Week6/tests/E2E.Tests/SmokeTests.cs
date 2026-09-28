using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace E2E.Tests
{
    public class SmokeTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://localhost:5173"; // Default Vite port

        public SmokeTests()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new"); // Run headless for CI
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Fact]
        public void SmokeTest_OpensLoginPage()
        {
            // Act
            _driver.Navigate().GoToUrl($"{_baseUrl}/login");

            // Wait for the login form to load
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => {
                try {
                    return d.FindElement(By.CssSelector("form")).Displayed;
                }
                catch (NoSuchElementException) {
                    return false;
                }
            });

            // Assert
            var title = _driver.Title;
            Assert.Contains("student-portal", title); // Vite default title

            var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
            Assert.NotNull(loginButton);
            Assert.Equal("Login", loginButton.Text);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
