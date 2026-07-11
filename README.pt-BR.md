# NormalCAD (Português)

> Protótipo de sistema CAD 2D desenvolvido em **C#** com **Avalonia UI**, estruturado na arquitetura **MVC** e com suporte a leitura e escrita de arquivos **DXF/DWG** via biblioteca **ACadSharp**.

---

## Visão Geral

O **NormalCAD** é um protótipo de aplicação de desenho técnico (CAD) 2D de código aberto. Seu objetivo é demonstrar como construir um sistema CAD funcional usando tecnologias .NET modernas e abertas, sem depender de bibliotecas proprietárias como o SDK do AutoCAD.

A arquitetura interna foi modelada com base na **API .NET do AutoCAD** (ObjectARX/Managed), utilizando os mesmos conceitos de `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable`, `ViewportTable` e entidades (`Entity`, `Curve`), tornando o projeto familiar para desenvolvedores da área de AEC (Arquitetura, Engenharia e Construção). Veja [ARCHITECTURE.md](docs/ARCHITECTURE.md) para a referência completa da API de entidades, mapeamento de namespaces e estrutura do projeto.

---

## Funcionalidades

### Ferramentas de Desenho

- **Linha** — Desenho em cadeia com preview dinâmico tracejado durante o posicionamento.
- **Círculo** — Centro + raio, com alternância `Radius`/`Diameter` via keyword no prompt.
- **Arco** — Centro + raio + ângulo inicial/final, com preview dinâmico.
- **Polilinha** — Cliques sucessivos adicionam vértices; keywords `Undo` e `Close` com prefix matching.
- **Seleção** — Clique para adicionar, `Shift + Clique` para remover. Arraste esquerda→direita para Window Select, direita→esquerda para Crossing Select. Utiliza índice espacial R*-tree para performance em desenhos grandes.
- **Exclusão** — Tecla `Delete` remove todos os objetos selecionados.
- **Limpar** — Apaga todo o desenho atual.

### Linha de Comando Integrada (estilo AutoCAD)

- **Prompt de Comandos** — Digite o nome ou alias de um comando e pressione `Enter`/`Espaço` para executar. `Escape` cancela o comando ativo.
- **Sistema de Keywords** — Comandos registram opções no prompt (ex: `[Undo/Close]`, `[Diameter/Radius]`). Prefix matching (digite `U` para Undo); prefixos ambíguos são rejeitados.
- **Prompt Dinâmico** — Formato AutoCAD: `"PLINE Specify next point or [Undo/Close]:"`. Prefixo atualiza conforme o comando ativo. Feedback de seleção: `"1 found, 3 total"`.
- **Aliases** — Descobertos automaticamente via reflection (ex: `C`/`CI` para `CIRCLE`).
- **Popup Flutuante** — Mensagens de feedback do sistema aparecem acima da barra de comando com fade-out.

### Object Snapping (Atração Magnética)

- **Endpoint** — Extremidades de linhas, arcos e vértices de polilinhas (caixa verde).
- **Midpoint** — Ponto médio de linhas e segmentos de polilinhas (triângulo verde).
- **Center** — Centro de círculos e arcos (círculo verde).

### Viewport e Navegação

- **Zoom** — Scroll do mouse com foco na posição do cursor.
- **Pan** — Arrastar com o botão do meio do mouse.
- **Grade Adaptativa** — Grade cartesiana que se adapta automaticamente ao nível de zoom.
- **Persistência de Viewport** — Posição da vista salva no registro `*ACTIVE` da `ViewportTable`.

### Importação / Exportação DXF e DWG

- **Abrir (OPEN)** — Abre arquivos DXF/DWG. Detecta o formato pela extensão e importa linhas, círculos, arcos, polilinhas (`LwPolyline`) e inserções de bloco (`Insert` → `BlockReference`).
- **Salvar (SAVE)** — Salva no caminho atual do documento. Documentos novos (sem caminho) comportam-se como SAVEAS.
- **Salvar Como (SAVEAS)** — Exporta no formato compatível com AutoCAD, LibreCAD, QCAD (DXF ou DWG conforme extensão escolhida), preservando propriedades de entidade (layer, cor, linetype, lineweight, transparência).

### Interface

