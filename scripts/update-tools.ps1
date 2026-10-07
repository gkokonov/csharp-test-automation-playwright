[CmdletBinding()]
param(
    [switch]$SkipWinget,
    [switch]$SkipSkills,
    [switch]$SkipMcp
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$WorkspaceRoot = Split-Path -Parent $PSScriptRoot

function Write-Step {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    Write-Host ""
    Write-Host "==> $Message"
}

function Invoke-WingetUpgrade {
    param(
        [Parameter(Mandatory)]
        [string]$PackageId
    )

    Write-Step "Updating $PackageId"

    & winget upgrade `
        --id $PackageId `
        --exact `
        --scope machine `
        --silent `
        --accept-source-agreements `
        --accept-package-agreements `
        --disable-interactivity

    # Winget may return non-zero when there is simply no applicable update,
    # therefore do not fail the whole script based solely on LASTEXITCODE.
}

Push-Location $WorkspaceRoot

try {
    #
    # 1. Machine-installed Winget tools
    #
    if (-not $SkipWinget) {
        Invoke-WingetUpgrade "rtk-ai.rtk"
        Invoke-WingetUpgrade "BurntSushi.ripgrep.MSVC"
    }

    #
    # 2. Global Node tooling
    #
    if (-not $SkipSkills) {
        Write-Step "Updating global Playwright CLI"

        & npm install -g "@playwright/cli@latest"

        if ($LASTEXITCODE -ne 0) {
            throw "Playwright CLI update failed."
        }

        Write-Step "Refreshing workspace-local Playwright skill"

        & playwright-cli install --skills=agents

        if ($LASTEXITCODE -ne 0) {
            throw "Playwright skill refresh failed."
        }

        Write-Step "Updating workspace-local Archify skill"

        & npx -y skills add `
            tt-a1i/archify `
            --skill archify `
            --agent codex `
            --copy `
            --yes

        if ($LASTEXITCODE -ne 0) {
            throw "Archify skill update failed."
        }
    }

    #
    # 3. MCP tooling
    #
    if (-not $SkipMcp) {
        Write-Step "Updating FFF MCP"

        $fffInstaller = Join-Path $env:TEMP "install-fff-mcp.ps1"

        Invoke-WebRequest `
            -Uri "https://raw.githubusercontent.com/dmtrKovalenko/fff/main/install-mcp.ps1" `
            -OutFile $fffInstaller

        & powershell.exe `
            -NoProfile `
            -ExecutionPolicy Bypass `
            -File $fffInstaller

        if ($LASTEXITCODE -ne 0) {
            throw "FFF MCP update failed."
        }

        Remove-Item `
            $fffInstaller `
            -Force `
            -ErrorAction SilentlyContinue

        Write-Step "Checking Codebase Memory MCP"

        if (Get-Command codebase-memory-mcp -ErrorAction SilentlyContinue) {
            & codebase-memory-mcp update
        }
        else {
            Write-Warning "codebase-memory-mcp was not found in PATH."
        }
    }

    #
    # 4. Verification
    #
    Write-Step "Verifying installations"

    if (Get-Command rtk -ErrorAction SilentlyContinue) {
        Write-Host -NoNewline "RTK: "
        & rtk --version
    }
    else {
        Write-Warning "RTK not found."
    }

    if (Get-Command rg -ErrorAction SilentlyContinue) {
        Write-Host -NoNewline "ripgrep: "
        & rg --version | Select-Object -First 1
    }
    else {
        Write-Warning "ripgrep not found."
    }

    if (Get-Command playwright-cli -ErrorAction SilentlyContinue) {
        Write-Host -NoNewline "Playwright CLI: "
        & playwright-cli --version
    }
    else {
        Write-Warning "Playwright CLI not found."
    }

    $PlaywrightSkill =
        Join-Path $WorkspaceRoot ".agents\skills\playwright-cli"

    if (Test-Path $PlaywrightSkill) {
        Write-Host "Playwright skill: $PlaywrightSkill"
    }
    else {
        Write-Warning "Workspace Playwright skill not found."
    }

    $ArchifySkill =
        Join-Path $WorkspaceRoot ".agents\skills\archify"

    if (Test-Path $ArchifySkill) {
        Write-Host "Archify skill: $ArchifySkill"
    }
    else {
        Write-Warning "Workspace Archify skill not found."
    }

    Write-Step "Agent tooling update completed."
}
finally {
    Pop-Location
}