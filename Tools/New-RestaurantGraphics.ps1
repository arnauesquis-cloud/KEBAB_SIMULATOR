param([string]$Destination = (Join-Path $PSScriptRoot '..\Assets\Kebab\Art\Textures'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force -Path $Destination | Out-Null
$cream = [Drawing.ColorTranslator]::FromHtml('#FFF4DC')
$gold = [Drawing.ColorTranslator]::FromHtml('#EAB659')
$red = [Drawing.ColorTranslator]::FromHtml('#762E35')
$dark = [Drawing.ColorTranslator]::FromHtml('#25373B')
function Brush($c) { [Drawing.SolidBrush]::new($c) }
function Label($g, $text, $x, $y, $size, $color, $bold=$false) {
    $style = if($bold){[Drawing.FontStyle]::Bold}else{[Drawing.FontStyle]::Regular}
    $f = [Drawing.Font]::new('Bahnschrift',$size,$style,[Drawing.GraphicsUnit]::Pixel)
    $b = Brush $color
    $g.DrawString($text,$f,$b,[single]$x,[single]$y)
    $f.Dispose(); $b.Dispose()
}
function Canvas($name,$w,$h,$bg,$draw) {
    $bmp=[Drawing.Bitmap]::new($w,$h)
    $g=[Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint=[Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear($bg)
    & $draw $g $w $h
    $bmp.Save((Join-Path $Destination ($name+'.png')),[Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
Canvas 'Brand' 2048 640 $red {
    param($g,$w,$h)
    $p=[Drawing.Pen]::new($gold,3)
    $g.DrawRectangle($p,35,35,1978,570)
    Label $g 'B R A S A' 200 90 230 $cream $true
    Label $g 'K E B A B   ·   F E T   A L   M O M E N T' 248 380 56 $gold
    Label $g 'D Ö N E R     /     D Ü R Ü M     /     F A L A F E L' 275 491 38 $cream
    $p.Dispose()
}
Canvas 'Menu' 2048 384 $dark {
    param($g,$w,$h)
    Label $g 'LA NOSTRA CARTA' 65 18 38 $cream $true
    Label $g 'FRESC. GENERÓS. FET AQUÍ.' 1430 26 24 $gold
    $pen=[Drawing.Pen]::new($gold,3)
    $g.DrawLine($pen,65,76,1980,76)
    $columns=@(@('01 / DÖNER','Pa cruixent, carn a la brasa','amanida i salsa de la casa','6,50 €'),@('02 / DÜRÜM','Tortilla calenta, verdures','i pollastre o vedella','7,00 €'),@('03 / FALAFEL','Cigrons, espècies i herbes','amb amanida i salsa tahina','6,00 €'))
    for($i=0;$i -lt 3;$i++){
        $x=65+$i*650
        if($i -gt 0){$g.DrawLine($pen,$x-30,105,$x-30,304)}
        Label $g $columns[$i][0] $x 102 34 $gold $true
        # Original vector illustration of a filled pita, drawn directly into the sign.
        $bread=Brush ([Drawing.ColorTranslator]::FromHtml('#DDAA63'))
        $fill=Brush ([Drawing.ColorTranslator]::FromHtml('#6C3825'))
        $green=Brush ([Drawing.ColorTranslator]::FromHtml('#83A557'))
        $tomato=Brush ([Drawing.ColorTranslator]::FromHtml('#D45A43'))
        $g.FillEllipse($bread,$x+395,169,195,100)
        $g.FillEllipse($fill,$x+405,167,172,59)
        for($j=0;$j -lt 7;$j++){$g.FillEllipse($green,$x+407+$j*22,161+($j%2)*9,34,25)}
        for($j=0;$j -lt 4;$j++){$g.FillEllipse($tomato,$x+420+$j*36,178,27,22)}
        Label $g $columns[$i][1] $x 165 24 $cream
        Label $g $columns[$i][2] $x 202 24 $cream
        Label $g $columns[$i][3] $x 249 43 $gold $true
        $bread.Dispose();$fill.Dispose();$green.Dispose();$tomato.Dispose()
    }
    Label $g 'MENÚ + PATATES + BEGUDA   /   +3,00 €' 65 334 26 $cream
    Label $g 'BON PROFIT!' 1780 332 28 $gold $true
    $pen.Dispose()
}
Canvas 'Poster' 1024 1536 $cream {
    param($g,$w,$h)
    $b=Brush $red; $g.FillRectangle($b,65,65,894,1406);$b.Dispose()
    Label $g 'AQUÍ' 175 160 156 $cream $true
    Label $g 'ES FA' 175 360 156 $cream $true
    Label $g 'A FOC' 175 560 156 $gold $true
    Label $g 'LENT.' 175 760 156 $gold $true
    Label $g 'I ES MENJA DE GUST.' 172 1100 47 $cream
    Label $g 'BRASA / KEBAB' 175 1320 52 $cream $true
}
Canvas 'Wayfinding' 1024 256 $dark {
    param($g,$w,$h)
    Label $g 'WC  ←' 50 53 95 $cream $true
    Label $g 'PERSONAL  →' 490 82 47 $gold
}
Canvas 'Order' 1024 256 $red {
    param($g,$w,$h)
    Label $g 'COMANDA AQUÍ' 68 62 86 $cream $true
}
Canvas 'Pickup' 1024 256 $red {
    param($g,$w,$h)
    Label $g 'RECOLLIDA' 200 62 86 $cream $true
}
Canvas 'Hours' 512 768 $cream {
    param($g,$w,$h)
    Label $g 'BENVINGUTS' 43 90 55 $red $true
    Label $g 'OBERT' 70 220 98 $red $true
    Label $g '12.00 - 23.00' 75 410 49 $dark
    Label $g 'CADA DIA' 113 500 42 $dark
    Label $g 'BRASA / KEBAB' 73 654 36 $red $true
}
Canvas 'Fridge' 1024 256 $red {
    param($g,$w,$h)
    Label $g 'BEGUDES FRESQUES' 75 68 75 $cream $true
}
Canvas 'Register' 512 384 $dark {
    param($g,$w,$h)
    Label $g 'BRASA' 38 35 73 $cream $true
    Label $g 'Benvinguts!' 40 165 40 $gold
    Label $g 'Fet al moment.' 40 275 29 $cream
}
Write-Output 'Restaurant graphics generated.'

