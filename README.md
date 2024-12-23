# O365psauth
Powershell script to initiate Office 365 (Microsoft 365) authentication.

Another C# based version is available under O365Auth directory

## Download

- Powershell script:
  [o365psauth.ps1](https://github.com/mguessan/o365psauth/archive/refs/tags/1.1.zip)

This package contains actual Powershell script and Webview2 dependencies.

- Dotnet C# version built by Github actions:
  https://github.com/mguessan/o365psauth/actions/runs/12470764907/artifacts/2356808671

Standalone O365Auth.exe

## Description
Script goal is to let user proceed with O365 authentication, including MFA, and retrieve the OIDC code that can be later
used to obtain an OIDC token.


The powershell script is based on WebView2 embedded browser implementation.
Usage:

- Authenticate with default url, clientId and redirect_uri:
  `.\o365psauth.ps1`

- Authenticate using native OS authentication (workplace join):
  `.\o365psauth.ps1 -SSO`

- Use the provided url to authenticate on a specific application:
  `.\o365psauth.ps1 -url https://login.microsoftonline.com/common/oauth2/authorize?client_id=XXXXXXXXXX&response_type=code&redirect_uri=XXXXXXXXXX&response_mode=query&resource=https%3A%2F%2Foutlook.office365.com`

- In addition, you can retrieve the actual OIDC token:
  `.\o365psauth.ps1 -Token`
  `.\o365psauth.ps1 -SSO -Token`

Standalone O365Auth provides the same options, just replace .\o365psauth.ps1 with O365Auth.

## Prerequisites
Just retrieve the script and WebView2 runtime from https://github.com/mguessan/o365psauth or use the script with a
locally installed runtime, see https://developer.microsoft.com/en-us/microsoft-edge/webview2

## Build
Powershell version: Nothing to build, this is just a script

CSharp version:
`dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true`

## Reference

* Inspired by:

https://gist.github.com/COFFEETALES

* Microsoft documentation:

https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core
