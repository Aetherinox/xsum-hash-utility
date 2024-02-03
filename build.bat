@ECHO       OFF
TITLE       Aetherx - Build xSum
SETLOCAL    ENABLEDELAYEDEXPANSION
MODE        con:cols=125 lines=30
MODE        125,30
GOTO        comment_end

-----------------------------------------------------------------------------------------------------
Build Script for xsum.exe
-----------------------------------------------------------------------------------------------------

:comment_end

:: -----------------------------------------------------------------------------------------------------
::  DEFINE BATCH VARS
:: -----------------------------------------------------------------------------------------------------

SET dir_home=%~dp0

:: -----------------------------------------------------------------------------------------------------
::  define:     gpg library
:: -----------------------------------------------------------------------------------------------------

SET signtool=signtool
SET CERT_THUMBPRINT=58a1539d6988d76f44bae27c27ed5645d3b1222a

:: -----------------------------------------------------------------------------------------------------
::  config file
:: -----------------------------------------------------------------------------------------------------

if exist !dir_home!/!file_cfg! (
    for /F "tokens=*" %%I in ( %file_cfg% ) do set %%I
)

:: -----------------------------------------------------------------------------------------------------
::  define:     signtool
::              attempt to locate signtool via where command
:: -----------------------------------------------------------------------------------------------------

WHERE /Q %signtool%

IF !ERRORLEVEL! NEQ 0 (
    cls
    %echo%   ERROR
    %echo%   This script has detected that the command %signtool% is not accessible.
    %echo%.

    TITLE Aetherx - Signtool Missing [Error]

    %echo%   Press any key to acknowledge error and try anyway  ...
    PAUSE >nul
    cls
)

:: -----------------------------------------------------------------------------------------------------
::  remove trailing slash
:: -----------------------------------------------------------------------------------------------------

IF %dir_home:~-1%==\ SET dir_home=%dir_home:~0,-1%

:: -----------------------------------------------------------------------------------------------------
::  This script requires https://www.nuget.org/packages/ilmerge
::  set your NuGet ILMerge Version, this is the number from the package manager install, for example:
::      PM> Install-Package ilmerge -Version 3.0.41
::
:: to confirm it is installed for a given project, see the packages.config file
:: -----------------------------------------------------------------------------------------------------

SET ILMERGE_VERSION=3.0.41
SET SN_KEY=aetherx_9a_sn.pub.snk

:: -----------------------------------------------------------------------------------------------------
::  target executable name
:: -----------------------------------------------------------------------------------------------------

SET APP_NAME=xsum.exe

:: -----------------------------------------------------------------------------------------------------
::  Set build, used for directory. Typically Release or Debug
:: -----------------------------------------------------------------------------------------------------

SET PATH_ROOT=bin\Release
SET PATH_PLATFORM=net481
SET PATH_PUBLISH=publish

:: -----------------------------------------------------------------------------------------------------
::  full ILMerge should be found here:
:: -----------------------------------------------------------------------------------------------------

SET PATH_ILMERGE=%USERPROFILE%\.nuget\packages\ilmerge\%ILMERGE_VERSION%\tools\net452

:: -----------------------------------------------------------------------------------------------------
::  batch script
:: -----------------------------------------------------------------------------------------------------

echo Merging %APP_NAME% ...

"%PATH_ILMERGE%"\ILMerge.exe %PATH_ROOT%\%PATH_PLATFORM%\%PATH_PUBLISH%\%APP_NAME%  ^
    /lib:%PATH_ROOT%\%PATH_PLATFORM%\%PATH_PUBLISH% ^
    /out:%PATH_ROOT%\%PATH_PLATFORM%\%PATH_PUBLISH%\%APP_NAME% ^
    System.Buffers.dll ^
    System.Management.Automation.dll ^
    System.Memory.dll ^
    SauceControl.Blake2Fast.dll ^
    System.Numerics.Vectors.dll

    GOTO SIGN_EXE_DLL_CUROLDER

:: -----------------------------------------------------------------------------------------------------
:: sign executables
:: -----------------------------------------------------------------------------------------------------

:SIGN_EXE_DLL_CUROLDER

    :: -----------------------------------------------------------------------------------------------------
    ::  sign EXE
    :: -----------------------------------------------------------------------------------------------------

    for /R %PATH_ROOT%\%PATH_PLATFORM%\%PATH_PUBLISH% %%f in ( *.exe ) do (
        call signtool sign /sha1 "%CERT_THUMBPRINT%" /fd SHA256 /d "Aetherx" /du "https://github.com/Aetherinox" /t http://timestamp.comodoca.com/authenticode "%%f"
    )

    goto FINISH

:: -----------------------------------------------------------------------------------------------------
::  finish
:: -----------------------------------------------------------------------------------------------------

:FINISH
    dir %APP_NAME%
    pause
    Exit /B 0