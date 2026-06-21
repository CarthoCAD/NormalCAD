# NormalCAD

> Protótipo de sistema CAD 2D desenvolvido em **C#** com **Avalonia UI**, estruturado na arquitetura **MVC** e com suporte a leitura e escrita de arquivos **DXF/DWG** via biblioteca **ACadSharp**.

---

## Visão Geral

O **NormalCAD** é um protótipo de aplicação de desenho técnico (CAD) 2D de código aberto. Seu objetivo é demonstrar como construir um sistema CAD funcional usando tecnologias .NET modernas e abertas, sem depender de bibliotecas proprietárias como o SDK do AutoCAD.

A arquitetura interna do banco de dados do desenho foi modelada com base na **API .NET do AutoCAD** (ObjectARX/Managed), utilizando os mesmos conceitos de `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable`, `ViewportTable` e entidades (`Entity`), tornando o projeto familiar para desenvolvedores da área de AEC (Arquitetura, Engenharia e Construção). A estrutura de namespaces também espelha a API do AutoCAD: `NormalCAD.Core.ApplicationServices`, `NormalCAD.Core.DatabaseServices`, `NormalCAD.Core.EditorInput` e `NormalCAD.Core.Geometry`.

### API da Entidade

A classe `Entity` implementa as propriedades e métodos da API .NET do AutoCAD:

**Propriedades:** `Layer`, `LayerId`, `Color`, `Linetype`, `LinetypeId`, `LineWeight`, `LinetypeScale`, `Transparency`, `Visible`, `BlockId`, `BlockTransform`, `Bounds`, `GeometricExtents`

**Métodos:** `Clone()`, `GetTransformedCopy()`, `IntersectWith()`, `GetDistanceTo()`, `GetGripPoints()`, `MoveGripPointsAt()`, `GetStretchPoints()`, `MoveStretchPointsAt()`, `GetOsnapPoints()`, `Highlight()` / `Unhighlight()`, `Erase()`, `Draw()`, `List()`, `SetDatabaseDefaults()`

A classe `Curve` adiciona: `Length`, `Closed`, `Area`, `StartPoint`, `EndPoint`, `GetPointAtDist()`, `GetDistAtPoint()`, `GetClosestPointTo()`, `GetFirstDerivative()`, `GetPointAtParameter()`, `GetParameterAtPoint()`

A interseção e distância são delegadas às primitivas geométricas (`Curve3d` → `LineSegment3d` / `CircularArc3d` / `CompositeCurve3d`) via `GetGeometricCurve()`.

---

## Funcionalidades (v0.1)

### 🖊️ Ferramentas de Desenho

- **Linha** — Desenho em cadeia (o final de uma linha é o início da próxima), com preview dinâmico tracejado durante o posicionamento.
- **Círculo** — Clique no centro, arraste para definir raio. Suporte a alternância `Radius`/`Diameter` via keyword no prompt.
- **Arco** — Clique no centro, arraste para definir raio + ângulo inicial, clique para ângulo final. Preview dinâmico.
- **Polilinha** — Cliques sucessivos adicionam vértices; `Enter`/`Espaço` finaliza aberta. Keywords `Undo` (remove último vértice) e `Close` (fecha polilinha) no prompt, com prefix matching (ex: digitar "U" para Undo). Preview atualiza automaticamente ao usar keywords.
- **Seleção** — Clique para adicionar entidades individualmente à seleção; `Shift + Clique` para remover. Arraste da esquerda→direita para Window Select ou direita→esquerda para Crossing Select. Utiliza índice espacial R*-tree para performance em desenhos grandes.
- **Exclusão** — Tecla `Delete` remove todos os objetos selecionados.
- **Limpar** — Botão para apagar todo o desenho atual.

### ⌨️ Linha de Comando Integrada (estilo AutoCAD)

- **Prompt de Comandos** — `TextBox` na barra inferior: digite o nome ou alias de um comando e pressione `Enter` ou `Espaço` para executá-lo. `Escape` cancela o comando ativo.
- **Sistema de Keywords** — Comandos podem registrar opções no prompt (ex: `[Undo/Close]`, `[Diameter/Radius]`). O usuário digita a keyword completa ou apenas o prefixo (`U` → Undo). Se houver ambiguidade (duas keywords com o mesmo prefixo), o sistema rejeita e informa. Quando keywords estão ativas, o prompt bloqueia execução de novos comandos.
- **Prompt Dinâmico** — Formato AutoCAD: `"PLINE Specify next point or [Undo/Close]:"`. Prefixo do prompt (`CMD:`, `CIRCLE:`, etc.) atualiza conforme o comando ativo. Durante seleção por janela, mostra `"CMD Specify opposite corner:"`. Feedback de seleção exibe `"1 found, 3 total"` ou `"2 removed, 1 total"`.
- **Aliases** — Cada comando registra automaticamente seus aliases (ex: `C` ou `CI` para `CIRCLE`). O `CmdManager` resolve aliases via descoberta por reflection.
- **Popup Flutuante** — Mensagens de feedback do sistema aparecem acima da barra de comando com fade-out.

