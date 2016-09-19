#
# Replacement.ps1
#
$lookupTable = @{
"param[][]" = "List<param>" 
"param[]" = "List<param>" 
"familiesFamilylistFamilyTypesType[]" = "List<familiesFamilylistFamilyTypesType>" 
"familiesFamilylistFamily[][]" = "List<familiesFamilylistFamily>" 
"[]" = "" 
}

$original_file = 'C:\Daten-Arbeit\Extern\BEGA\src\BEGAFamilyUpdater\BEGAFamilyUpdater\Data\DataStructure.new'
$destination_file =  'C:\Daten-Arbeit\Extern\BEGA\src\BEGAFamilyUpdater\BEGAFamilyUpdater\Data\DataStructure.cs'

Get-Content -Path $original_file | ForEach-Object { 
    $line = $_

    $lookupTable.GetEnumerator() | ForEach-Object {
        if ($line -match $_.Key)
        {
            $line = $line -replace $_.Key, $_.Value
        }
    }
   $line
} | Set-Content -Path $destination_file