@echo off

echo BUILD START

echo ------------------
echo MyGame
call protoc --csharp_out=./output ./MyGame.proto
echo MyGame OK

echo ------------------
echo BUILD COMPLETE

pause