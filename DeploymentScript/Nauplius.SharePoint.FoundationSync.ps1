Param([boolean]$Confirm)

$solutionName = "Nauplius.SharePoint.FoundationSync.wsp"
$solutionUri = "https://github.com/Nauplius/FoundationSync/wiki"
$restartSPTimer = $true

Write-Host -ForegroundColor White "$solutionName Installation Script"
Write-Host -ForegroundColor White "----------------------------------"
Write-Host -ForegroundColor White "Information about this solution can be found at $solutionUri."
Write-Host -ForegroundColor DarkYellow "License: GPLv2"
Write-Host -ForegroundColor DarkYellow "Release Date: 12/11/2014"
Write-Host -ForegroundColor DarkYellow "Version: 2.5"
Write-Host -ForegroundColor DarkYellow "Platform: SharePoint 2013"
Write-Host

if ($Confirm -eq $false)
	{
	Write-Host -ForegroundColor White "Press Y to continue with the installation or any other key to exit."
	Write-Host

	$char = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
	if ($char.Character -ne "Y")
	{
		exit
	}
}

Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction Stop

function WaitForSPSolutionJobToComplete([string]$solutionName)
{
    $solution = Get-SPSolution -Identity $solutionName -EA 0

    if ($solution)
    {
	    if ($solution.JobExists)
	    {
		    Write-Host -ForegroundColor DarkGray -NoNewLine "`tWaiting for timer job to complete for solution $solutionName."
	    }
		
	    # Check if there is a timer job still associated with this solution and wait until it has finished
	    while ($solution.JobExists)
	    {
		    $jobStatus = $solution.JobStatus
			
		    # If the timer job succeeded then proceed
		    if ($jobStatus -eq [Microsoft.SharePoint.Administration.SPRunningJobStatus]::Succeeded)
		    {
			    Write-Host -ForegroundColor DarkGray "Solution $solutionName timer job suceeded"
			    return $true | Out-Null
		    }
			
		    # If the timer job failed or was aborted then fail
		    if ($jobStatus -eq [Microsoft.SharePoint.Administration.SPRunningJobStatus]::Aborted -or
			    $jobStatus -eq [Microsoft.SharePoint.Administration.SPRunningJobStatus]::Failed)
		    {
			    Write-Host -ForegroundColor DarkGray "Solution $solutionName has timer job status $jobStatus."
			    return $false | Out-Null
		    }
			
		    # Otherwise wait for the timer job to finish
		    Write-Host -ForegroundColor DarkGray -NoNewLine "."
		    Sleep 1
	    }
		
	    # Write a new line to the end of the '.....'
	    Write-Host
    }

return $true | Out-Null
}

function CheckForExistingSolution([string]$solutionName)
{
$solution = Get-SPSolution -Identity $solutionName -EA 0

    if ($solution -ne $null)
    {
        if ($solution.Deployed -eq $true)
        {
            Write-Host -ForegroundColor Green "Uninstalling $solutionName from SharePoint..."
            Write-Host
            Uninstall-SPSolution -Identity $solution -Confirm:$false
            WaitForSPSolutionJobToComplete($solutionName)
        }

        Write-Host -ForegroundColor Green "Removing $solutionName from SharePoint..."
        Write-Host
        Remove-SPSolution $solution -Confirm:$false

        if ($restartSPTimer = $true)
        {
            foreach($server in (Get-SPServer | where {$_.Role -ne "Invalid"}).Name)
            {
                Write-Host -ForegroundColor Yellow "`tStopping SPTimerV4 on $server"
                $service = Get-Service -Name SPTimerV4 -ComputerName $server
                $service | Set-Service -Status Stopped
                while($service.Status -ne "Stopped")
                {
                    Sleep 1
                    $service.Refresh()
                }

                Write-Host -ForegroundColor Yellow "`tStarting SPTimerV4 on $server"
                $service | Set-Service -Status Running
                while($service.Status -ne "Running")
                {
                    Sleep 1
                    $service.Refresh()
                }
                Write-Host
            }
        }
    }
}

function ValidateSolutionDeployment([string]$solutionName)
{
    $solution = Get-SPSolution -Identity $solutionName -EA 0

    if ($solution -ne $null)
    {
        if ($solution.Deployed -eq $true)
        {
            Write-Host -ForegroundColor Cyan "$solutionName has been successfully deployed." 
        }
    }
}

$path = Resolve-Path .\$solutionName
CheckForExistingSolution($solutionName)
Write-Host -ForegroundColor Green "Adding $solutionName to SharePoint..."
Write-Host
Add-SPSolution -LiteralPath $path | Out-Null
Write-Host -ForegroundColor Green "Installing $solutionName to SharePoint..."
Write-Host
Install-SPSolution -Identity $solutionName -GACDeployment
WaitForSPSolutionJobToComplete($solutionName)
Write-Host
ValidateSolutionDeployment($solutionName)