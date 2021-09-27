cd /d "%~dp0"
mkdir "%~p1\%~n1_tile"
E:\SMS\Tools\bmp2tile-0.5\BMP2Tile.exe "%1" -removedupes -mirror -8x8 -tileoffset 65 -fullpalette -savetiles %~p1\%~n1_tile\%~n1_tile.psgcompr -savetilemap %~p1\%~n1_tile\%~n1_map.bin -savepalette %~p1\%~n1_tile\%~n1_pal.bin

cd %~p1"
E:\SMS\SDK\devkitSMS-master\folder2c\folder2c.exe "%~p1\%~n1_tile" "%~n1_tile"

pause
