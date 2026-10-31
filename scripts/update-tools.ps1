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

    $exitCode = $LASTEXITCODE

    if ($exitCode -eq -1978335189) {
        Write-Host "$PackageId is already up to date."
        return
    }

    if ($exitCode -ne 0) {
        throw "Winget upgrade failed for $PackageId with exit code $exitCode."
    }
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
    # 2. RTK agent integrations
    #
    if (Get-Command rtk -ErrorAction SilentlyContinue) {
        Write-Step "Configuring RTK for GitHub Copilot"

        & rtk init -g --copilot

        if ($LASTEXITCODE -ne 0) {
            throw "RTK GitHub Copilot setup failed with exit code $LASTEXITCODE."
        }

        Write-Step "Configuring RTK for Codex"

        & rtk init -g --codex

        if ($LASTEXITCODE -ne 0) {
            throw "RTK Codex setup failed with exit code $LASTEXITCODE."
        }
    }
    else {
        Write-Warning "RTK was not found in PATH. Skipping RTK agent setup."
    }

    #
    # 3. Global Node tooling
    #
    if (-not $SkipSkills) {
        Write-Step "Updating global Playwright CLI"

        & npm install -g "@playwright/cli@latest"

        if ($LASTEXITCODE -ne 0) {
            throw "Playwright CLI update failed."
        }

        Write-Step "Updating global Open Code Review CLI"

        & npm install -g "@alibaba-group/open-code-review@latest"

        if ($LASTEXITCODE -ne 0) {
            throw "Open Code Review CLI update failed."
        }

        Write-Step "Refreshing workspace-local Playwright skill"

        & playwright-cli install --skills=agents

        if ($LASTEXITCODE -ne 0) {
            throw "Playwright skill refresh failed."
        }

        Write-Step "Updating global Archify skill for Codex"

        & npx -y skills add `
            tt-a1i/archify `
            --skill archify `
            --agent codex `
            --global `
            --copy `
            --yes

        if ($LASTEXITCODE -ne 0) {
            throw "Archify skill update failed."
        }

        Write-Step "Updating workspace-local Open Code Review delegate skill"

        & npx -y skills add `
            alibaba/open-code-review `
            --skill open-code-review-delegate `
            --agent codex `
            --copy `
            --yes

        if ($LASTEXITCODE -ne 0) {
            throw "Open Code Review delegate skill update failed."
        }
    }

    #
    # 4. MCP tooling
    #
    if (-not $SkipMcp) {
        Write-Step "Updating FFF MCP"

        $fffInstaller = Join-Path $env:TEMP "install-fff-mcp.ps1"
        $fffRelease = Invoke-RestMethod `
            -Uri "https://api.github.com/repos/dmtrKovalenko/fff/releases/latest" `
            -Headers @{ "User-Agent" = "csharp-test-automation-playwright" }

        if ([string]::IsNullOrWhiteSpace($fffRelease.tag_name) -or $fffRelease.prerelease) {
            throw "Could not resolve a stable FFF MCP release."
        }

        Invoke-WebRequest `
            -Uri "https://raw.githubusercontent.com/dmtrKovalenko/fff/main/install-mcp.ps1" `
            -OutFile $fffInstaller

        Write-Host "Using latest stable FFF release: $($fffRelease.tag_name)"

        & powershell.exe `
            -NoProfile `
            -ExecutionPolicy Bypass `
            -File $fffInstaller `
            -Version $fffRelease.tag_name

        if ($LASTEXITCODE -ne 0) {
            throw "FFF MCP update failed."
        }

        Remove-Item `
            $fffInstaller `
            -Force `
            -ErrorAction SilentlyContinue

        Write-Step "Checking Codebase Memory MCP"

        if (Get-Command codebase-memory-mcp -ErrorAction SilentlyContinue) {
            Write-Step "Updating Codebase Memory MCP"

            $cbmInstaller = Join-Path `
                ([System.IO.Path]::GetTempPath()) `
                "install-codebase-memory-mcp-$([guid]::NewGuid().ToString('N')).ps1"

            try {
                Invoke-WebRequest `
                    -Uri "https://raw.githubusercontent.com/DeusData/codebase-memory-mcp/main/install.ps1" `
                    -OutFile $cbmInstaller `
                    -UseBasicParsing

                & powershell.exe `
                    -NoProfile `
                    -ExecutionPolicy Bypass `
                    -File $cbmInstaller `
                    --skip-config

                if ($LASTEXITCODE -ne 0) {
                    throw "Codebase Memory MCP update failed with exit code $LASTEXITCODE."
                }
            }
            finally {
                Remove-Item `
                    -LiteralPath $cbmInstaller `
                    -Force `
                    -ErrorAction SilentlyContinue
            }
        }
        else {
            Write-Warning "codebase-memory-mcp was not found in PATH."
        }
    }

    #
    # 5. Verification
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

    if (Get-Command ocr -ErrorAction SilentlyContinue) {
        Write-Host -NoNewline "Open Code Review: "
        & ocr --version
    }
    else {
        Write-Warning "Open Code Review CLI not found."
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
        Join-Path $HOME ".agents\skills\archify"

    $OpenCodeReviewSkill =
        Join-Path $WorkspaceRoot ".agents\skills\open-code-review-delegate"

    if (Test-Path $ArchifySkill) {
        Write-Host "Global Archify skill: $ArchifySkill"
    }
    else {
        Write-Warning "Global Archify skill not found."
    }

    if (Test-Path $OpenCodeReviewSkill) {
        Write-Host "Open Code Review delegate skill: $OpenCodeReviewSkill"
    }
    else {
        Write-Warning "Open Code Review delegate skill not found."
    }

    Write-Step "Agent tooling update completed."
}
finally {
    Pop-Location
}
