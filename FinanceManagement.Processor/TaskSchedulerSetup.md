# Windows Task Scheduler Setup Guide

This guide will help you set up the Recurring Transaction Processor to run automatically using Windows Task Scheduler.

## Prerequisites

1. **Build the Application**: Ensure the application builds successfully
2. **Database Connection**: Verify your database connection string is correct
3. **Permissions**: Ensure the user account has necessary permissions

## Step 1: Create a Batch File

Create a batch file to run your processor. This makes it easier to manage and debug.

### Create `RunProcessor.bat`

```batch
@echo off
echo Starting Finance Management Recurring Transaction Processor...
echo Time: %date% %time%

cd /d "C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor"
dotnet run --configuration Release

echo Processor execution completed.
echo Time: %date% %time%
pause
```

**Important**: Update the path `C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor` to match your actual project location.

## Step 2: Create a PowerShell Script (Alternative)

If you prefer PowerShell, create `RunProcessor.ps1`:

```powershell
# Finance Management Recurring Transaction Processor
Write-Host "Starting Finance Management Recurring Transaction Processor..." -ForegroundColor Green
Write-Host "Time: $(Get-Date)" -ForegroundColor Yellow

# Change to project directory
Set-Location "C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor"

# Run the processor
dotnet run --configuration Release

Write-Host "Processor execution completed." -ForegroundColor Green
Write-Host "Time: $(Get-Date)" -ForegroundColor Yellow
```

## Step 3: Set Up Windows Task Scheduler

### Method 1: Using Task Scheduler GUI

1. **Open Task Scheduler**
   - Press `Win + R`, type `taskschd.msc`, press Enter
   - Or search "Task Scheduler" in Start menu

2. **Create Basic Task**
   - Click "Create Basic Task..." in the Actions panel
   - Name: `Finance Management Recurring Processor`
   - Description: `Automatically processes recurring transactions`

3. **Set Trigger**
   - Choose "Daily" (recommended)
   - Start date: Today
   - Start time: `00:00:00` (midnight) or your preferred time
   - Recur every: `1 days`

4. **Set Action**
   - Choose "Start a program"
   - Program/script: Browse to your batch file or PowerShell script
   - Start in: `C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor`

5. **Configure Settings**
   - Check "Run whether user is logged on or not"
   - Check "Run with highest privileges"
   - Check "Hidden" (optional)

### Method 2: Using Command Line (Advanced)

Create a PowerShell script to set up the task:

```powershell
# Create scheduled task for Finance Management Processor
$TaskName = "Finance Management Recurring Processor"
$TaskDescription = "Automatically processes recurring transactions"
$ScriptPath = "C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor\RunProcessor.bat"

# Create the task action
$Action = New-ScheduledTaskAction -Execute $ScriptPath

# Create the task trigger (daily at midnight)
$Trigger = New-ScheduledTaskTrigger -Daily -At "00:00:00"

# Create task settings
$Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

# Register the task
Register-ScheduledTask -TaskName $TaskName -Action $Action -Trigger $Trigger -Settings $Settings -Description $TaskDescription -User "SYSTEM" -RunLevel Highest

Write-Host "Task '$TaskName' created successfully!" -ForegroundColor Green
```

## Step 4: Configure Application Settings

### Update `appsettings.json` for Production

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "FinanceManagement.Processor": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=5836-LAP-0322;Database=FinanceDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "RecurringTransactionProcessor": {
    "ExecutionIntervalHours": 1,
    "MaxRetryAttempts": 3,
    "RetryDelayMinutes": 5
  }
}
```

## Step 5: Testing the Setup

### Test the Batch File Manually

1. Double-click `RunProcessor.bat`
2. Check console output for any errors
3. Verify database connections work

### Test the Scheduled Task

1. In Task Scheduler, right-click your task
2. Select "Run"
3. Check the "Last Run Result" column
4. View task history for detailed logs

## Step 6: Monitoring and Logging

### View Application Logs

The processor logs to console. For production, consider:

1. **File Logging**: Add file logging to `appsettings.json`
2. **Event Log**: Configure Windows Event Log integration
3. **Database Logging**: Store processing results in database

### Monitor Task Execution

1. **Task Scheduler History**: View task execution history
2. **Application Logs**: Check console output
3. **Database Verification**: Query transaction tables for new records

## Troubleshooting

### Common Issues

1. **Path Not Found**
   - Verify all paths in batch file are correct
   - Use absolute paths instead of relative paths

2. **Permission Denied**
   - Run Task Scheduler as Administrator
   - Check file permissions
   - Ensure user account has necessary rights

3. **Database Connection Failed**
   - Verify connection string
   - Check SQL Server is running
   - Test connection manually

4. **Dotnet Not Found**
   - Add .NET runtime to system PATH
   - Use full path to dotnet.exe in batch file

### Debug Mode

For troubleshooting, modify your batch file:

```batch
@echo off
echo Starting Finance Management Recurring Transaction Processor...
echo Time: %date% %time%

cd /d "C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor"

echo Current directory: %CD%
echo Checking dotnet installation...
dotnet --version

echo Running processor...
dotnet run --configuration Release --verbosity detailed

echo Processor execution completed.
echo Time: %date% %time%
pause
```

## Production Recommendations

1. **Use Release Configuration**: Always use `--configuration Release`
2. **Set Appropriate Intervals**: Don't run too frequently (hourly is usually sufficient)
3. **Monitor Resource Usage**: Check CPU and memory usage
4. **Backup Database**: Ensure regular database backups
5. **Error Handling**: Set up proper error notifications

## Alternative Deployment Options

### Windows Service

For more robust deployment, consider converting to a Windows Service:

1. Use `Microsoft.Extensions.Hosting.WindowsServices`
2. Install as Windows Service
3. Automatic startup and recovery

### Docker Container

For containerized deployment:

1. Create Dockerfile
2. Run in container with scheduler
3. Deploy to cloud services

## Security Considerations

1. **Service Account**: Use dedicated service account with minimal permissions
2. **Network Security**: Secure database connections
3. **Log Security**: Don't log sensitive information
4. **Access Control**: Limit access to configuration files

## Maintenance

1. **Regular Updates**: Keep .NET runtime updated
2. **Log Rotation**: Implement log file rotation
3. **Performance Monitoring**: Monitor processing times
4. **Database Maintenance**: Regular database maintenance tasks

This setup will ensure your recurring transaction processor runs automatically and reliably in your production environment.
