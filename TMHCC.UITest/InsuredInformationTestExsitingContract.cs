using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;

namespace TMHCC.UITest
{

    /// <summary>
    /// Login --> Verify --> New Quote --> General Liablity-->
    /// With Contract NUmber--> License State-->Policy NUmber
    /// </summary>

    [TestClass]
    public class InsuredInformationTestExsitingContract
    {
        private IWebDriver driver;
        private Config config;
        private WebDriverWait wait;

        [TestInitialize]
        public void Setup()
        {
            config = ConfigLoader.LoadConfig("../../../config.json");

            new DriverManager().SetUpDriver(new ChromeConfig());
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            Login(); // Call login at the start of every test
        }

        private void Login()
        {
            driver.Navigate().GoToUrl(config.Url);

            wait.Until(d => d.FindElement(By.Name("Email"))).SendKeys(config.Email);
            driver.FindElement(By.Name("Password")).SendKeys(config.Password);
            driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            wait.Until(d => d.FindElement(By.Name("answer"))).SendKeys(config.Answer);

            try
            {
                var verifyBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementToBeClickable(By.CssSelector("input[type='submit']")));
                verifyBtn.Click();
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Verify button not found or not clickable.");
            }

            wait.Until(d => d.Url.Contains("dashboard"));
            Assert.IsTrue(driver.Url.Contains("dashboard"), "Login failed or Dashboard not reached.");
        }


        [TestMethod]
        public void CreateNewClaimTest()
        {
            try
            {
                var newClaimButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementToBeClickable(By.Id("create-new-claim-btn")));
                newClaimButton.Click();

                // JavaScript fallback for 'General Liability' link
                IWebElement generalLiabilityLink = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementExists(By.XPath("//a[normalize-space(text())='General Liability']")));

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", generalLiabilityLink);

                wait.Until(d => d.Url.Contains("gl-insured"));
                InsuredYesInformationTest();


                Assert.IsTrue(driver.Url.Contains("gl-insured"), "Failed to navigate to General Liability page.");
            }
            catch (WebDriverTimeoutException ex)
            {

                Assert.Fail("Error clicking General Liability: " + ex.Message);
            }
        }
        public void SearchAndAssertInsuredInfo()
        {
            try
            {
                // 1. Click the dropdown to open it
                var dropdownContainer = wait.Until(ExpectedConditions
                    .ElementToBeClickable(By.CssSelector(".choices[data-type='select-one']")));
                dropdownContainer.Click();

                // 2. Wait for "CALIFORNIA" to appear in the dropdown options
                var californiaOption = wait.Until(ExpectedConditions
                    .ElementToBeClickable(By.XPath("//div[contains(@class,'choices__item--choice') and span[text()='CALIFORNIA']]")));

                // Scroll into view (optional but helpful for some UIs)
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", californiaOption);

                // 3. Click "CALIFORNIA"
                californiaOption.Click();

                // 4. Wait for the selected item to be updated (excluding placeholder)
                var selectedItem = wait.Until(ExpectedConditions
                    .ElementIsVisible(By.CssSelector(".choices__list--single .choices__item:not(.choices__placeholder)")));

                // 5. Read and assert the selected state
                string selectedState = selectedItem.Text.Trim();
                Console.WriteLine("Selected state: " + selectedState);
                Assert.AreEqual("CALIFORNIA", selectedState.ToUpper(), "Expected selected state to be CALIFORNIA.");


                // 4. Fill in the license number
                var licenseInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementIsVisible(By.Id("e3oimpa-licNum")));
                licenseInput.Clear();
                licenseInput.SendKeys("1082094");

                // 5. Wait for and click the Search button
                var searchButton = wait.Until(driver =>
                {
                    var btn = driver.FindElement(By.CssSelector("button[name='data[search]']"));
                    return btn.Enabled ? btn : null;
                });
                searchButton.Click();

                // 6. Wait for business type field to show expected value
                var resultInput = wait.Until(driver =>
                {
                    try
                    {
                        var input = driver.FindElement(By.Id("ea17wjm-exactPolName"));
                        string value = input.GetAttribute("value");
                        Console.WriteLine("Found business type: " + value);
                        return value == "PAINT & DECOR FINISHES" ? input : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });

                Assert.IsNotNull(resultInput, "Expected 'PAINT & DECOR FINISHES' was not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Page HTML:\n" + driver.PageSource);
                Assert.Fail("Timeout or assertion failed during business type check: " + ex.Message);
            }
        }

        public void InsuredYesInformationTest()
        {
            try
            {
                // Wait for radio input by name and value
                var accountOption = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementToBeClickable(By.CssSelector("input[type='radio'][name='data[licQuestion][e5yott]'][value='46532']")));
                accountOption.Click();
                SearchAndAssertInsuredInfo();
            }
            catch (WebDriverTimeoutException ex)
            {
                
                Assert.Fail("Timeout waiting for Insured Info section: " + ex.Message);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            driver.Quit();
        }
    }

}
