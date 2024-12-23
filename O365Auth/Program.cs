namespace O365Auth;

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;


class O365Auth
{
    private static void Main()
    {
        Form mainForm = new();
        mainForm.WindowState = FormWindowState.Normal;
        mainForm.Text = "O365 Authentication";
        mainForm.ClientSize = new Size(800, 600);
        mainForm.StartPosition = FormStartPosition.CenterScreen;
        mainForm.AutoScaleMode = AutoScaleMode.None;
        
        WebView2 webView = new WebView2();
        
        // run main form
        Application.Run(mainForm);
    }
}