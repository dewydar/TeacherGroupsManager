param(
    [string]$SiteName = "TeacherGroupsManager",
    [string]$HostName = "www.TGM.com",
    [string]$PublishPath = "$PSScriptRoot\..\artifacts\publish",
    [string]$SitePath = "C:\inetpub\wwwroot\TGM",
    [string]$AppPoolName = "TeacherGroupsManager",
    [string]$CertificateThumbprint = "",
    [switch]$CreateSelfSignedCertificate,
    [switch]$UpdateHostsFile
)

$ErrorActionPreference = "Stop"

function Assert-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "IIS deployment must run as Administrator."
    }
}

function Get-OrCreate-Certificate {
    param(
        [string]$HostName,
        [string]$Thumbprint,
        [bool]$CreateSelfSigned
    )

    if (-not [string]::IsNullOrWhiteSpace($Thumbprint)) {
        $certificate = Get-ChildItem Cert:\LocalMachine\My |
            Where-Object { $_.Thumbprint -eq $Thumbprint } |
            Select-Object -First 1

        if ($null -eq $certificate) {
            throw "Certificate with thumbprint '$Thumbprint' was not found in Cert:\LocalMachine\My."
        }

        return $certificate
    }

    $existing = Get-ChildItem Cert:\LocalMachine\My |
        Where-Object { $_.DnsNameList.Unicode -contains $HostName -and $_.NotAfter -gt (Get-Date) } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if ($null -ne $existing) {
        return $existing
    }

    if (-not $CreateSelfSigned) {
        throw "No certificate found for '$HostName'. Provide -CertificateThumbprint or use -CreateSelfSignedCertificate."
    }

    return New-SelfSignedCertificate `
        -DnsName $HostName `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -FriendlyName "$HostName local IIS certificate" `
        -NotAfter (Get-Date).AddYears(5)
}

function Set-HostsEntry {
    param([string]$HostName)

    $hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
    $hostsContent = Get-Content -LiteralPath $hostsPath -ErrorAction SilentlyContinue
    if ($hostsContent -notmatch "(?i)\s$([regex]::Escape($HostName))(\s|$)") {
        Add-Content -LiteralPath $hostsPath -Value "127.0.0.1 $HostName"
    }
}

Assert-Admin

if (-not (Test-Path -LiteralPath $PublishPath)) {
    throw "Publish path '$PublishPath' does not exist."
}

Import-Module WebAdministration

if ($UpdateHostsFile) {
    Set-HostsEntry -HostName $HostName
}

$certificate = Get-OrCreate-Certificate `
    -HostName $HostName `
    -Thumbprint $CertificateThumbprint `
    -CreateSelfSigned:$CreateSelfSignedCertificate

if (-not (Test-Path -LiteralPath $SitePath)) {
    New-Item -ItemType Directory -Path $SitePath | Out-Null
}

if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-WebAppPool -Name $AppPoolName | Out-Null
}

Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value "ApplicationPoolIdentity"

if (Test-Path "IIS:\Sites\$SiteName") {
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
}
if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped") {
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
}

Get-ChildItem -LiteralPath $SitePath -Force | Remove-Item -Recurse -Force
Copy-Item -Path (Join-Path $PublishPath "*") -Destination $SitePath -Recurse -Force

if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    New-Website -Name $SiteName -PhysicalPath $SitePath -ApplicationPool $AppPoolName -Port 80 -HostHeader $HostName | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $SitePath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
}

$httpBinding = Get-WebBinding -Name $SiteName -Protocol "http" |
    Where-Object { $_.bindingInformation -eq "*:80:$HostName" }
if ($null -eq $httpBinding) {
    New-WebBinding -Name $SiteName -Protocol "http" -Port 80 -HostHeader $HostName | Out-Null
}

$httpsBinding = Get-WebBinding -Name $SiteName -Protocol "https" |
    Where-Object { $_.bindingInformation -eq "*:443:$HostName" }
if ($null -eq $httpsBinding) {
    New-WebBinding -Name $SiteName -Protocol "https" -Port 443 -HostHeader $HostName -SslFlags 1 | Out-Null
}

$bindingPath = "IIS:\SslBindings\0.0.0.0!443!$HostName"
if (Test-Path $bindingPath) {
    Remove-Item $bindingPath -Force
}

$certificate | New-Item $bindingPath -SslFlags 1 | Out-Null

Start-WebAppPool -Name $AppPoolName
Start-Website -Name $SiteName

Write-Host "Published '$SiteName' to https://$HostName"
