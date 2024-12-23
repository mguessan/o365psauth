using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Web;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace O365Auth;

class O365Auth
{
    [STAThread]
    private static void Main()
    {
        // enable single sign on with windows user session
        bool isSso = false;
        // retrieve token from code
        bool isGetToken = false;
        
        // Build default url
        
        // Outlook clientId/redirectUri
        string clientId = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
        string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        string resource = "https://outlook.office365.com";
        
        // main microsoft login url
        string baseUrl = "https://login.microsoftonline.com/common/oauth2/authorize";
        
        NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("client_id", clientId);
        queryString.Add("response_type", "code");
        queryString.Add("redirect_uri", redirectUri);
        queryString.Add("response_mode", "query");
        queryString.Add("resource", resource);

        string url = baseUrl+"?"+queryString;

        String[] commandLineArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            String arg = commandLineArgs[i];
            if ("-SSO".Equals(arg, StringComparison.InvariantCultureIgnoreCase))
            {
                isSso = true;
            }
            else if ("-Token".Equals(arg, StringComparison.InvariantCultureIgnoreCase))
            {
                isGetToken = true;
            }
            else if (arg.StartsWith("https://"))
            {
                // override url
                url = arg;
            }
        }

        int clientWidth = 800;
        int clientHeight = 600;

        Form mainForm = new();
        mainForm.WindowState = FormWindowState.Normal;
        mainForm.Text = "O365 Authentication";
        mainForm.ClientSize = new Size(clientWidth, clientHeight);
        mainForm.StartPosition = FormStartPosition.CenterScreen;
        mainForm.AutoScaleMode = AutoScaleMode.None;


        // suspend layout during build
        mainForm.SuspendLayout();

        // Create WebView2 embedded browser
        WebView2 webView = new WebView2();

        ((ISupportInitialize)webView).BeginInit();
        webView.Name = "O365Auth";
        webView.Size = new Size(clientWidth, clientHeight);
        webView.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
        ((ISupportInitialize)webView).EndInit();

        mainForm.Controls.Add(webView);

        // resume and force layout
        mainForm.ResumeLayout(false);
        mainForm.PerformLayout();

        // Prepare WebView2 Environment with default cache location
        CoreWebView2EnvironmentOptions webView2EnvironmentOptions = new()
        {
            // use native OS authentication, requires latest webview2 version
            AllowSingleSignOnUsingOSPrimaryAccount = isSso
        };

        Task<CoreWebView2Environment> webView2Env = CoreWebView2Environment.CreateAsync(
            "", Path.Combine(Path.GetTempPath(), "O365WebView"), webView2EnvironmentOptions
        );

        // Navigate to url when ready
        webView2Env.GetAwaiter().OnCompleted(
            () =>
            {
                webView.EnsureCoreWebView2Async(webView2Env.Result);
                webView.Source = new Uri(url);
            }
        );

        // Adjust Webview settings
        webView.CoreWebView2InitializationCompleted += WebViewCoreWebView2InitializationCompleted;
        // attach navigation starting handler
        webView.NavigationStarting += WebViewCoreWebView2NavigationStarting;

        // run main form
        Debug.WriteLine("Run main form with url: "+url);
        Application.Run(mainForm);
        return;

        async void WebViewCoreWebView2NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Debug.WriteLine("Navigate to " + e.Uri);
            
            if (e.Uri.Contains("code="))
            {
                Debug.WriteLine("Authentication succeeded received code");

                try
                {
                    if (isGetToken)
                    {
                        Debug.WriteLine("Retrieving token from code");
                        string tokenUri = url.Substring(0, url.IndexOf("/authorize", StringComparison.Ordinal)) +
                                          "/token";
                        using HttpClient client = new();
                        client.BaseAddress = new Uri(tokenUri);

                        // get code from uri
                        String codeQueryString = new Uri(e.Uri).Query;
                        String code = HttpUtility.ParseQueryString(codeQueryString).Get("code") ?? "";

                        HttpContent httpContent = new FormUrlEncodedContent(
                            new Dictionary<string, string>
                            {
                                { "grant_type", "authorization_code" },
                                { "client_id", clientId },
                                { "redirect_uri", redirectUri },
                                { "code", code }
                            });

                        try
                        {
                            HttpResponseMessage response = await client.PostAsync(tokenUri, httpContent);
                            response.EnsureSuccessStatusCode();
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(jsonResponse);
                        }
                        catch (WebException webException)
                        {
                            Console.WriteLine($"Exception trying to retrieve token ${webException.Response}");
                        } catch (Exception exception) {
                            Console.WriteLine($"Exception trying to retrieve token ${exception.Message}");
                        }
                    }
                    else
                    {
                        // print url with code
                        Console.WriteLine(e.Uri);
                    }
                }
                finally
                {
                    mainForm.Close();
                }
            }

            if (e.Uri.Contains("error="))
            {
                Debug.WriteLine("Authentication failed");
                Console.WriteLine("Error: "+e.Uri);
                mainForm.Close();
            }
        }

        void WebViewCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            CoreWebView2Settings settings = webView.CoreWebView2.Settings;
            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDefaultScriptDialogsEnabled = false;
            settings.AreDevToolsEnabled = false;
            settings.AreHostObjectsAllowed = false;
            settings.IsBuiltInErrorPageEnabled = false;
            settings.IsScriptEnabled = true;
            settings.IsStatusBarEnabled = true;
            settings.IsWebMessageEnabled = false;
            settings.IsZoomControlEnabled = true;
        }
    }
}