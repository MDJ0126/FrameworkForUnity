@echo off
setlocal
set "DOC_NODE="
for /f "delims=" %%I in ('where node.exe 2^>nul') do if not defined DOC_NODE set "DOC_NODE=%%I"
if not defined DOC_NODE set "DOC_NODE=%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe"
if not exist "%DOC_NODE%" (
    echo Node.js was not found. Install Node.js or check the Codex runtime path.
    set "DOC_EXIT=1"
    goto finish
)
"%DOC_NODE%" "%~dp0build.mjs"
set "DOC_EXIT=%ERRORLEVEL%"
if "%DOC_EXIT%"=="0" (
    echo Documentation build completed.
) else (
    echo Documentation build failed.
)
:finish
if /i not "%~1"=="--no-pause" pause
exit /b %DOC_EXIT%
