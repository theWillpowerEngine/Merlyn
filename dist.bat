@ECHO OFF

RMDIR /s /q dist
MKDIR dist

cd dist

copy ..\shiro\bin\* .
rmdir /s /q ..\shiro\bin

del *.pdb

mkdir libs
cd libs

copy ..\..\libs\* .