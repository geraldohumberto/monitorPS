# Perfil Windows - Controle de Processos e Serviços

## Objetivo

Criar um software para Windows 11 capaz de varrer, visualizar, comparar, salvar e aplicar decisões sobre tudo que roda na máquina, usando um PC enxuto como modelo de referência.

A ideia principal é permitir que uma máquina gere um perfil com seus processos, serviços e itens de inicialização. Esse perfil pode ser enviado para outro computador, carregado no software e usado para comparar o que está rodando a mais.

O usuário deve conseguir decidir visualmente o que manter, encerrar, parar, desativar ou mudar para manual.

## Tecnologia recomendada

### Linguagem

C# com .NET 8 ou superior.

### Interface

WPF.

### Motivo da escolha

C#/.NET é a escolha mais indicada porque o projeto precisa de integração profunda com Windows:

- listar processos ativos;
- encerrar processos individualmente ou em lote;
- listar serviços do Windows;
- iniciar, parar e reiniciar serviços;
- mudar serviço para automatico, manual ou desativado;
- ler itens de inicialização;
- acessar Registro do Windows;
- detectar permissão de administrador;
- reiniciar o próprio app como administrador;
- gerar arquivos locais `.json`, `.txt` e relatórios.

WPF é recomendado em vez de uma interface web porque o software é focado em administração local do Windows. WPF também evita depender de navegador, servidor local ou empacotamento mais complexo.

## Formato de arquivo

O formato principal deve ser `.json`, porque o programa precisa ler os dados com precisão.

O `.txt` deve existir como exportação legível para humanos.

### Exemplo de perfil `.json`

```json
{
  "profileName": "Perfil Humberto",
  "createdAt": "2026-06-05T14:30:00",
  "machineName": "DESKTOP-HUMBERTO",
  "osVersion": "Windows 11",
  "processes": [
    {
      "name": "explorer.exe",
      "path": "C:\\Windows\\explorer.exe",
      "category": "Windows",
      "allowed": true
    }
  ],
  "services": [
    {
      "name": "Spooler",
      "displayName": "Print Spooler",
      "status": "Stopped",
      "startupType": "Disabled",
      "allowed": false
    }
  ],
  "startupItems": [
    {
      "name": "Steam",
      "source": "RegistryRun",
      "enabled": true,
      "command": "C:\\Program Files (x86)\\Steam\\steam.exe"
    }
  ]
}
```

## Conceito do software

O software deve trabalhar com dois estados:

- verificação atual da máquina;
- perfil carregado de um arquivo.

Com esses dois estados, ele deve mostrar:

- itens iguais ao perfil;
- itens extras no PC atual;
- itens que existem no perfil mas não existem no PC atual;
- itens protegidos;
- itens selecionados para ação imediata;
- ações pendentes.

## Layout principal

A interface deve ser uma janela única com abas, para evitar bagunça visual.

```text
+--------------------------------------------------------------------------------+
| Perfil Windows - Controle de Processos e Serviços                              |
+--------------------------------------------------------------------------------+
| [Varrer este PC] [Salvar verificação] [Carregar arquivo] [Comparar]            |
| [Aplicar ações pendentes] [Reiniciar como admin]                               |
|                                                                                |
| Perfil carregado: perfil-humberto.json                                         |
| Status: 128 processos | 83 serviços | 14 inicialização | 22 extras             |
+--------------------------------------------------------------------------------+
| [Visão Geral] [Processos] [Serviços] [Inicialização] [Comparativo]             |
| [Ações Pendentes] [Relatório]                                                  |
+--------------------------------------------------------------------------------+
|                                                                                |
| Conteúdo da aba selecionada                                                    |
|                                                                                |
+--------------------------------------------------------------------------------+
```

## Abas

### 1. Visão Geral

Mostra um resumo da máquina atual e da comparação com o perfil carregado.

Deve exibir:

- total de processos ativos;
- total de serviços rodando;
- total de itens de inicialização;
- quantidade de extras em relação ao perfil;
- quantidade de protegidos;
- quantidade de ações pendentes.

