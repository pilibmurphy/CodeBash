<img width="500" height="575" alt="image" src="https://github.com/user-attachments/assets/eb45cea1-9804-49c2-b6dc-39514e1c2e06" />  


dotnet tool install -g dotnet-reportgenerator-globaltool;   dotnet test --collect:"XPlat Code Coverage";  
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report";   explorer.exe .\coverage-report\
