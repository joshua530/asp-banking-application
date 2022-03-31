#!/bin/bash

if [ ! $(command -v dotnet 2>/dev/null) ]; then
    echo "ensure dotnet sdk is installed before running this script"
    exit 1
fi

dotnet test MvcBankingApplicationTests/MvcBankingApplicationTests.csproj