### 🧲 Object Snapping (Atração Magnética)

- **Endpoint** — Extremidades de linhas, arcos e vértices de polilinhas (indicador: **caixa verde**).
- **Midpoint** — Ponto médio de linhas e segmentos de polilinhas (indicador: **triângulo verde**).
- **Center** — Centro de círculos e arcos (indicador: **círculo verde**).

### 🗺️ Viewport e Navegação

- **Zoom** — Scroll do mouse com foco na posição do cursor.
- **Pan** — Arrastar com o botão do meio do mouse.
- **Grade Adaptativa** — Grade cartesiana que se adapta automaticamente ao nível de zoom.
- **Persistência de Viewport** — Posição da vista salva no registro `*ACTIVE` da `ViewportTable`.

### 📂 Importação / Exportação DXF e DWG

- **Abrir DXF/DWG** — Importa linhas, círculos, arcos, polilinhas (`LwPolyline`) e inserções de bloco (`Insert` → `BlockReference`). Cria novo documento via `Application.DocumentManager`.
- **Salvar DXF/DWG** — Exporta no formato compatível com AutoCAD, LibreCAD, QCAD, preservando propriedades de entidade (layer, cor, linetype, lineweight, transparência).

### 🎨 Interface e Temas

- Painel de Propriedades, Gerenciador de Camadas, Barra de Status.
- Dark Mode / Light Mode dinâmico sem reinicialização.

---

## Estrutura do Projeto

```bash
NormalCAD.sln
├── NormalCAD.Core/              # Class Library — Modelo de dados (zero dependências)
│   ├── NormalCAD.Core.csproj
│   ├── ApplicationServices/     # Application, Document, DocumentCollection, DocumentLock
│   │   ├── Application.cs       # Facade estático (singleton), Application.Host
│   │   ├── Document.cs          # Database + Editor + LockDocument()
│   │   ├── DocumentCollection.cs # MdiActiveDocument
│   │   └── DocumentLock.cs      # IDisposable, Monitor.Enter/Exit
│   ├── DatabaseServices/        # Tipos auxiliares de banco
│   │   ├── Intersect.cs         # Enum Intersect (OnBothOperands, ExtendThis, ExtendArgument, ExtendBoth)
│   │   ├── LineWeight.cs        # Enum LineWeight (ByLayer, ByBlock, Default, W0..W211)
│   │   └── Transparency.cs      # Struct Transparency (ByLayer / alpha 0-255)
│   ├── EditorInput/             # Editor, PromptPointResult, PromptPointOptions, PromptStatus
│   │   ├── Editor.cs            # GetPoint(string), GetPoint(PromptPointOptions) — casca temporária
│   │   └── PromptResult.cs      # PromptStatus (OK/Cancel/Keyword/Error)
│   ├── Entities/                # Entidades concretas
│   │   ├── Line.cs, Circle.cs, Arc.cs, Polyline.cs
│   │   └── BlockReference.cs    # Inserção de bloco (Position, Rotation, ScaleFactors)
│   ├── Geometry/                # Primitivas geométricas e matemática
│   │   ├── Point2d.cs, Point3d.cs, Vector3d.cs, Matrix3d.cs, Extents3d.cs, Point3dCollection.cs
│   │   ├── Curve3d.cs           # Classe abstrata — base para curvas geométricas
│   │   ├── LineSegment3d.cs     # Segmento de reta (P0→P1)
│   │   ├── CircularArc3d.cs     # Arco circular / círculo completo
│   │   └── CompositeCurve3d.cs  # Curva composta (itera segmentos)
│   ├── Spatial/                 # RTree (índice espacial R*-tree)
│   ├── DBObject.cs              # Base de todos os objetos do banco
│   ├── Entity.cs                # Base de todas as entidades (Layer, Color, Linetype, LineWeight, etc.)
│   ├── Curve.cs                 # Base de entidades curvas (Length, GetPointAtDist, GetClosestPointTo, etc.)
│   ├── Database.cs, ObjectId.cs, Transaction.cs, TransactionManager.cs
│   ├── BlockTable.cs, BlockTableRecord.cs
│   ├── LayerTable.cs, LayerTableRecord.cs
│   ├── ViewportTable.cs, ViewportTableRecord.cs
│   ├── SymbolTable.cs, SymbolTableRecord.cs
│   ├── EntityColor.cs, SnapType.cs, OpenMode.cs, Culture.cs
│   └── IApplicationHost.cs      # Interface internal de inicialização do host
│
├── NormalCAD/                   # WinExe — Aplicação (Avalonia UI + comandos)
│   ├── NormalCAD.csproj          (Referencia NormalCAD.Core)
│   ├── Host/                    # Implementação do host
│   │   └── ApplicationHost.cs   # IApplicationHost, cria documentos
│   ├── Controller/              # Lógica de comandos e orquestração
│   │   ├── CadController.cs     # Orquestrador central (inicializa Application, gerencia Document)
│   │   ├── CmdManager.cs        # Descoberta, registro e despacho de comandos
│   │   ├── InputManager.cs      # Input + prompt keywords + prefix matching
│   │   ├── Commands/            # ICadCommand implementations
│   │   └── Services/Converters/ # Conversores NormalCAD ↔ ACadSharp
│   ├── View/                    # Interface Avalonia
│   │   ├── Controls/            # CadViewport, BottomBar, MenuBar, paletas
│   │   └── Drawing/             # Renderers (Line, Circle, Arc, Polyline)
│   ├── MainWindow.axaml, App.axaml, Program.cs
│   └── Themes/
│
└── NormalCAD.Tests/             # Testes unitários (xUnit)
    ├── NormalCAD.Tests.csproj    (Referencia NormalCAD.Core)
    └── Core/
        ├── Geometry/
        │   ├── LineSegment3dTests.cs
        │   ├── CircularArc3dTests.cs
        │   └── CompositeCurve3dTests.cs
        ├── DatabaseServices/
        │   └── PolylineTests.cs
        └── Spatial/
            └── RTreeTests.cs
```

