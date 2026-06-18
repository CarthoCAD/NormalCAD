# NormalCAD

> Protótipo de sistema CAD 2D simples desenvolvido em **C#** com **Avalonia UI**, estruturado na arquitetura **MVC** e com suporte a leitura e escrita de arquivos **DXF/DWG** via biblioteca **ACadSharp**.

---

## Visão Geral

O **NormalCAD** é um protótipo de aplicação de desenho técnico (CAD) 2D de código aberto. Seu objetivo é demonstrar como construir um sistema CAD funcional usando tecnologias .NET modernas e abertas, sem depender de bibliotecas proprietárias como o SDK do AutoCAD.

A arquitetura interna do banco de dados do desenho foi modelada com base na **API .NET do AutoCAD** (ObjectARX/Managed), utilizando os mesmos conceitos de `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable`, `ViewportTable` e entidades (`Entity`), tornando o projeto familiar para desenvolvedores da área de AEC (Arquitetura, Engenharia e Construção).

---

## Funcionalidades (v0.1)

### 🖊️ Ferramentas de Desenho

- **Linha** — Desenho em cadeia (o final de uma linha é o início da próxima), com preview dinâmico tracejado durante o posicionamento.
- **Círculo** — Definição por clique no centro e arrastar para ajustar o raio, com preview em tempo real.
- **Polilinha** — Cliques sucessivos adicionam vértices; `Enter` finaliza aberta, `C` finaliza fechada. Preview em tempo real com rubber-band até o cursor.
- **Seleção** — Clique para selecionar entidades individualmente; `Ctrl + Clique` para seleção múltipla acumulativa. Arraste da esquerda→direita para Window Select ou direita→esquerda para Crossing Select. O conjunto de seleção é gerenciado pelo `CadController` com API dedicada (`AddToSelection`, `RemoveFromSelection`, `ClearSelection`, `IsSelected`).
- **Exclusão** — Tecla `Delete` remove todos os objetos selecionados.
- **Limpar** — Botão para apagar todo o desenho atual.

### 🧲 Object Snapping (Atração Magnética)

Atração automática do cursor a pontos notáveis de entidades existentes ao aproximar o mouse:

- **Endpoint** — Extremidades de linhas, arcos e vértices de polilinhas (indicador: **caixa verde**).
- **Midpoint** — Ponto médio de linhas e segmentos de polilinhas (indicador: **triângulo verde**).
- **Center** — Centro de círculos e arcos (indicador: **círculo verde**).

### 🗺️ Viewport e Navegação

- **Zoom** — Scroll do mouse com foco na posição do cursor.
- **Pan** — Arrastar com o botão do meio do mouse.
- **Grade Adaptativa** — Grade cartesiana que se adapta automaticamente ao nível de zoom (ex: escala de 1, 5, 10, 50, 100 unidades).
- **Persistência de Viewport** — A posição da vista (`WorldCenter` + `Zoom`) é salva no registro `*ACTIVE` da `ViewportTable` e restaurada ao reabrir o arquivo.

### 📂 Importação / Exportação DXF e DWG

Usando a biblioteca **ACadSharp** (versão 3.6.29, licença MIT):

- **Abrir DXF** — Importa linhas, círculos, arcos e polilinhas (`LwPolyline`). As camadas e viewports do arquivo são recriadas no banco de dados interno. Polilinhas preservam sua estrutura de vértices e estado de fechamento.
- **Salvar DXF** — Exporta o desenho atual para um arquivo `.dxf` compatível com AutoCAD, LibreCAD, QCAD e outros softwares CAD.
- **Abrir DWG** — Suporte a leitura de arquivos `.dwg` nas versões R14 até 2020 (AC1014–AC1032). Versões não suportadas exibem mensagem informativa.
- **Salvar DWG** — Exporta usando o formato AC1032 (AutoCAD 2018–2020), a versão mais suportada pelo ACadSharp.

### ⌨️ Linha de Comando Integrada (estilo AutoCAD)

