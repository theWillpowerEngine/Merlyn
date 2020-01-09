RMDIR /s /q dist
MKDIR dist

cd dist
mkdir Sense

copy ..\shiro\bin\* .

del *.pdb

rmdir /s /q libs
mkdir libs
cd libs

xcopy ..\..\libs\dist . /s /e

cd ..
cd Sense

copy ..\..\sense\bin\dist\* .