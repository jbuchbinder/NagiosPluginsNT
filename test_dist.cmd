@echo off

set DIST_DIR=%~dp0\..\dist
set TEST_OUTPUT=%DIST_DIR%\test.log

del %TEST_OUTPUT%

%DIST_DIR%\check_cpu.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_disk_free.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_disk_time.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_disk_use.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_http.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_mem.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_ping.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_reg.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_services_stopped.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_snmp.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_snmp_if.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_swap.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_tcp.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_uname.exe 1>>%TEST_OUTPUT% 2>&1
%DIST_DIR%\check_uptime.exe 1>>%TEST_OUTPUT% 2>&1