`NormalCAD.Core` é uma **Class Library pura** (sem dependência de UI ou ACadSharp). Isso permite que plugins e extensões referenciem apenas o modelo de dados, espelhando a separação `AcDbMgd.dll` / `AcMgd.dll` do AutoCAD.

---

## Dependências

| Pacote | Projeto | Licença | Uso |
| --- | --- | --- | --- |
| `Avalonia` 12.0.4 | NormalCAD | MIT | Framework de UI multiplataforma |
| `Avalonia.Desktop` 12.0.4 | NormalCAD | MIT | Suporte Windows/Linux/macOS |
| `Avalonia.Themes.Fluent` 12.0.4 | NormalCAD | MIT | Tema visual |
| `ACadSharp` 3.6.29 | NormalCAD | MIT | Leitura/escrita DXF e DWG |
| `xUnit` 2.9.3 | NormalCAD.Tests | Apache 2.0 | Framework de testes |
| `NormalCAD.Core` | — | MIT | **Sem dependências externas** |

---

## Como Executar

### Pré-requisitos

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)

### Clonar e rodar

```bash
git clone https://github.com/seu-usuario/NormalCAD.git
cd NormalCAD

# Restaurar dependências
dotnet restore

# Compilar toda a solução
dotnet build

# Executar a aplicação
dotnet run --project NormalCAD/NormalCAD.csproj
```

### Executar testes

```bash
dotnet test NormalCAD.Tests/NormalCAD.Tests.csproj
```

---

## Como Usar

| Ação | Como fazer |
| --- | --- |
| **Navegar (Pan)** | Arrastar com o botão do **meio** do mouse |
| **Zoom** | Scroll do mouse (focado na posição do cursor) |
| **Desenhar Linha** | Digitar `LINE` / `L` e clicar dois pontos, ou menu Draw → Line |
| **Desenhar Círculo** | Digitar `CIRCLE` / `C` / `CI` — clique no centro; digite `D` + Enter para Diameter, `R` + Enter para Radius; clique para definir raio/diâmetro |
| **Desenhar Arco** | Digitar `ARC` / `A` e clicar centro → raio → ângulo final, ou menu Draw → Arc |
| **Desenhar Polilinha** | Digitar `PLINE` / `PL` e clicar vértices; `Enter` finaliza aberta; keywords: `U` (Undo), `C` (Close via prompt) |
| **Selecionar** | Clicar na entidade para adicionar à seleção; `Shift + Clique` para remover |
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

### Comandos e Aliases

| Comando | Digite | Aliases |
| --- | --- | --- |
| Line | `LINE` | `L` |
| Circle | `CIRCLE` | `C`, `CI` |
| Arc | `ARC` | `A` |
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
- `feature/mvc-cad-setup` — Branch ativa da primeira fase de desenvolvimento.

---

## Licença

Distribuído sob a licença **MIT**. Veja o arquivo `LICENSE` para mais detalhes.
