RMDIR /s /q dist
MKDIR dist

cd dist

copy ..\shiro\bin\* .

del *.pdb

rmdir /s /q libs
mkdir libs
cd libs

xcopy ..\..\libs\dist . /s /e