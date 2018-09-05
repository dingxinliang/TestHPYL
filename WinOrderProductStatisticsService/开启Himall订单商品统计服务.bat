@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinOrderProductStatisticsService.exe"
net start Himall预约单诊疗项目统计服务
pause
@pause



