using CsharpTestAutomation.Framework.UI;
using CsharpTestAutomation.Tests.Configurations.Models;
using CsharpTestAutomation.Tests.UI.Pages.MSEntraID;
using Microsoft.Playwright;
using NLog;

namespace CsharpTestAutomation.Tests.Authentication;

public static class UiAuthenticationBootstrapper
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    // MSAL stores the access token under a dynamic sessionStorage key, so scan values for an
    // entry whose credentialType is "AccessToken".
    private const string TokenScanScript = """
        () => {
            for (let i = 0; i < sessionStorage.length; i++) {
                const key = sessionStorage.key(i);
                const raw = sessionStorage.getItem(key);
                try {
                    const obj = JSON.parse(raw);
                    if (obj
                            && typeof obj.credentialType === 'string'
                            && obj.credentialType.toLowerCase() === 'accesstoken'
                            && typeof obj.secret === 'string'
                            && obj.secret.length > 10) {
                        return obj.secret;
                    }
                } catch { }
            }
            return null;
        }
        """;

    private const string SessionStorageDumpScript = "() => JSON.stringify(sessionStorage)";

    public static async Task BootstrapAllAsync(UiConfigurationDTO ui)
    {
        if (string.IsNullOrWhiteSpace(ui.ApplicationUrl))
        {
            throw new InvalidOperationException("UI.ApplicationUrl is not configured.");
        }

        UiBootstrapAuthConfigurationDTO auth = ui.Authentication;
        if (auth.Users.Count == 0)
        {
            s_log.Warn("No bootstrap users configured under UI.Authentication.Users; skipping UI auth bootstrap.");
            return;
        }

        Directory.CreateDirectory(auth.StorageStateDirectory);

        await PlaywrightBrowserFactory.InitializeAsync();

        try
        {
            foreach (BootstrapUserDTO user in auth.Users)
            {
                BootstrapAuthState state = await LoginAndCaptureAsync(ui.ApplicationUrl!, auth, user);
                BootstrapSession.Set(user.Key, state);
                s_log.Info($"Bootstrapped authenticated session for user '{user.Key}'.");
            }
        }
        finally
        {
            await PlaywrightBrowserFactory.DisposeContextAsync();
            await PlaywrightBrowserFactory.DisposeBrowserAsync();
        }
    }

    private static async Task<BootstrapAuthState> LoginAndCaptureAsync(string appUrl, UiBootstrapAuthConfigurationDTO auth, BootstrapUserDTO user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
        {
            throw new InvalidOperationException($"Username/Password not configured for bootstrap user '{user.Key}'.");
        }

        // HTTP credentials enable ADFS/WIA silent auth and drive the federated form.
        IBrowserContext context = await PlaywrightBrowserFactory
            .CreateContextWithGivenHttpCredentialsAsync(user.Username!, user.Password!);
        IPage page = await PlaywrightBrowserFactory.CreatePageAsync();

        await page.GotoAsync(appUrl, new PageGotoOptions {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = auth.LoginTimeoutInMs
        });

        var loginPage = new MSLoginPage(page);
        await loginPage.WaitUntilLoadedAsync();
        await loginPage.EnterAccountAsync(user.Username!);
        await loginPage.ClickNextAsync();

        Uri appUri = new(appUrl);
        await page.WaitForURLAsync(
            url => new Uri(url).Host.Equals(appUri.Host, StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = auth.LoginTimeoutInMs });

        // Give MSAL time to finish writing the access token to sessionStorage.
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Wait until MSAL writes the access token. A fixed delay fails when login is slow and
        // wastes time when it is fast. LoginTimeoutInMs bounds the wait. Playwright 1.63
        // WaitForFunctionAsync is non-generic and returns a handle to the truthy value.
        IJSHandle tokenHandle = await page.WaitForFunctionAsync(
            TokenScanScript,
            null,
            new PageWaitForFunctionOptions { Timeout = auth.LoginTimeoutInMs });
        var token = await tokenHandle.JsonValueAsync<string>();
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException($"Failed to extract MSAL access token for user '{user.Key}' after login.");
        }

        var sessionStorageJson = await page.EvaluateAsync<string>(SessionStorageDumpScript);

        var storageStatePath = Path.Combine(auth.StorageStateDirectory, $"{SanitizeKey(user.Key)}.json");
        var storageStateJson = await context.StorageStateAsync(new BrowserContextStorageStateOptions {
            Path = storageStatePath
        });

        s_log.Debug($"Saved storage state for user '{user.Key}' to {storageStatePath}.");

        await page.CloseAsync();
        // PlaywrightBrowserFactory owns the context. The next user replaces it, and
        // DisposeContextAsync closes the last one. Do not close it here.

        return new BootstrapAuthState(token, storageStateJson, sessionStorageJson);
    }

    private static string SanitizeKey(string key)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", key.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
