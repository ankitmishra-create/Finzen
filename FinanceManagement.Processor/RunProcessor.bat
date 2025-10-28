@echo off
echo Starting Finance Management Recurring Transaction Processor...
echo Time: %date% %time%

cd /d "C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor"
dotnet run --configuration Release

echo Processor execution completed.
echo Time: %date% %time%
pause
