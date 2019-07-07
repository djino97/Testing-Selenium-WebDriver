using System;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TaskTest
{
    public class TestFramework
    {
        public static string igWorkDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); // рабочий каталог, относительно исполняемого файла (в нашем случае относительно DLL)
        public static IWebDriver driver;
        public WebDriverWait wait;

        /*
          Automatic properties classs containing input parametrs of the filter.
          Example:
            Country: "Russian Federation"
            Languages: {"English", "Russian"}, - search in two languages

            Country:"Romania"
            Languages: {"English",} - search one languge
                   
        */ 
        class InputArguments
        {   
            public String Country { get; set; }
            public List<String> Languages { get; set; }
        }


        /*
          Automatic properties classs for output the values of one or several test data.
          Example:
            CountVacancy: {"The number of vacancies Test 1 - 12", "The number of vacancies Test 2 - 32 ..."}
        */
        class OutputTestResult
        {
            public String CountVacancy { get; set; }
        }


        /* 
          Called before run all tests.
          Configures and launches the browser Chrome.
        */
        [OneTimeSetUp] 
        public void OneTimeSetUp()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--ignore-certificate-errors");
            options.AddArguments("--ignore-ssl-errors");
            driver = new ChromeDriver(igWorkDir, options, TimeSpan.FromSeconds(200));
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        }


        /*
          Adding parametrs to filter.
          Example, adding multiple parameters:
            ...
            arguments.Add(new InputArguments { Country = "Romania", Languages = new List<String> { "English" } });
            arguments.Add(new InputArguments { Country = "Russian Federation", Languages = new List<String> { "English", "Russian"} });
            ...
        */
        static List<InputArguments> EnterArguments()
        {
            List<InputArguments> arguments = new List<InputArguments>();
            arguments.Add(new InputArguments { Country = "Romania", Languages = new List<String> { "English" } });
            return arguments;
        }


        //Called before each the tests. Open the browser Chrome.
        [SetUp] 
        public void SetUp()
        {
            driver.Navigate().GoToUrl("https://careers.veeam.com/");
        }


        // Called after all the tests. Close the browser Chrome.
        [OneTimeTearDown] 
        public void OneTimeTearDown()
        {
            driver.Quit();
        }


        // Enter values in the filter by country.
        void EnterValueCountry(String country)
        {
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible( By.CssSelector(".selecter.closed")));
            element.Click();
            driver.FindElement(By.XPath(String.Format("//span[text()='{0}']", country))).Click();

        }


        // Enter values in the filter by language.
        void EnterValueLanguage(List <String> languages)
        {
            IWebElement inputLanguages =  wait.Until(ExpectedConditions.ElementIsVisible(By.Id("language")));
            inputLanguages.Click();

            IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector(".controls-checkbox"));

            foreach (var language in languages)
            {
                int i = 0;
                foreach (var element in elements)
                {
                
                    if (element.Text == language)
                        driver.FindElements(By.CssSelector(".controls-checkbox"))[i].Click();
                    i ++;
                }
            }
        }

        /* 
          Pressing the button "Apply" and pressing the button "Show all jobs".
          Then comes the waiting for the opening of all vacancies.
        */
        void FindContent()
        {
            driver.FindElement(By.CssSelector(".selecter-fieldset-submit")).Click();
            IWebElement applyLanguages = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".content-loader-button.load-more-button")));
            applyLanguages.Click();
            Thread.Sleep(5000);
        }


        /*
          The main test to testing the operation of the filter for jobs.
          Processed input values and output in the list view.
        */
        [Test]
        public void TestFilteringOfVacancies()
        {
            driver.FindElement(By.CssSelector(".cookie-messaging__button")).Click();
            List<OutputTestResult> outputVacancy = new List<OutputTestResult>();
            List<InputArguments> arguments = EnterArguments();

            int numberTest = 0;

            foreach (InputArguments parameter in arguments)
            {
                EnterValueCountry(parameter.Country);
                EnterValueLanguage(parameter.Languages);
                FindContent();
                numberTest ++;
                IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector(".vacancies-blocks-item-description"));
                outputVacancy.Add(new OutputTestResult { CountVacancy = String.Format("The number of vacancies (Test {0}) - ", numberTest) + elements.Count });
                driver.FindElement(By.Id("clear-filters-button")).Click();
            }

            foreach (var output in outputVacancy)
                Console.WriteLine(output.CountVacancy);
        }

    }

}
