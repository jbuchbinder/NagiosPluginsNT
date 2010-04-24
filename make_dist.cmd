@echo off

set DIST_DIR=%~dp0\..\dist

del /q %DIST_DIR%\*.*
rmdir %DIST_DIR%
mkdir %DIST_DIR%

copy %~dp0\CHANGELOG.txt %DIST_DIR%
copy %~dp0\LICENSE.txt %DIST_DIR%
copy %~dp0\check_cpu\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_disk_free\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_disk_time\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_http\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_mem\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_ping\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_reg\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_services_stopped\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_snmp\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_snmp_if\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_swap\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_tcp\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_uname\bin\Release\*.exe %DIST_DIR%
copy %~dp0\check_uptime\bin\Release\*.exe %DIST_DIR%
copy %~dp0\NagiosPluginsNT\lib\Mono.GetOptions.dll %DIST_DIR%
copy %~dp0\NagiosPluginsNT\lib\snmp.dll %DIST_DIR%