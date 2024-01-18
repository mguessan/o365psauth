# O365 logon script to create OIDC authentication code.
# Inspired by https://gist.github.com/COFFEETALES

Param ([String]$URL = 'https://login.microsoftonline.com/common/oauth2/authorize?client_id=d3590ed6-52b3-4102-aeff-aad2292ab01c&response_type=code&redirect_uri=urn%3Aietf%3Awg%3Aoauth%3A2.0%3Aoob&response_mode=query&resource=https%3A%2F%2Foutlook.office365.com', [String]$Mode = 'Default')

# relaunch self with right options and working directory
If ('Default' -ieq $Mode)
{
    [String]$PowerShellPath = Get-Process -Id $PID | Select-Object -ExpandProperty Path

    Start-Process `
	  -Wait `
	  -FilePath $PowerShellPath `
	  -NoNewWindow `
	  -ArgumentList (
    '-NoLogo',
    '-NoProfile',
    '-NonInteractive',
    '-File', ('"', ($MyInvocation.MyCommand.Definition), '"' -join ''),
    $URL,
    'WebView'
    )
}

If ('WebView' -ine $Mode)
{
    Return
}

Try
{

    # uncomment for debug logs
    #$DebugPreference = 'Continue'
    $ErrorActionPreference = 'Stop'

    # force SetProcessDPIAware
    $SetProcessDPIAware =
    Add-Type -PassThru -Name User32SetProcessDPIAware `
			-MemberDefinition (
    @'
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	public static extern bool SetProcessDPIAware();
'@
    )
    [Void]$SetProcessDPIAware::SetProcessDPIAware()

    # Ensure windows forms and drawing assemblies are loaded
    [ScriptBlock]$LoadAssembly = {
        Param ([String]$PartialName)
        If (-not([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -ieq $PartialName }))
        {
            [Void][Reflection.Assembly]::LoadWithPartialName($PartialName)
        }
    }

    & $LoadAssembly 'System.Windows.Forms'
    & $LoadAssembly 'System.Drawing'


    # Ensure Webview2 assemblies are loaded
    If (-not([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -ieq 'Microsoft.Web.WebView2.WinForms.WebView2' }))
    {
        Write-Debug "Load embedded dlls"
        [Void][Reflection.Assembly]::LoadFrom('Microsoft.Web.WebView2.Core.dll')
        [Void][Reflection.Assembly]::LoadFrom('Microsoft.Web.WebView2.WinForms.dll')
    }


    # Create form
    $MainForm = [Windows.Forms.Form]::New()

    $MainForm.WindowState = [Windows.Forms.FormWindowState]::Normal
    $MainForm.Text = 'O365 Authentication'
    $MainForm.ClientSize = [Drawing.Size]::New(800, 600)
    $MainForm.StartPosition = [Windows.Forms.FormStartPosition]::CenterScreen
    $MainForm.AutoScalemode = [Windows.Forms.AutoScaleMode]::None

    $MainForm.SuspendLayout()

    # Ceate WebView and attach to main form
    $WebView = [Microsoft.Web.WebView2.WinForms.WebView2]::New()

    ([ComponentModel.ISupportInitialize]$WebView).BeginInit()
    $WebView.Name = 'O365Auth'
    $WebView.Size = [Drawing.Size]::New(800, 600)
    $WebView.Anchor = [Windows.Forms.AnchorStyles]::Top -bor [Windows.Forms.AnchorStyles]::Right -bor [Windows.Forms.AnchorStyles]::Bottom -bor [Windows.Forms.AnchorStyles]::Left

    ([ComponentModel.ISupportInitialize]$WebView).EndInit()

    $MainForm.Controls.Add($WebView)

    $MainForm.ResumeLayout($FALSE)
    $MainForm.PerformLayout()

    # Prepare WebView2 Environment with default cache location
    $WebView2EnvironmentOptions = [Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions]::New()
    # Supposed to allow workplace authentication, does not seem to work
    $WebView2EnvironmentOptions.AllowSingleSignOnUsingOSPrimaryAccount = $TRUE
    $WebView2Env = [Microsoft.Web.WebView2.Core.CoreWebView2Environment]::CreateAsync(
            [String]::Empty,[IO.Path]::Combine([String[]]([IO.Path]::GetTempPath(), 'O365WebView')),
            $WebView2EnvironmentOptions
    )
    # Navigate to url when ready
    $WebView2Env.GetAwaiter().OnCompleted(
            [Action]{
                $WebView.EnsureCoreWebView2Async($WebView2Env.Result)
                $WebView.Source = [Uri]::New($URL)
            }
    )

    # Adjust Webview settings
    $WebView.Add_CoreWebView2InitializationCompleted(
            [EventHandler[Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs]]{
                [Microsoft.Web.WebView2.Core.CoreWebView2Settings]$Settings = $WebView.CoreWebView2.Settings
                $Settings.AreDefaultContextMenusEnabled = $FALSE
                $Settings.AreDefaultScriptDialogsEnabled = $FALSE
                $Settings.AreDevToolsEnabled = $FALSE
                $Settings.AreHostObjectsAllowed = $FALSE
                $Settings.IsBuiltInErrorPageEnabled = $FALSE
                $Settings.IsScriptEnabled = $TRUE
                $Settings.IsStatusBarEnabled = $FALSE
                $Settings.IsWebMessageEnabled = $TRUE
                $Settings.IsZoomControlEnabled = $FALSE
            }
    )

    # Detect authentication complete (code available) print url and exit
    $WebView.add_NavigationStarting(
            [EventHandler[Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs]]{
                param ($eventWebView, $e)
                if ($e.Uri -match "code=")
                {
                    Write-Debug "Authentication succeeded received code"
                    Write-Host $e.Uri
                    $MainForm.Close()
                }
                Write-Debug("Navigate to " + $e.Uri)
            }
    )

    # run main form
    [Windows.Forms.Application]::Run($MainForm)
}
Finally
{
    If ($NULL -cne $WebView)
    {
        $WebView.Dispose()
        $WebView = $NULL
    }
    If ($NULL -cne $MainForm)
    {
        $MainForm.Dispose()
        $MainForm = $NULL
    }
}
