set currdir=%~dp0
for %%i in (DataGate.Com DataGate.App DataGate.Api DataGate.Tests DataGate) do (
    "%currdir%nuget.exe" pack "%currdir%..\%%i" -OutputDirectory  "%currdir%packages" -Properties Configuration=Release
);
set currdir=
pause