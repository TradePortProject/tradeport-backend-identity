
#Install Report Generator tool
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:TestResults/**/*.xml -targetdir:coveragereport

# Open coverage report in browser
Start-Process "coveragereport\index.html"