# Plano de Implementação - Protótipo CAD 2D (NormalCAD)

Este documento detalha o planejamento para a criação de um protótipo de sistema CAD 2D simples usando **C#**, **Avalonia UI** e a biblioteca **netDxf** (ou alternativamente **ixMilia.Dxf**), estruturado de acordo com o padrão **MVC (Model-View-Controller)** e seguindo a arquitetura conceitual da **API do AutoCAD para .NET**.

---

## User Review Required

> [!IMPORTANT]
> **Escolha da Biblioteca DXF:** Proponho o uso da `netDxf.netstandard` devido à sua excelente compatibilidade com versões modernas do DXF (até 2018) e facilidade de manipulação de entidades e camadas. Ela está sob licença LGPL. Se preferir uma licença mais livre (MIT), podemos usar a `ixMilia.Dxf`.
> 
> **Padrão de Transações:** A API do AutoCAD utiliza transações para operações no banco de dados (`Database`). Para o protótipo, implementaremos uma versão simplificada de `Transaction` e `TransactionManager` que facilitará a criação segura de entidades e, futuramente, suporte nativo a Desfazer/Refazer (Undo/Redo).

---

## Open Questions

> [!NOTE]
> 1. **Qual biblioteca DXF prefere usar?** A recomendação é `netDxf` (LGPL, mais robusta) ou prefere a `ixMilia.Dxf` (MIT, mais simples)?
> 2. **Deseja suporte a Snapping (End/Mid/Center) já nesta primeira fase?** Ou focamos apenas na visualização, zoom, pan e desenho básico?
> 3. **Interface Inicial:** Devemos seguir uma estética escura clássica (Dark Mode) de sistemas CAD modernos?

---

## Proposed Changes

O projeto será criado como uma solução dotnet com um aplicativo de console/desktop usando Avalonia UI 11+. A estrutura de arquivos será dividida em:

```
NormalCAD/
├── NormalCAD.sln
└── NormalCAD/
    ├── NormalCAD.csproj
    ├── App.axaml
    ├── App.axaml.cs
    ├── Program.cs
    ├── Core/                <-- Estrutura similar ao AutoCAD .NET API
    │   ├── Database.cs
    │   ├── ObjectId.cs
    │   ├── DBObject.cs
    │   ├── Entity.cs
    │   ├── SymbolTable.cs
    │   ├── BlockTable.cs
    │   ├── BlockTableRecord.cs
    │   ├── LayerTable.cs
    │   ├── LayerTableRecord.cs
    │   ├── Transaction.cs
    │   ├── TransactionManager.cs
    │   └── Geometry/        <-- Tipos auxiliares (Point3d, Vector3d)
    ├── Model/               <-- Modelos de negócio específicos da aplicação
    │   └── CadModel.cs      <-- Agrupa o Database ativo e estado geral
    ├── View/                <-- Views do Avalonia UI
    │   ├── MainWindow.axaml
    │   ├── MainWindow.axaml.cs
    │   └── Controls/
    │       └── CadViewport.cs  <-- Custom Control para renderização SkiaSharp e mouse events
    └── Controller/          <-- Controladores de fluxo e comandos
        ├── CadController.cs <-- Centraliza input da view e atualiza o Model
        ├── Commands/        <-- Estados/Comandos de desenho interativos
        │   ├── ICadCommand.cs
        │   ├── DrawLineCommand.cs
        │   └── SelectCommand.cs
        └── Services/        <-- Tradução de/para DXF
            └── DxfService.cs
```

### [Fase 1: Estrutura Base e Viewport com Zoom/Pan]

Esta fase focará na inicialização do projeto, criação do banco de dados estilo AutoCAD, e no viewport interativo básico (desenhar linhas programaticamente e navegar nelas).

#### [NEW] [Database.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Database.cs)
Representa o banco de dados do desenho CAD, contendo as tabelas de símbolos (`BlockTable`, `LayerTable`) e gerenciando IDs e transações.

#### [NEW] [DBObject.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/DBObject.cs)
Classe base para todos os objetos persistidos no banco de dados CAD, incluindo IDs exclusivos (`ObjectId`).

#### [NEW] [Entity.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Entity.cs)
Classe base abstrata para objetos geométricos visíveis, que expõe propriedades como `Color`, `Layer` e `Linetype`.

#### [NEW] [Line.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Geometry/Line.cs)
Entidade de linha derivada de `Entity`, possuindo `StartPoint` e `EndPoint` (usando `Point3d`).

#### [NEW] [Transaction.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Transaction.cs)
Implementação simplificada do padrão de transações do AutoCAD para gerenciar adições e edições seguras na base de dados.

#### [NEW] [CadViewport.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/View/Controls/CadViewport.cs)
Controle customizado do Avalonia UI responsável por:
- Desenhar as entidades usando primitivas 2D (SkiaSharp) de forma otimizada.
- Controlar a matriz de visualização de Zoom e Pan baseada no mouse.
- Notificar eventos brutos do mouse para o Controller.

#### [NEW] [CadController.cs](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Controller/CadController.cs)
O cérebro do MVC. Ele recebe as interações físicas do `CadViewport` e decide quais modificações devem ser aplicadas no `Database`, além de gerenciar ferramentas de desenho ativas.

---

## Verification Plan

### Automated Tests
- Testar a integridade das transações do banco de dados (inserir entidades e verificar se foram adicionadas corretamente no commit).
- Testar a correta conversão de coordenadas mundo/tela no viewport.

### Manual Verification
- Compilar e rodar a aplicação para verificar se a janela abre corretamente no Avalonia UI.
- Testar o Pan (botão do meio arrastando) e Zoom (scroll do mouse) sobre um conjunto de linhas pré-definidas.
- Validar se a escala de zoom se mantém correta ao redor da posição do cursor.
