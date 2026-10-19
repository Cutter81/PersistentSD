# PersistentSD generated script template.
# The manager replaces the two tokens below before installing the task.
$ErrorActionPreference = 'Stop'
$script:DeviceUniqueId = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('__DEVICE_UNIQUE_ID_BASE64__'))
$script:PreferredLetter = '__PREFERRED_LETTER__'
if ($script:PreferredLetter -notmatch '^[D-Z]$') {
    throw "Invalid preferred drive letter '$($script:PreferredLetter)'. PersistentSD only allows D: through Z:; A:, B:, and C: are prohibited."
}
$script:LogPath = Join-Path $PSScriptRoot 'PersistentSD.log'
$script:EventSource = 'PersistentSDVolumeChange'

function Write-Log {
    param([string]$Message)
    try {
        $line = '{0:yyyy-MM-dd HH:mm:ss}  {1}' -f (Get-Date), $Message
        Add-Content -LiteralPath $script:LogPath -Value $line -Encoding UTF8
    } catch { }
}

function Get-TargetDisk {
    $foundDisks = @(Get-Disk -ErrorAction Stop | Where-Object { ([string]$_.UniqueId).Trim() -eq $script:DeviceUniqueId })
    if ($foundDisks.Count -ne 1) {
        Write-Log ("Expected exactly one disk matching configured UniqueId; found {0}. No changes made." -f $foundDisks.Count)
        return $null
    }
    return $foundDisks[0]
}

function Set-PreferredDriveLetter {
    try {
        $disk = Get-TargetDisk
        if ($null -eq $disk) { return }
        if ($disk.IsOffline) {
            Write-Log ("Selected disk {0} is offline; refusing to change its state automatically." -f $disk.Number)
            return
        }

        $parts = @(Get-Partition -DiskNumber $disk.Number -ErrorAction Stop | Where-Object {
            $_.Size -gt 0 -and $_.Type -notin @('Reserved','System','Recovery','Unknown') -and
            -not $_.IsSystem -and -not $_.IsBoot
        } | Sort-Object PartitionNumber)
        if ($parts.Count -eq 0) {
            Write-Log ("No eligible data partition found on selected disk number {0}." -f $disk.Number)
            return
        }

        # Prefer the partition already holding the requested letter, then an existing
        # drive-letter partition, then the first eligible data partition.
        $target = $parts | Where-Object { [string]$_.DriveLetter -eq $script:PreferredLetter } | Select-Object -First 1
        if (-not $target) { $target = $parts | Where-Object { $_.DriveLetter } | Select-Object -First 1 }
        if (-not $target) { $target = $parts | Select-Object -First 1 }
        if ([string]$target.DriveLetter -eq $script:PreferredLetter) {
            Write-Log ("Drive letter {0}: is already assigned to the selected device." -f $script:PreferredLetter)
            return
        }

        $owner = Get-Partition -DriveLetter $script:PreferredLetter -ErrorAction SilentlyContinue
        if ($owner) {
            # Never take a drive letter away from another volume. If it is occupied,
            # leave everything as-is and wait for a later retry.
            Write-Log ("Preferred letter {0}: is already in use by disk {1}, partition {2}; leaving it untouched." -f $script:PreferredLetter, $owner.DiskNumber, $owner.PartitionNumber)
            return
        }

        Set-Partition -DiskNumber $disk.Number -PartitionNumber $target.PartitionNumber -NewDriveLetter $script:PreferredLetter -ErrorAction Stop
        Write-Log ("Assigned {0}: to disk {1}, partition {2} ({3})." -f $script:PreferredLetter, $disk.Number, $target.PartitionNumber, $disk.FriendlyName)
    }
    catch { Write-Log ("Assignment attempt failed: {0}" -f $_.Exception.Message) }
}

Write-Log 'PersistentSD started.'
# Allow Windows storage discovery to settle; retry for up to about one minute.
for ($attempt = 0; $attempt -lt 12; $attempt++) {
    Set-PreferredDriveLetter
    $disk = Get-TargetDisk
    if ($disk) {
        $hasLetter = @(Get-Partition -DiskNumber $disk.Number -ErrorAction SilentlyContinue | Where-Object { [string]$_.DriveLetter -eq $script:PreferredLetter }).Count -gt 0
        if ($hasLetter) { break }
    }
    Start-Sleep -Seconds 5
}

try {
    Register-WmiEvent -Class Win32_VolumeChangeEvent -SourceIdentifier $script:EventSource -ErrorAction Stop | Out-Null
    Write-Log 'Volume arrival watcher registered.'
    while ($true) {
        $event = Wait-Event -SourceIdentifier $script:EventSource -Timeout 60
        if ($event) {
            if ($event.SourceEventArgs.NewEvent.EventType -eq 2) {
                Start-Sleep -Seconds 2
                Set-PreferredDriveLetter
            }
            Remove-Event -EventIdentifier $event.EventIdentifier -ErrorAction SilentlyContinue
        } else {
            # Periodic retry also covers cases where Windows misses an arrival event.
            Set-PreferredDriveLetter
        }
    }
}
catch { Write-Log ("Watcher error: {0}" -f $_.Exception.Message); exit 1 }
finally { Unregister-Event -SourceIdentifier $script:EventSource -ErrorAction SilentlyContinue }
