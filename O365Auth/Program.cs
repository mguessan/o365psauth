using System.ComponentModel;
using Microsoft.Web.WebView2.Core;

namespace O365Auth;

using Microsoft.Web.WebView2.WinForms;


class O365Auth
{
    [STAThread]
    private static void Main()
    {

        // enable single sign on with windows user session
        bool isSSO = false;
        
        // main microsoft login url
        string url = "https://login.microsoftonline.com/";
        
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
            AllowSingleSignOnUsingOSPrimaryAccount = isSSO
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

        // Adjust Webview settings
        webView.CoreWebView2InitializationCompleted += WebViewCoreWebView2InitializationCompleted;
        
        // run main form
        Application.Run(mainForm);
    }
}