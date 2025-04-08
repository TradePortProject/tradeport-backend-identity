# CI/CD Pipeline for Identity Microservice

This document provides an overview of the CI/CD pipelines for the Tradeport Identity Microservice.

## Workflow Structure

Our CI/CD pipeline is structured into three main workflows:

### 1. CI Pipeline (`ci.yml`)

Triggered on:
- Push to main, develop, feature/*, release/* branches
- Pull requests to main or develop
- Manual dispatch

Jobs:
- **Build & Test**: Compiles the code and runs unit tests with code coverage
- **Code Quality**: Runs code style checks and SonarQube analysis
- **Security Scanning**: Basic NuGet vulnerability checking
- **Docker Build**: Builds Docker image (without publishing) for verification

### 2. Publish Pipeline (`publish.yml`)

Triggered on:
- Push to main or develop
- Tags starting with 'v'
- Manual dispatch with environment selection

Jobs:
- **Build & Publish**: Builds and publishes Docker image to GitHub Container Registry
- **Deploy to Staging**: Deploys to staging environment
- **Deploy to Production**: Deploys to production environment (only for tags or manual selection)

### 3. Security Scan Pipeline (`security-scan.yml`)

Triggered on:
- Weekly schedule (Monday at 2am)
- Manual dispatch
- Push to main that changes dependency files

Jobs:
- **Security Scan**: Runs NuGet vulnerability scans and SAST scans
- **Dependency Check**: Runs OWASP Dependency Check for comprehensive vulnerability scanning
- **Outdated Packages**: Checks for outdated NuGet packages
- **Summary**: Generates a summary report of all security findings

## Artifacts

The pipelines generate several artifacts:
- Test results and code coverage reports
- Security scan reports
- Docker images
- Package update reports

## Environment Configuration

The pipelines use these GitHub secrets:
- `GITHUB_TOKEN`: Automatically provided by GitHub Actions
- `SONAR_TOKEN`: For SonarQube analysis (optional)
- `SONAR_PROJECT_KEY`: SonarQube project key (optional)
- `SONAR_ORGANIZATION`: SonarQube organization (optional)

## Deployment Flow

The deployment process follows a progression:
1. Code is built and tested in the CI pipeline
2. Images are published to the container registry
3. Automatic deployment to staging for develop branch or manual staging selection
4. Production deployment for tagged releases or manual production selection

## Getting Started

To manually run these workflows:
1. Go to the Actions tab in your GitHub repository
2. Select the workflow you want to run
3. Click "Run workflow" and select the branch

## Customization

These pipelines can be customized:
- Change the .NET version in the `dotnet-version` parameter
- Modify Docker image tags or registry locations
- Add specific deployment steps for your environment