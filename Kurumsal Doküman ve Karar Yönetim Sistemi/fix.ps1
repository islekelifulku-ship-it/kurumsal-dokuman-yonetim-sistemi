$replacements = @{
    "â€“" = "–"
    "â†’" = "→"
    "âœ•" = "✕"
    "Â·" = "·"
    "âœ“" = "✓"
    "â†—" = "↗"
}
$files = Get-ChildItem "c:\Users\ELİF\source\repos\Kurumsal Doküman ve Karar Yönetim Sistemi\Kurumsal Doküman ve Karar Yönetim Sistemi\wwwroot\*.html"
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    foreach ($k in $replacements.Keys) {
        $content = $content.Replace($k, $replacements[$k])
    }
    [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
}
