# Perfil Windows

Implementacao inicial do software descrito em `PROJETO.md`.

## Estrutura

```text
src/
  WindowsProfileManager.App/
    Models/
    Services/
    ViewModels/
    App.xaml
    MainWindow.xaml
tests/
  WindowsProfileManager.Tests/
docs/
  PROJETO.md
  DECISOES.md
```

## MVP entregue

- Projeto WPF em C# para Windows.
- Tela com abas: visao geral, processos, servicos, inicializacao, comparativo, acoes pendentes e relatorio.
- Varredura de processos.
- Varredura de servicos.
- Varredura de itens de inicializacao.
- Salvamento de perfil `.json`.
- Carregamento de perfil `.json`.
- Exportacao de relatorio `.txt`.
- Comparacao do estado atual com perfil carregado.
- Lista interna de protegidos.
- Encerramento imediato de processos selecionados, com bloqueio para protegidos.
- Controle imediato de servicos via `sc.exe`.
- Fila de acoes pendentes com confirmacao antes de aplicar.
- Workflow de build no GitHub Actions.

## Como compilar

Requer .NET SDK 8 ou superior no Windows.

```powershell
dotnet restore
dotnet build --configuration Release
```

## Como executar

```powershell
dotnet run --project src/WindowsProfileManager.App/WindowsProfileManager.App.csproj
```

Algumas acoes precisam de administrador. O app mostra a permissao atual e oferece reinicio elevado.
