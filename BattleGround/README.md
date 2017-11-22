To run match against itself with random map:
    ./halite.exe ./Bots/HaliteSveaBot.exe ./Bots/HaliteSveaBot.exe

More settings at: https://halite.io/learn-programming-challenge/halite-cli-and-tools/cli


After battle you can upload hlt replay to halite.io to see the match or download and run the website locally.


If you want to run a lot of matches you can run with the gym command (-i ITERATIONS):
python.exe client.py gym -r .\Bots\HaliteSveaBot.exe -r .\Bots\HaliteSveaBot.exe -b "halite" -i 10 -H 240 -W 160