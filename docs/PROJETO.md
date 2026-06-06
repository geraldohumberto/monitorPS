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
- Aba `Perfil Carregado` para revisar o JSON antes de aplicar.
- Aplicacao direta do perfil carregado no PC atual.
- Remocao de processos, servicos e itens de inicializacao do perfil carregado antes da aplicacao.
- Lista interna de protegidos.
- Encerramento imediato de processos selecionados, com bloqueio para protegidos.
- Controle imediato de servicos via `sc.exe`.
- Fila de acoes pendentes com confirmacao antes de aplicar.
- Workflow de build no GitHub Actions.

## Aplicar perfil carregado

O perfil salvo em um PC pode ser carregado em outro PC para servir como lista de referencia.

Fluxo:

1. No PC modelo, executar `Varrer este PC`.
2. Salvar o perfil em `.json`.
3. No PC alvo, carregar esse `.json`.
4. Abrir a aba `Perfil Carregado`.
5. Remover da referencia qualquer item que nao deve ser mantido.
6. Executar `Varrer este PC`.
7. Executar `Aplicar perfil carregado`.

Comportamento da aplicacao:

- processo rodando fora do perfil carregado: encerrar;
- servico fora do perfil carregado: parar e desativar;
- servico dentro do perfil carregado: aplicar o tipo salvo, quando for `Automatico`, `Manual` ou `Desativado`;
- inicializacao fora do perfil carregado: desativar;
- item protegido: ignorar e registrar no relatorio/log.

A aplicacao exige confirmacao visual antes de mexer no sistema.

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