- **Prompt de Comandos** — `TextBox` na barra inferior: digite o nome ou alias de um comando e pressione `Enter` ou `Espaço` para executá-lo. `Escape` limpa a barra e cancela o comando ativo.
- **Popup Flutuante** — Mensagens de feedback do sistema aparecem acima da barra de comando com animação de fade-in/fade-out, sobre o viewport.
- **Prefixo Dinâmico** — O indicador à esquerda da caixa de texto (`CMD:`) atualiza automaticamente para o nome do comando ativo (ex: `LINE:`, `CIRCLE:`).
- **Redirecionamento Automático** — Digitar qualquer caractere fora da caixa de comando move o foco automaticamente para ela.
- **Aliases** — Cada comando registra automaticamente seus aliases (ex: `C` ou `CI` para `CIRCLE`). O `CmdManager` resolve aliases via descoberta por reflection.
- **Histórico de Prompts** — O `InputManager` mantém os últimos 100 prompts/mensagens para consulta futura via `GetRecentPrompts()`.

### 🎨 Interface e Temas

- **Painel de Propriedades** — Exibe as coordenadas e a camada da entidade selecionada em tempo real.
- **Gerenciador de Camadas** — Cria e ativa camadas personalizadas com cores únicas.
- **Barra de Status** — Coordenadas do cursor (mundo real), botão **Model**, prefixo de comando dinâmico e prompt de comandos com auto-foco/aliases.
- **Popup de Feedback** — Mensagens do sistema com fade-in/fade-out animado sobre o viewport, estilizadas de acordo com o tema ativo.
- **Alternância de Tema** — Suporte dinâmico a **Dark Mode** e **Light Mode** sem reinicialização, com recursos de cor (`Theme.PopupBg`, `Theme.PopupText`, etc.) definidos via dicionários `Colors.axaml` e `ThemeTokens.axaml`.

---

## Arquitetura (MVC)

O projeto segue rigorosamente o padrão MVC e é organizado nos seguintes pacotes:

**Registro automático de comandos:** O `CmdManager` utiliza **reflection** para descobrir automaticamente todas as classes que implementam `ICadCommand` no assembly. Para cada comando, registra entradas para seu `Name` (ex: `_.CIRCLE`), `LocalName` (ex: `CIRCLE`) e `Alias` (ex: `C,CI`), eliminando a necessidade de registro ou mapeamento manual de aliases.

**Sistema de prompts:** O `InputManager` expõe `SetPromptMessage(string)` para feedback ao usuário (exibido no popup flutuante) e `SetCurrentPrompt(string)` para atualizar o prefixo da linha de comando conforme o comando ativo. O `CadController.SetCommand()` sincroniza automaticamente o `CurrentPrompt`. A `BottomBar` assina `CurrentPromptChanged` e `PromptMessageChanged` para manter a interface atualizada.

