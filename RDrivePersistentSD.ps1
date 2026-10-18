$targetLetter = 'R'
$readerName   = 'Realtek PCIE Card Reader'
$eventName    = 'RDriveVolumeChange'

function Set-ReaderCardToR {

    # Find the Realtek PCIe card reader.
    $readerDisks = Get-Disk -ErrorAction SilentlyContinue |
        Where-Object {
            $_.FriendlyName -eq $readerName
        }

    foreach ($disk in $readerDisks) {

        # Find the actual data partition on the reader.
        $cardPartitions = Get-Partition `
            -DiskNumber $disk.Number `
            -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Size -gt 0 -and
                $_.Type -notin @('Reserved','System','Recovery')
            }

        foreach ($card in $cardPartitions) {

            # ----------------------------------------------------
            # Already R: — nothing to do.
            # ----------------------------------------------------
            if ($card.DriveLetter -eq $targetLetter) {
                return
            }

            # ----------------------------------------------------
            # Something else has R:.
            #
            # Remove ONLY its R: drive-letter access path.
            # We are NOT ejecting/offlining the physical device.
            # ----------------------------------------------------
            $rOwner = Get-Partition `
                -DriveLetter $targetLetter `
                -ErrorAction SilentlyContinue

            if ($rOwner) {

                try {
                    Remove-PartitionAccessPath `
                        -DiskNumber $rOwner.DiskNumber `
                        -PartitionNumber $rOwner.PartitionNumber `
                        -AccessPath 'R:' `
                        -ErrorAction Stop
                }
                catch {
                    return
                }
            }

            # ----------------------------------------------------
            # Now give R: to the card.
            # ----------------------------------------------------
            try {

                Set-Partition `
                    -DiskNumber $disk.Number `
                    -PartitionNumber $card.PartitionNumber `
                    -NewDriveLetter $targetLetter `
                    -ErrorAction Stop

                return
            }
            catch {
                return
            }
        }
    }
}

# ================================================================
# INITIAL BOOT CHECK
#
# If a card was already inserted before Windows booted,
# catch it immediately.
# ================================================================

Start-Sleep -Seconds 1

Set-ReaderCardToR


# ================================================================
# EVENT-DRIVEN MONITOR
#
# Windows tells us when a volume appears.
# No polling loop.
# ================================================================

Register-WmiEvent `
    -Class Win32_VolumeChangeEvent `
    -SourceIdentifier $eventName `
    -ErrorAction Stop |
    Out-Null

try {

    while ($true) {

        # Sleep here until Windows actually generates an event.
        $event = Wait-Event -SourceIdentifier $eventName

        if ($event) {

            $eventType = $event.SourceEventArgs.NewEvent.EventType

            # EventType 2 = device/volume arrival.
            if ($eventType -eq 2) {

                # Give Windows a moment to finish mounting it.
                Start-Sleep -Milliseconds 500

                Set-ReaderCardToR
            }

            Remove-Event `
                -EventIdentifier $event.EventIdentifier `
                -ErrorAction SilentlyContinue
        }
    }
}
finally {

    Unregister-Event `
        -SourceIdentifier $eventName `
        -ErrorAction SilentlyContinue
}