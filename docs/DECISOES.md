# Decisoes

## Plataforma

- O aplicativo foi estruturado em C# com WPF e alvo `net8.0-windows`, conforme pedido no projeto.
- A interface usa uma janela unica com abas para manter varredura, comparacao, acoes pendentes e relatorio no mesmo fluxo.

## Integracao com Windows

- A varredura de processos usa `System.Diagnostics.Process`, sem dependencias externas.
- A varredura de servicos usa `sc.exe` e Registro do Windows para evitar pacote NuGet adicional.
- Itens de inicializacao sao lidos do Registro `Run`, pastas Startup e servicos automaticos.

## Seguranca

- Processos e servicos essenciais entram em uma lista interna de protecao.
- Acoes pendentes exigem confirmacao visual antes de aplicar.
- Processos protegidos nao sao encerrados pelo app.
- Servicos protegidos nao sao parados nem desativados pelo app.

## Formatos

- Perfis sao salvos e carregados em JSON.
- Relatorios humanos sao exportados em TXT.

## Limite atual

- A ativacao de itens de inicializacao desativados ainda nao foi implementada porque exige preservar metadados do local original.
- CPU e usuario de processo ficam limitados ao que pode ser lido sem WMI ou pacotes externos.