```bash
NormalCAD/
├── NormalCAD.sln
└── NormalCAD/
    ├── Core/                          # MODEL — Banco de dados e entidades CAD
    │   ├── Database.cs                # Container principal do desenho
    │   ├── ObjectId.cs                # Identificador único para objetos
    │   ├── DBObject.cs                # Classe base para objetos persistíveis
    │   ├── Entity.cs                  # Classe base para entidades geométricas
    │   ├── EntityColor.cs             # Cor de entidade (ByLayer ou RGBA)
    │   ├── OpenMode.cs                # Enum ForRead / ForWrite
    │   ├── SnapType.cs                # Enum de tipos de snap
    │   ├── SymbolTable.cs             # Tabela de símbolos genérica
    │   ├── SymbolTableRecord.cs       # Registro genérico de tabela
    │   ├── BlockTable.cs              # Tabela de blocos
    │   ├── BlockTableRecord.cs        # Bloco (inclui ModelSpace)
    │   ├── LayerTable.cs              # Tabela de camadas
    │   ├── LayerTableRecord.cs        # Definição de uma camada
    │   ├── ViewportTable.cs           # Tabela de viewports
    │   ├── ViewportTableRecord.cs     # Registro de viewport (*Active, centro, altura)
    │   ├── Transaction.cs             # Transação de modificação no DB
    │   ├── TransactionManager.cs      # Gerenciador da pilha de transações
    │   └── Geometry/
    │       ├── Point3d.cs             # Ponto tridimensional
    │       └── Vector3d.cs            # Vetor tridimensional
    │
    ├── View/                          # VIEW — Interface Avalonia UI
    │   ├── Controls/
    │   │   ├── CadViewport.cs         # Controle de renderização e navegação
    │   │   ├── BottomBar.axaml/.cs    # Barra de status, linha de comando e popup
    │   │   ├── MenuBar.axaml/.cs      # Barra de menu superior
    │   │   ├── PropertyPalette.axaml/.cs  # Painel de propriedades
    │   │   └── LayerPalette.axaml/.cs     # Painel de camadas
    │   └── Drawing/
    │       ├── IEntityRenderer.cs     # Interface de renderização de entidade
    │       ├── DrawingService.cs      # Registro de renderizadores + cache de cores por layer
    │       ├── LineRenderer.cs        # Renderizador de linha
    │       ├── CircleRenderer.cs      # Renderizador de círculo
    │       ├── ArcRenderer.cs         # Renderizador de arco
    │       └── PolylineRenderer.cs    # Renderizador de polilinha
    │
    ├── Controller/                    # CONTROLLER — Lógica e comandos
    │   ├── CadController.cs           # Orquestrador central do MVC + seleção
    │   ├── InputManager.cs            # Gerenciador de entrada, prompts e CurrentPrompt
    │   ├── CmdManager.cs              # Descoberta, registro e despacho de comandos
    │   ├── Commands/
    │   │   ├── ICadCommand.cs         # Interface de comando (Name, LocalName, Alias, IsInternal)
    │   │   ├── BaseCommand.cs         # Comando padrão de seleção (interno)
    │   │   ├── DrawLineCommand.cs     # Ferramenta de linha (_.LINE)
    │   │   ├── DrawCircleCommand.cs   # Ferramenta de círculo (_.CIRCLE)
    │   │   ├── DrawPolylineCommand.cs # Ferramenta de polilinha (_.PLINE)
    │   │   ├── EraseCommand.cs        # Excluir selecionados (_.ERASE)
    │   │   ├── CleanAllCommand.cs     # Limpar todo o desenho (_.CLEANALL)
    │   │   ├── OpenDxfCommand.cs      # Abrir arquivo DXF (_.DXFIN)
    │   │   ├── SaveDxfCommand.cs      # Salvar arquivo DXF (_.DXFOUT)
    │   │   ├── OpenDwgCommand.cs      # Abrir arquivo DWG (_.DWGIN)
    │   │   ├── SaveDwgCommand.cs      # Salvar arquivo DWG (_.DWGOUT)
    │   │   ├── ToggleThemeCommand.cs  # Alternar tema claro/escuro (_.THEME)
    │   │   └── ExitCommand.cs         # Sair da aplicação (_.QUIT)
    │   └── Services/
    │       ├── DxfService.cs          # Serviço de importação/exportação DXF
    │       ├── DwgService.cs          # Serviço de importação/exportação DWG
    │       └── Converters/
    │           ├── IConverter.cs          # Interface base de conversor
    │           ├── EntityConverter.cs     # Base abstrata p/ conversão de entidades + ColorConverter
    │           ├── LineConverter.cs       # Line ↔ ACadSharp.Line
    │           ├── CircleConverter.cs     # Circle ↔ ACadSharp.Circle
    │           ├── ArcConverter.cs        # Arc ↔ ACadSharp.Arc (graus ↔ radianos)
    │           ├── LwPolylineConverter.cs # LwPolyline ↔ ACadSharp.LwPolyline (bidirecional)
    │           ├── LayerConverter.cs      # LayerTableRecord ↔ ACadSharp.Layer
    │           ├── VPortConverter.cs      # ViewportTableRecord ↔ ACadSharp.VPort
    │           ├── ConverterService.cs    # Registro e despacho de conversores por tipo
    │           └── TableParsers.cs        # Módulo estático de parsing de tabelas (Layers, Viewports, Entities)
    │
    ├── Themes/                        # TEMAS — Recursos de cor
    │   ├── Colors.axaml               # Definições de cores Light/Dark
    │   └── ThemeTokens.axaml          # SolidColorBrush mapeados para Theme.*
    │
    ├── MainWindow.axaml               # Layout principal da janela
    ├── MainWindow.axaml.cs            # Código-behind + redirecionamento de teclas
    ├── App.axaml                      # Configuração da aplicação Avalonia
    └── Program.cs                     # Ponto de entrada da aplicação
```

