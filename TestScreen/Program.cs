using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using TestScreen;

class Program
{
    private const string screnshotsFolder = "testScreens";

    static void Main(string[] args)
    {
        

        string configFilePath = "config.json";


        Config config = File.Exists(configFilePath)
            ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath))
            : new Config();

        foreach (var arg in args)
        {
            var splitArg = arg.Split('=');
            if (splitArg.Length == 2)
            {
                string key = splitArg[0].ToLower();
                string value = splitArg[1];

                switch (key)
                {
                    case "-b":
                        config.Browser = value;
                        break;
                    case "-w":
                        config.WindowWidth = int.Parse(value);
                        break;
                    case "-h":
                        config.WindowHeight = int.Parse(value);
                        break;
                    case "-u":
                        config.Url = value;
                        break;
                }
            }
        }


        if (string.IsNullOrEmpty(config.Browser) || string.IsNullOrEmpty(config.Url))
        {
            Console.WriteLine("Url is empty ");
            return;
        }

        IWebDriver driver = null;
        try
        {
            if (config.Browser.ToLower() == "chrome")
            {
                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true; 
                service.SuppressInitialDiagnosticInformation = true;
                driver = new ChromeDriver(service);
            }
            else if (config.Browser.ToLower() == "firefox")
            {
                var service = FirefoxDriverService.CreateDefaultService();
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true; 
                driver = new FirefoxDriver(service);
            }
            else if (config.Browser.ToLower() == "edge")
            {
                var service = EdgeDriverService.CreateDefaultService();
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true; 
                driver = new EdgeDriver(service);
            }
            else
            {
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.WriteLine($"Unsupported browser: {config.Browser}");
                return;
            }
           
            driver.Manage().Window.Size = new System.Drawing.Size(config.WindowWidth, config.WindowHeight);

            driver.Navigate().GoToUrl(config.Url);

            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            var uri = new Uri(config.Url);
            string screenshotPath = $"{uri.Host}_{config.WindowWidth}x{config.WindowHeight}_{config.Browser}.png";
            screenshot.SaveAsFile(screenshotPath);
            Console.WriteLine($"Screen saved: {screenshotPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            driver?.Quit();
        }
    }
}