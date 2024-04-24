@echo off

echo BUILD START

echo ------------------
echo MyGame
call protoc --csharp_out=./output ./MyGame.proto
echo MyGame OK

echo Conn
call protoc --csharp_out=./output ./Conn.proto
echo Conn OK

echo Player
call protoc --csharp_out=./output ./Player.proto
echo Player OK

echo ------------------
echo BUILD COMPLETE

pause