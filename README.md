
# o365psauth
Powershell script to initiate Office 365 (Microsoft 365) authentication

## Description
Script goal is to let user proceed with O365 authentication, including MFA, and retrieve the OIDC code that can be later
used to obtain an OIDC token.


The powershell script is based on WebView2 embedded browser implementation.
Usage:

- Authenticate with default url, clientId and redirect_uri:
  `.\o365psauth.ps1`

- Use the provided url to authenticate on a specific application:
  `.\o365psauth.ps1 -url https://login.microsoftonline.com/common/oauth2/authorize?client_id=XXXXXXXXXX&response_type=code&redirect_uri=XXXXXXXXXX&response_mode=query&resource=https%3A%2F%2Foutlook.office365.com`

## Prerequisites
Just retrieve the script and WebView2 runtime from https://github.com/mguessan/o365psauth or use the script with a 
locally installed runtime, see https://developer.microsoft.com/en-us/microsoft-edge/webview2

## Build
Nothing to build, this is just a script

## Reference

* Inspired by:

https://gist.github.com/COFFEETALES

* Microsoft documentation:

https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core
    
    
    
