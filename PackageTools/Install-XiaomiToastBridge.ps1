# Run this script from an elevated PowerShell window in the extracted package folder.
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$certPath = Join-Path $here 'LocalTest.cer'
if (!(Test-Path $certPath)) { throw "Certificate file not found: $certPath" }
$packages = @(Get-ChildItem -Path $here -Recurse -File | Where-Object { $_.Extension -in '.appx', '.msix' })
if ($packages.Count -eq 0) { throw 'No .appx/.msix package found in this folder.' }

$cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($certPath)
$store = [System.Security.Cryptography.X509Certificates.X509Store]::new('TrustedPeople', 'CurrentUser')
$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
try {
    $store.Add($cert)
} finally {
    $store.Close()
}

foreach ($package in $packages) {
    Write-Host "Installing $($package.FullName) ..."
    Add-AppxPackage -Path $package.FullName
}
Write-Host 'Installation completed. Launch XiaomiToastBridge from the Start menu.' -ForegroundColor Green
