using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http;

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
            StartApplicationServer();

            var options = new ChromeOptions();
            string uniqueUserDir = Path.Combine(Path.GetTempPath(), "chrome-test-" + Guid.NewGuid().ToString());

            options.AddArguments(
                "--headless",
                "--disable-gpu",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--window-size=1920,1080",
                $"--user-data-dir={uniqueUserDir}"
            );

            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = "chromedriver.log";
            service.EnableVerboseLogging = true;

            _driver = new ChromeDriver(service, options);
            _objectContainer.RegisterInstanceAs<IWebDriver>(_driver);

            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";

            if (!IsAppAvailable(baseUrl, 30))
            {
                throw new Exception($"Application not available at {baseUrl}");
            }

            _driver.Navigate().GoToUrl(baseUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to setup WebDriver: {ex.Message}");
            throw;
        }
    }

    [AfterScenario]
    public void AfterScenario()
    {
        try
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during WebDriver cleanup: {ex.Message}");
        }
        finally
        {
            StopApplicationServer();
        }
    }

    private void StartApplicationServer()
    {
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            return;

        try
        {
            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --project ../../../../YourWebProject/YourWebProject.csproj",
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            _serverProcess.Start();

            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
            if (!IsAppAvailable(baseUrl))
            {
                Console.WriteLine($"Warning: Application not responsive at {baseUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start application server: {ex.Message}");
        }
    }

    private bool IsAppAvailable(string url, int timeoutSeconds = 30)
    {
        Console.WriteLine($"Checking application availability at {url}");

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(1);

        for (int i = 0; i < timeoutSeconds; i++)
        {
            try
            {
                var response = client.GetAsync(url).Result;
                Console.WriteLine($"Response: {(int)response.StatusCode} {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {i + 1}/{timeoutSeconds}: {ex.GetType().Name} - {ex.Message}");
                Thread.Sleep(1000);
            }
        }
        return false;
    }

    private void StopApplicationServer()
    {
        try
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill();
                _serverProcess = null;
            }
        }
        catch
        {
        }
    }
}