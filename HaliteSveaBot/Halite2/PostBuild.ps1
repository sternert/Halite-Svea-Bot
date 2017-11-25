$ErrorActionPreference='Stop'

$date = Get-Date -format ddHHmm

$assemblyName = "SveaBot$($date).exe"
#Rename-Item ..\..\BattleGround\Bots\HaliteSveaBot.exe $assemblyName

Copy-Item -Path ..\..\BattleGround\Bots\HaliteSveaBot.exe -Destination ..\..\BattleGround\Bots\$assemblyName