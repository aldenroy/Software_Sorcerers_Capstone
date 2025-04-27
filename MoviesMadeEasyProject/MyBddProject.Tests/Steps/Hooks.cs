using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private Process? _serverProcess;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            try
            {
                // Only start the server locally
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != "true")
                {
                    StartApplicationServer();
                }
                else
                {
                    Console.WriteLine("Running in GitHub Actions - using workflow-started app server");
                }

                var options = new ChromeOptions();

                // Basic options for headless testing
                options.AddArguments("--headless", "--disable-gpu");

                // Add GitHub Actions specific options
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
                {
                    options.AddArguments(
                        "--no-sandbox",
                        "--disable-dev-shm-usage",
                        "--window-size=1920,1080"
                    );
                }

                // Create ChromeDriver with appropriate settings
                _driver = new ChromeDriver(options);

                // Set longer timeouts for GitHub Actions
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
                {
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
                    _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                }

                _objectContainer.RegisterInstanceAs(_driver);

                // Wait for app to be available
                var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                bool isAvailable = WaitForAppAvailability(baseUrl,
                    Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ? 60 : 30);

                if (!isAvailable)
                {
                    throw new Exception($"Application not available at {baseUrl}");
                }

                _driver.Navigate().GoToUrl(baseUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SETUP ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_driver != null)
            {
                try
                {
                    _driver.Quit();
                    _driver.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error quitting driver: {ex.Message}");
                }
            }

            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != "true")
            {
                StopApplicationServer();
            }
        }

        private bool WaitForAppAvailability(string url, int timeoutSeconds)
        {
            Console.WriteLine($"Checking application availability at {url}");
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            DateTime endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            bool isSuccess = false;
            int attempts = 0;

            while (DateTime.Now < endTime && !isSuccess)
            {
                attempts++;
                try
                {
                    var response = client.GetAsync(url).Result;
                    Console.WriteLine($"Attempt {attempts}: Response {(int)response.StatusCode} {response.StatusCode}");
                    isSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempts}: {ex.GetType().Name}");
                    Thread.Sleep(1000);
                }

                if (!isSuccess && DateTime.Now < endTime)
                {
                    Thread.Sleep(1000);
                }
            }

            return isSuccess;
        }

        private void StartApplicationServer()
        {
            try
            {
                _serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run --project ../../../MoviesMadeEasyProject/MoviesMadeEasy/MoviesMadeEasy.csproj",
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };
                _serverProcess.Start();
                Console.WriteLine("Started application server locally");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start application server: {ex.Message}");
            }
        }

        private void StopApplicationServer()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    _serverProcess = null;
                    Console.WriteLine("Stopped local application server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping server: {ex.Message}");
            }
        }
    }
}