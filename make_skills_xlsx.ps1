$headers = @(
    "Skill ID / Name",
    "Damage Type",
    "Scale Stat",
    "Cost",
    "Multiplier",
    "Self Damage",
    "Target Curse Type",
    "Target Curse Chance",
    "Self Curse Type",
    "Self Curse Chance"
)

$rows = @(
    @("01 Strong Slash","Physical","Attack","HP 10% / MP 0","2.0 ~ 2.5","0% (chance 0%)","","0%","","0%"),
    @("02 Fireball","Magic","Magic","HP 0% / MP 5","1.8 ~ 2.3","10% (chance 0%)","","0%","","0%"),
    @("03 Slash","Physical","Attack","HP 5% / MP 0","1.5 ~ 1.8","0% (chance 0%)","","0%","","0%"),
    @("04 Meditation","Magic","Magic","HP 0% / MP 0","0 ~ 0","0% (chance 0%)","","0%","","0%"),
    @("05 Magic Bolt","Magic","Magic","HP 0% / MP 3","1.2 ~ 1.3","10% (chance 5%)","","0%","","0%"),
    @("06 Quickhand","Physical","Agility","HP 0% / MP 0","0.6 ~ 0.8","0% (chance 0%)","","0%","","0%"),
    @("07 Shield Wall","Physical","Attack","HP 0% / MP 0","0 ~ 0","0% (chance 0%)","","0%","","0%")
)

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$workbook = $excel.Workbooks.Add()
$sheet = $workbook.Worksheets.Item(1)
$sheet.Name = "Skills"

for ($i = 0; $i -lt $headers.Count; $i++) {
    $sheet.Cells.Item(1, $i + 1) = $headers[$i]
}

for ($r = 0; $r -lt $rows.Count; $r++) {
    for ($c = 0; $c -lt $headers.Count; $c++) {
        $sheet.Cells.Item($r + 2, $c + 1) = $rows[$r][$c]
    }
}

$path = Join-Path (Get-Location) "SkillDataList.xlsx"
$workbook.SaveAs($path)
$workbook.Close($true)
$excel.Quit()

[System.Runtime.Interopservices.Marshal]::ReleaseComObject($sheet) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null

Write-Output "Saved: $path"







