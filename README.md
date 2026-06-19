# NormalCAD

> Protótipo de sistema CAD 2D desenvolvido em **C#** com **Avalonia UI**, estruturado na arquitetura **MVC** e com suporte a leitura e escrita de arquivos **DXF/DWG** via biblioteca **ACadSharp**.

---

## Visão Geral

O **NormalCAD** é um protótipo de aplicação de desenho técnico (CAD) 2D de código aberto. Seu objetivo é demonstrar como construir um sistema CAD funcional usando tecnologias .NET modernas e abertas, sem depender de bibliotecas proprietárias como o SDK do AutoCAD.

A arquitetura interna do banco de dados do desenho foi modelada com base na **API .NET do AutoCAD** (ObjectARX/Managed), utilizando os mesmos conceitos de `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable`, `ViewportTable` e entidades (`Entity`), tornando o projeto familiar para desenvolvedores da área de AEC (Arquitetura, Engenharia e Construção).

---

## Funcionalidades (v0.1)

### 🖊️ Ferramentas de Desenho

- **Linha** — Desenho em cadeia (o final de uma linha é o início da próxima), com preview dinâmico tracejado durante o posicionamento.
- **Círculo** — Definição por clique no centro e arrastar para ajustar o raio, com preview em tempo real.
- **Arco** — Clique no centro, arraste para definir raio + ângulo inicial, clique para ângulo final. Preview dinâmico.
- **Polilinha** — Cliques sucessivos adicionam vértices; `Enter`/`Espaço` finaliza aberta, `C` finaliza fechada. Preview em tempo real.
- **Seleção** — Clique para selecionar entidades individualmente; `Ctrl + Clique` para seleção múltipla acumulativa. Arraste da esquerda→direita para Window Select ou direita→esquerda para Crossing Select. Utiliza índice espacial R*-tree para performance em desenhos grandes.
- **Exclusão** — Tecla `Delete` remove todos os objetos selecionados.
- **Limpar** — Botão para apagar todo o desenho atual.

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

- **Abrir DXF** — Importa linhas, círculos, arcos e polilinhas (`LwPolyline`).
- **Salvar DXF** — Exporta para `.dxf` compatível com AutoCAD, LibreCAD, QCAD.
- **Abrir DWG** — Suporte a R14 até 2020 (AC1014–AC1032).
- **Salvar DWG** — Exporta no formato AC1032 (AutoCAD 2018–2020).

### ⌨️ Linha de Comando Integrada

- Prompt de comandos estilo AutoCAD com aliases, histórico e auto-foco.
- Popup flutuante com animação de fade-in/fade-out para feedback.

### 🎨 Interface e Temas

- Painel de Propriedades, Gerenciador de Camadas, Barra de Status.
- Dark Mode / Light Mode dinâmico sem reinicialização.

---

## Estrutura do Projeto

```bash
NormalCAD.sln
├── NormalCAD.Core/          # Class Library — Modelo de dados (API compatível com AutoCAD .NET)
│   ├── NormalCAD.Core.csproj
│   ├── Database.cs, Entity.cs, DBObject.cs, ObjectId.cs
│   ├── BlockTable.cs, BlockTableRecord.cs
│   ├── LayerTable.cs, LayerTableRecord.cs
│   ├── ViewportTable.cs, ViewportTableRecord.cs
│   ├── SymbolTable.cs, SymbolTableRecord.cs
│   ├── Transaction.cs, TransactionManager.cs
│   ├── EntityColor.cs, OpenMode.cs, SnapType.cs, Culture.cs
│   ├── Entities/
│   │   ├── Line.cs, Circle.cs, Arc.cs, Polyline.cs
│   ├── Geometry/
│   │   ├── Point2d.cs, Point3d.cs, Vector3d.cs
│   │   ├── Matrix3d.cs, Extents3d.cs
│   └── Spatial/
│       └── RTree.cs           # Índice espacial R*-tree
│
├── NormalCAD/               # WinExe — Aplicação (Avalonia UI + comandos)
│   ├── NormalCAD.csproj       (Referencia NormalCAD.Core)
│   ├── Controller/            # Lógica de comandos e orquestração
│   │   ├── CadController.cs
│   │   ├── CmdManager.cs, InputManager.cs
│   │   ├── Commands/          # ICadCommand implementations
│   │   └── Services/          # DWG/DXF + Conversores
│   ├── View/                  # Interface Avalonia
│   │   ├── Controls/          # CadViewport, BottomBar, MenuBar, paletas
│   │   └── Drawing/           # Renderers (Line, Circle, Arc, Polyline)
│   ├── MainWindow.axaml, App.axaml, Program.cs
│   └── Themes/
│
└── NormalCAD.Tests/          # Testes unitários (xUnit)
    ├── NormalCAD.Tests.csproj  (Referencia NormalCAD.Core)
    └── Core/Spatial/
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

### Compilar projetos individuais

```bash
dotnet build NormalCAD.Core/NormalCAD.Core.csproj     # Apenas o modelo de dados
dotnet build NormalCAD/NormalCAD.csproj               # Apenas a aplicação
dotnet build NormalCAD.Tests/NormalCAD.Tests.csproj   # Apenas os testes
```

---

## Como Usar

| Ação | Como fazer |
| --- | --- |
| **Navegar (Pan)** | Arrastar com o botão do **meio** do mouse |
| **Zoom** | Scroll do mouse (focado na posição do cursor) |
| **Desenhar Linha** | Digitar `LINE` / `L` e clicar dois pontos, ou menu Draw → Line |
| **Desenhar Círculo** | Digitar `CIRCLE` / `C` / `CI` e clicar centro + raio, ou menu Draw → Circle |
| **Desenhar Arco** | Digitar `ARC` / `A` e clicar centro → raio → ângulo final, ou menu Draw → Arc |
| **Desenhar Polilinha** | Digitar `PLINE` / `PL` e clicar vértices; `Enter` finaliza aberta, `C` finaliza fechada |
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
