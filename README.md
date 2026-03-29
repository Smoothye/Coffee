In case of missing `Microsoft.EntityFrameworkCore` import:  
1. `dotnet nuget list source`  
2. Daca nu apare "nuget.org": `dotnet nuget add source https://api.nuget.org/v3/index.json -n "nuget.org"`  
3. `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`  