### 2. Processos

Lista todos os processos ativos.

Campos sugeridos:

- seleção;
- nome;
- PID;
- CPU;
- RAM;
- caminho;
- usuário;
- status na comparação;
- categoria.

Botões:

- Encerrar Agora;
- Adicionar para Encerrar;
- Adicionar aos Permitidos;
- Detalhes.

Requisitos:

- permitir selecionar um processo isolado;
- permitir selecionar vários processos;
- encerrar um ou vários processos de uma só vez;
- bloquear ou alertar ao tentar encerrar processos protegidos;
- registrar sucesso ou falha no relatório.

### 3. Serviços

Lista serviços do Windows.

Campos sugeridos:

- seleção;
- nome interno;
- nome de exibição;
- status;
- tipo de inicialização;
- caminho do executável, quando disponível;
- status na comparação;
- categoria.

Botões:

- Iniciar;
- Parar;
- Reiniciar;
- Automatico;
- Manual;
- Desativado;
- Adicionar aos Permitidos;
- Marcar para Desativar;
- Detalhes.

Requisitos:

- permitir ação em um serviço;
- permitir ação em vários serviços selecionados;
- distinguir parar agora de desativar inicialização;
- exigir administrador quando necessário;
- registrar sucesso ou falha no relatório.

### 4. Inicialização

Lista itens que iniciam junto com o Windows.

Fontes sugeridas:

- Registro `HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`;
- Registro `HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`;
- pasta Startup do usuário;
- pasta Startup global;
- serviços com inicialização automatica.

Botões:

- Ativar;
- Desativar;
- Adicionar aos Permitidos;
- Marcar para Desativar;
- Detalhes.

### 5. Comparativo

Tela para comparar o perfil carregado com a máquina atual.

Colunas sugeridas:

```text
+----------------------------+----------------------------+----------------------+
| Permitidos no Perfil       | Extras Neste PC            | Protegidos           |
+----------------------------+----------------------------+----------------------+
| explorer.exe               | discord.exe                | dwm.exe              |
| chrome.exe                 | onedrive.exe               | winlogon.exe         |
| steam.exe                  | teams.exe                  | nvcontainer.exe      |
+----------------------------+----------------------------+----------------------+
```

Essa aba deve permitir selecionar itens extras e decidir:

- adicionar aos permitidos;
- adicionar para encerrar;
- abrir detalhes;
- aplicar modo seguro;
- aplicar modo seguir lista.

### 6. Ações Pendentes

Mostra tudo que ainda não foi aplicado.

Deve ser uma tela de revisão antes de mexer no sistema.

Seções:

- processos para encerrar;
- serviços para parar;
- serviços para desativar;
- serviços para mudar para manual;
- itens de inicialização para desativar;
- itens adicionados aos permitidos.

Botões:

- Remover selecionados;
- Limpar tudo;
- Aplicar ações pendentes;
- Salvar ações como arquivo.

### 7. Relatório

Mostra o resultado das verificações e ações executadas.

Botões:

- Salvar verificação `.json`;
- Exportar relatório `.txt`;
- Carregar `.json`;
- Carregar `.txt`, se suportado;
- Abrir pasta de relatórios.

## Modos de aplicação

### Modo Análise

Não altera nada.

Serve apenas para:

- varrer;
- listar;
- comparar;
- gerar relatório.

### Modo Seguro

Encerra ou altera apenas itens fora do perfil, respeitando lista interna de proteção.

Deve proteger:

- processos essenciais do Windows;
- processos de vídeo/GPU;
- o próprio software;
- itens sem permissão clara;
- serviços críticos.

### Modo Seguir Lista

Mantém somente o que está no perfil carregado, mas ainda respeita uma proteção mínima para não derrubar Windows, tela, login ou driver gráfico.

Esse modo deve exigir confirmação explícita.

## Proteções internas

Mesmo no modo agressivo, o software deve proteger alguns processos e serviços.

### Processos essenciais

Exemplos iniciais:

```text
System
Registry
Idle
smss.exe
csrss.exe
wininit.exe
winlogon.exe
services.exe
lsass.exe
svchost.exe
dwm.exe
explorer.exe
audiodg.exe
fontdrvhost.exe
```

### GPU NVIDIA

Exemplos iniciais:

```text
nvcontainer.exe
nvdisplay.container.exe
nvidia share.exe
nvidia web helper.exe
nvcplui.exe
```

### GPU AMD

Exemplos iniciais:

```text
radeonsoftware.exe
amdow.exe
amdrsserv.exe
atiesrxx.exe
atieclxx.exe
cncmd.exe
```

### GPU Intel

Exemplos iniciais:

```text
igfxem.exe
igfxhk.exe
igfxtray.exe
IntelCpHDCPSvc.exe
```

## Permissão de administrador

O software deve detectar se está rodando como administrador.

Se não estiver, deve mostrar:

```text
Permissão atual: usuário comum
Algumas ações podem falhar.

[Reiniciar como administrador]
```

Com administrador, o software pode:

- encerrar mais processos;
- parar serviços;
- alterar tipo de inicialização dos serviços;
- ler mais detalhes do sistema.

## Ações imediatas vs ações pendentes

O programa deve suportar os dois jeitos.

### Ação imediata

Executa na hora.

Exemplos:

- Encerrar Agora;
- Parar;
- Iniciar;
- Reiniciar;
- Desativar.

### Ação pendente

Adiciona em uma lista para aplicar depois.

Exemplos:

- Adicionar para Encerrar;
- Marcar para Desativar;
- Marcar como Manual.

A aba Ações Pendentes deve permitir revisar tudo antes de aplicar.

## Requisitos técnicos iniciais

O projeto deve ser dividido em camadas:

```text
src/
  WindowsProfileManager.App/
    Views/
    ViewModels/
    Models/
    Services/
    App.xaml
    MainWindow.xaml

tests/
  WindowsProfileManager.Tests/

docs/
  PROJETO.md
  DECISOES.md
```

Serviços internos sugeridos:

- ProcessScannerService;
- WindowsServiceScannerService;
- StartupScannerService;
- ProfileSerializer;
- ProfileComparer;
- ActionPlanner;
- ProcessKillerService;
- ServiceControlService;
- AdminPermissionService;
- ReportWriter.

## MVP recomendado

### Fase 1

- criar projeto WPF em C#;
- criar tela com abas;
- implementar varredura de processos;
- salvar perfil em `.json`;
- exportar relatório `.txt`;
- carregar perfil `.json`;
- comparar processos atuais com perfil carregado.

### Fase 2

- encerrar processo isolado;
- encerrar vários processos selecionados;
- adicionar lista de protegidos;
- aba Ações Pendentes para processos.

### Fase 3

- listar serviços;
- iniciar/parar/reiniciar serviços;
- mudar serviço para automatico/manual/desativado;
- comparar serviços com perfil carregado.

### Fase 4

- listar inicialização;
- ativar/desativar itens de inicialização;
- comparar inicialização com perfil carregado.

### Fase 5

- modo seguro;
- modo seguir lista;
- relatórios completos;
- GitHub Actions para build e testes.

## GitHub Actions

Adicionar workflow para:

- restaurar dependências;
- compilar;
- rodar testes.

Exemplo futuro:

```yaml
name: build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test --configuration Release --no-build
```

## Regras para o Codex ao implementar

- Documentar decisões importantes em Markdown.
- Não implementar ações destrutivas sem confirmação visual.
- Separar processos, serviços e inicialização.
- Usar `.json` como formato principal.
- Usar `.txt` apenas como exportação legível.
- Não matar processos protegidos por padrão.
- Criar logs/relatórios de tudo que foi aplicado.
- Preferir código simples, claro e testável.
- Fazer commits pequenos e bem descritos.
- Antes de cada commit, rodar build e testes quando existirem.

## Nome provisório

Perfil Windows.

Outras opções:

- Windows Lean Profile;
- WinProcess Profile Manager;
- Controle de Perfil Windows;
- Perfil Enxuto Windows.