- **Painel de Propriedades** — Exibe e edita propriedades das entidades selecionadas. Suporta merge multi-seleção (mostra `*VARIES*` quando valores diferem) e categorias Geometry/Misc específicas por entidade. Labels de categorias e propriedades vêm de arquivos `.resx` preparados para localização.
- **Gerenciador de Camadas** — Cria e gerencia layers. Entidades herdam propriedades da layer.
- **Barra de Status** — Exibe coordenadas do cursor em tempo real.
- **Dark Mode / Light Mode** — Alterna tema em tempo de execução sem reiniciar (comando `THEME`).
- **Localização** — Interface em Inglês e Português (pt-BR) com troca de idioma em tempo de execução (comando `LANGUAGE` / Base → Change Language). As strings vêm de recursos `.resx` e são re-localizadas ao vivo na troca. O idioma e o tema escolhidos persistem entre sessões em `%APPDATA%/NormalCAD/config.json`.

---

## Estrutura do Projeto

```bash
NormalCAD.sln
├── docs/                    # Backlog, guia de contribuição, documentação de arquitetura
├── NormalCAD.Core/          # Class Library — modelo de dados, zero dependências
│   ├── ApplicationServices/ # Application, Document, DocumentCollection
│   ├── DatabaseServices/    # Entity, Curve, Line, Circle, Arc, Polyline, Database, Transaction, tabelas
│   ├── EditorInput/         # Editor e tipos de prompt result
│   ├── Geometry/            # Point3d, Vector3d, Matrix3d, primitivas Curve3d
│   └── Spatial/             # Índice espacial R*-tree
├── NormalCAD/               # Aplicação Avalonia UI
│   ├── Controller/          # CadController, CmdManager, InputManager, Commands, Providers, Converters
│   ├── Resources/           # Arquivos .resx de localização e helpers ResourceManager
│   ├── View/                # Controles de UI (viewport, paletas, menus) e renderers de entidade
│   ├── Host/                # Implementação do host da aplicação
│   └── Utilities/           # Helpers transversais (AngleConverter, etc.)
└── NormalCAD.Tests/         # Testes unitários xUnit (geometria, database, spatial)
```

`NormalCAD.Core` é uma **Class Library pura** (sem dependência de UI ou ACadSharp), espelhando a separação `AcDbMgd.dll` / `AcMgd.dll` do AutoCAD. Para a árvore completa arquivo por arquivo, veja [ARCHITECTURE.md](docs/ARCHITECTURE.md).

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
git clone https://github.com/CarthoCAD/NormalCAD.git
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

| Ação | Comando | Aliases | Menu |
| --- | --- | --- | --- |
| Desenhar Linha | `LINE` | `L` | Draw → Line |
| Desenhar Círculo | `CIRCLE` | `C`, `CI` | Draw → Circle |
| Desenhar Arco | `ARC` | `A` | Draw → Arc |
| Desenhar Polilinha | `PLINE` | `PL` | Draw → Polyline |
| Excluir Selecionados | `ERASE` | `E` | Edit → Erase |
| Limpar Tudo | `CLEANALL` | `CLA` | Edit → Clean All |
| Abrir Arquivo | `OPEN` | — | File → Open... |
| Salvar | `SAVE` | — | File → Save |
| Salvar Como | `SAVEAS` | — | File → Save As... |
| Alternar Tema | `THEME` | `TH` | Base → Change Theme |
| Alternar Idioma | `LANGUAGE` | `LANG` | Base → Change Language |
| Sair | `QUIT` | `EXIT`, `Q` | File → Exit |
| **Navegar (Pan)** | Arrastar com o botão do meio do mouse | | |
| **Zoom** | Scroll do mouse (foco na posição do cursor) | | |
| **Selecionar** | Clique para adicionar, `Shift + Clique` para remover | | |
| **Seleção por Janela** | Arrastar esquerda→direita (Window) ou direita→esquerda (Crossing) | | |
| **Cancelar** | `Escape` | | Edit → Select |

---

## Branch e Fluxo de Desenvolvimento

O projeto utiliza o **Trunk-Based Flow**:

- `main` — Branch principal de integração e releases.
- `feat/*` — Novas funcionalidades.
- `fix/*` — Correções de bugs.

Veja [CONTRIBUTING.md](docs/CONTRIBUTING.md) para convenção de commits e workflow detalhado.

---

## Licença

Distribuído sob a licença **MIT**. Veja o arquivo `LICENSE` para mais detalhes.
