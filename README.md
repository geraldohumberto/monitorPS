# monitorPS

Perfil Windows e um aplicativo WPF para Windows 11 que varre processos,
servicos e itens de inicializacao, salva um perfil JSON e compara outro PC
com esse perfil.

## Baixar versao pronta

O GitHub Actions gera um pacote do aplicativo a cada push na `main`.

1. Abra a aba **Actions** do repositorio.
2. Entre no workflow `build` mais recente com status verde.
3. Baixe o artefato `PerfilWindows-win-x64`.
4. Extraia o `.zip`.
5. Execute `WindowsProfileManager.App.exe`.

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

## Fluxo principal

### Criar perfil em um PC modelo

1. Clique em `Varrer este PC`.
2. Confira processos, servicos e inicializacao.
3. Clique em `Salvar verificacao` para gerar o perfil `.json`.

### Aplicar perfil em outro PC

1. Clique em `Carregar arquivo` e selecione o `.json` criado no PC modelo.
2. Abra a aba `Perfil Carregado`.
3. Remova da lista qualquer processo, servico ou item de inicializacao que nao deve fazer parte da referencia.
4. Clique em `Varrer este PC`.
5. Clique em `Aplicar perfil carregado`.

Ao aplicar o perfil:

- processos em execucao que nao estao no perfil carregado sao encerrados;
- servicos que nao estao no perfil carregado sao parados e desativados;
- servicos que estao no perfil recebem o tipo de inicializacao salvo: `Automatico`, `Manual` ou `Desativado`;
- itens de inicializacao que nao estao no perfil carregado sao desativados;
- itens protegidos internamente sao ignorados e registrados no relatorio/log.

Algumas acoes exigem administrador. Use `Reiniciar como admin` quando necessario.
