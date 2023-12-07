function KillProcessesWithHandles
{
    param([string]$path)
	
	[cmdletBinding(SupportsShouldProcess=$true)]
	param(
	[Parameter(Position = 0, Mandatory = $true)]
	[ValidateScript({
		$vr = Test-Path $_
		if(!$vr){Write-Host "The provided path $_ is invalid!"}
		$vr
	})][String]$Path
	)
	
	Write-Host "Provided path: $_"

	$allProcesses = Get-Process
    
    # Start by closing all chromedriver processes,which may have left after previous test run
    $allProcesses | where {$_.Name -eq "chromedriver.exe"} | Stop-Process -Force -ErrorAction SilentlyContinue
    
    # Then close all processes running inside the folder we are trying to delete
    $allProcesses | where {$_.Path -like ($path.substring(4) + "*")} | Stop-Process -Force -ErrorAction SilentlyContinue
    
    # Finally close all processes with modules loaded from folder we are trying to delete
    foreach($lockedFile in Get-ChildItem -Path $path -Include * -Recurse) {
        foreach ($process in $allProcesses) {
            $process.Modules | where {$_.FileName -eq $lockedFile} | Stop-Process -Force -ErrorAction SilentlyContinue
        }
    }
}
