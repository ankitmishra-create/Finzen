# Finance Management Recurring Transaction Processor
Write-Host "Starting Finance Management Recurring Transaction Processor..." -ForegroundColor Green
Write-Host "Time: $(Get-Date)" -ForegroundColor Yellow

# Change to project directory
Set-Location "C:\Users\Coditas\Desktop\FinanceManagement\FinanceManagement.Processor"

# Run the processor
dotnet run --configuration Release

Write-Host "Processor execution completed." -ForegroundColor Green
Write-Host "Time: $(Get-Date)" -ForegroundColor Yellow
