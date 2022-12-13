@echo off

set RUN_COMMAND=dotnet ef migrations add

if '%1' == '' goto USAGE
if '%2' == '' goto USAGE
if not '%3' == '' goto USAGE

set PROJECT_DIR=%1
set MIGRATION_NAME=%2

pushd %PROJECT_DIR%

%RUN_COMMAND% %MIGRATION_NAME% --configuration MIGRATION
if ERRORLEVEL 1 echo Failed to create migration

popd

exit /b %ERRORLEVEL%


:USAGE
echo.
echo This batch-file helps to generate database migration source files with the '%RUN_COMMAND%' command.
echo.
echo USAGE:

echo %~n0 ^<PROJECT_DIR^> ^<MIGRATION_NAME^>
echo.
echo ^<PROJECT_DIR^>    - the project diretcory when the migration is required.
echo ^<MIGRATION_NAME^> - the name of the migration
echo                    (equals to the 'name' parameter of the '%RUN_COMMAND%' command).

exit /b 1

