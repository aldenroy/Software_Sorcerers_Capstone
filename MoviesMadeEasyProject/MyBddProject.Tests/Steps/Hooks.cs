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
            // Start application server if needed
            StartApplicationServer();

            // Configure ChromeDriver
            var options = new ChromeOptions();
            options.AddArguments(
                "--headless", // Run in headless mode (no visible window)
                "--disable-gpu",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--window-size=1920,1080"
            );
            if (!OperatingSystem.IsWindows())
            {
                options.AddArgument("--user-data-dir=/tmp/chrome-profile");
            }

            string? driverPath = null;

            if (OperatingSystem.IsLinux())
            {
                // On Linux (including GitHub Actions), just create the driver
                // The chromedriver should be in PATH from your GitHub workflow
                _driver = new ChromeDriver(options);
            }
            else if (OperatingSystem.IsMacOS())
            {
                driverPath = _configuration["DriverPaths:Mac"];
                if (string.IsNullOrWhiteSpace(driverPath))
                {
                    throw new Exception("ChromeDriver path for Mac is not configured in appsettings.json.");
                }

                _driver = new ChromeDriver(driverPath, options);
            }
            else if (OperatingSystem.IsWindows())
            {
                _driver = new ChromeDriver(options);
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS.");
            }

            _objectContainer.RegisterInstanceAs<IWebDriver>(_driver);

            var baseUrl = _configuration["BaseUrl"];

            // Check if application is available before proceeding
            if (!IsAppAvailable(baseUrl))
            {
                throw new Exception($"Application not available at {baseUrl}. Please ensure it's running.");
            }

            _driver.Navigate().GoToUrl(baseUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to setup WebDriver: {ex.Message}");
            Console.WriteLine($"Make sure your application is running at {_configuration["BaseUrl"]}");
            throw;
        }
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _driver?.Quit();
        StopApplicationServer();
    }

    private void StartApplicationServer()
    {
        // Skip starting server in GitHub Actions - it's started by workflow
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

            // Wait for app to be ready
            var baseUrl = _configuration["BaseUrl"];
            if (!IsAppAvailable(baseUrl))
            {
                Console.WriteLine($"Warning: Application not responsive at {baseUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start application server: {ex.Message}");
            // Continue - maybe server is already running
        }
    }

    private bool IsAppAvailable(string url, int timeoutSeconds = 30)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(1);

        for (int i = 0; i < timeoutSeconds; i++)
        {
            try
            {
                var response = client.GetAsync(url).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Waiting for application to start... ({i + 1}/{timeoutSeconds})");
            }
        }
        return false;
    }

    private void StopApplicationServer()
    {
        try
        {
            _serverProcess?.Kill();
        }
        catch
        {
            // Ignore
        }
    }
}