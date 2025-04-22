# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

UserManagement.Tests\TestResults\<GUID>\

dotnet tool install --global dotnet-reportgenerator-globaltool


reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html

# Open the report in the default browser
#Start-Process "coveragereport\index.html"
