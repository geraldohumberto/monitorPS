# monitorPS

Perfil Windows e um aplicativo WPF para Windows 11 que varre processos,
servicos e itens de inicializacao, salva um perfil JSON e compara outro PC
com esse perfil.

## Requisitos

- Windows 11
- .NET SDK 8 ou superior

## Build

```powershell
dotnet restore
dotnet build --configuration Release
```

## Executar

```powershell
dotnet run --project src/WindowsProfileManager.App/WindowsProfileManager.App.csproj
```
