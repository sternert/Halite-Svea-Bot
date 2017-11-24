$ErrorActionPreference='Stop'

$date = Get-Date -format ddHHmm

$assemblyName = "SveaBot$($date).exe"
Rename-Item ..\..\BattleGround\Bots\HaliteSveaBot.exe $assemblyName