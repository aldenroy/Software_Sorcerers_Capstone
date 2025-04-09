using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

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
            // options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            if (!OperatingSystem.IsWindows())
            {
                options.AddArgument("--user-data-dir=/tmp/chrome-profile");
            }

            string? driverPath = null;

            if (OperatingSystem.IsMacOS())
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
            
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            
            var baseUrl = _configuration["BaseUrl"];
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
            Thread.Sleep(5000); // Wait for server to start
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start application server: {ex.Message}");
            // Continue - maybe server is already running
        }
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