---

## Dependências

| Pacote | Versão | Licença | Uso |
| --- | --- | --- | --- |
| `Avalonia` | 12.0.4 | MIT | Framework de UI multiplataforma |
| `Avalonia.Desktop` | 12.0.4 | MIT | Suporte para Windows/Linux/macOS |
| `Avalonia.Themes.Fluent` | 12.0.4 | MIT | Tema visual padrão |
| `Avalonia.Fonts.Inter` | 12.0.4 | MIT | Fonte Inter para tipografia |
| `ACadSharp` | 3.6.29 | MIT | Leitura e escrita de arquivos DXF e DWG |

---

## Como Executar

### Pré-requisitos

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)

### Rodando o projeto

```bash
# Clone o repositório
git clone <url-do-repositorio>
cd NormalCAD

# Execute o projeto
dotnet run --project NormalCAD/NormalCAD.csproj
```

Ou, já dentro da pasta do projeto:

```bash
cd NormalCAD/NormalCAD
dotnet run
```

### Compilar sem executar

```bash
dotnet build
```

---

## Como Usar

| Ação | Como fazer |
| --- | --- |
| **Navegar (Pan)** | Arrastar com o botão do **meio** do mouse |
| **Zoom** | Scroll do mouse (focado na posição do cursor) |
| **Desenhar Linha** | Digitar `LINE` / `L` e clicar dois pontos, ou menu Draw → Line |
| **Desenhar Círculo** | Digitar `CIRCLE` / `C` / `CI` e clicar centro + raio, ou menu Draw → Circle |
| **Desenhar Polilinha** | Digitar `PLINE` / `PL` e clicar vértices; `Enter` finaliza aberta, `C` finaliza fechada, ou menu Draw → Polyline |
| **Selecionar** | Clicar na entidade; `Ctrl + Clique` acumula seleções |
| **Seleção por Janela** | Arrastar da esquerda → direita (Window) ou direita → esquerda (Crossing) |
| **Excluir Selecionados** | Tecla `Delete` ou digitar `ERASE` / `E` |
| **Cancelar / Voltar** | `Escape` (limpa prompt e volta à seleção) ou menu Edit → Select |
| **Limpar Tudo** | Digitar `CLEANALL` / `CLA` ou menu Edit → Clean All |
| **Abrir DXF** | Digitar `DXFIN` / `DXFI` ou menu File → Open → Open DXF... |
| **Abrir DWG** | Digitar `DWGIN` / `DWG` ou menu File → Open → Open DWG... |
| **Salvar DXF** | Digitar `DXFOUT` / `DXFO` ou menu File → Save → Save DXF... |
| **Salvar DWG** | Digitar `DWGOUT` / `DWGS` ou menu File → Save → Save DWG... |
| **Alternar Tema** | Digitar `THEME` / `TEMA` / `TH` ou menu → Change Theme |
| **Sair** | Digitar `QUIT` / `EXIT` / `Q` ou menu File → Exit |
| **Executar Comando** | Digitar nome/alias no prompt e pressionar `Enter` ou `Espaço` |

### Tabela de Comandos e Aliases

| Comando | Digite | Aliases |
| --- | --- | --- |
| Line | `LINE` | `L` |
| Circle | `CIRCLE` | `C`, `CI` |
| Polyline | `PLINE` | `PL` |
| Erase | `ERASE` | `E` |
| Clean All | `CLEANALL` | `CLA` |
| Open DXF | `DXFIN` | `DXFI` |
| Save DXF | `DXFOUT` | `DXFO` |
| Open DWG | `DWGIN` | `DWG` |
| Save DWG | `DWGOUT` | `DWGS` |
| Toggle Theme | `THEME` | `TEMA`, `TH` |
| Quit | `QUIT` | `EXIT`, `Q` |

---

## Branch e Fluxo de Desenvolvimento

O projeto utiliza o **GitLab Flow**:

- `main` — Branch de produção/estável.
- `feature/mvc-cad-setup` — Branch ativa da primeira fase de desenvolvimento (estrutura MVC, viewport, snapping, comandos e integração DXF).

---

## Licença

Distribuído sob a licença **MIT**. Veja o arquivo `LICENSE` para mais detalhes.
