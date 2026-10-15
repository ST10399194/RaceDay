name: RaceDay API CI/CD

on:
  push:
    branches: [ main, master ]
  pull_request:
    branches: [ main, master ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    # 1. Checkout repository
    - name: Checkout Repository
      uses: actions/checkout@v4

    # 2. Install .NET
    - name: Setup .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    # 3. Restore packages
    - name: Restore Dependencies
      run: dotnet restore RaceDay.sln

    # 4. Build solution
    - name: Build Solution
      run: dotnet build RaceDay.sln --configuration Release --no-restore

    # 5. Run unit tests -> Green build
    - name: Run xUnit Automation Tests
      run: dotnet test RaceDay.sln --configuration Release --no-build --verbosity normal