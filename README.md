# NormalCAD

> Protótipo de sistema CAD 2D simples desenvolvido em **C#** com **Avalonia UI**, estruturado na arquitetura **MVC** e com suporte a leitura e escrita de arquivos **DXF** via biblioteca **netDxf**.

---

## Visão Geral

O **NormalCAD** é um protótipo de aplicação de desenho técnico (CAD) 2D de código aberto. Seu objetivo é demonstrar como construir um sistema CAD funcional usando tecnologias .NET modernas e abertas, sem depender de bibliotecas proprietárias como o SDK do AutoCAD.

A arquitetura interna do banco de dados do desenho foi modelada com base na **API .NET do AutoCAD** (ObjectARX/Managed), utilizando os mesmos conceitos de `Database`, `ObjectId`, `Transaction`, `BlockTable`, `LayerTable` e entidades (`Entity`), tornando o projeto familiar para desenvolvedores da área de AEC (Arquitetura, Engenharia e Construção).

---

## Funcionalidades (v0.1)

### 🖊️ Ferramentas de Desenho

- **Linha** — Desenho em cadeia (o final de uma linha é o início da próxima), com preview dinâmico tracejado durante o posicionamento.
- **Círculo** — Definição por clique no centro e arrastar para ajustar o raio, com preview em tempo real.
- **Seleção** — Clique para selecionar entidades individualmente; `Ctrl + Clique` para seleção múltipla acumulativa.
- **Exclusão** — Tecla `Delete` remove todos os objetos selecionados.
- **Limpar** — Botão para apagar todo o desenho atual.

### 🧲 Object Snapping (Atração Magnética)

Atração automática do cursor a pontos notáveis de entidades existentes ao aproximar o mouse:

- **Endpoint** — Extremidades de linhas e arcos (indicador: **caixa verde**).
- **Midpoint** — Ponto médio de linhas (indicador: **triângulo verde**).
- **Center** — Centro de círculos e arcos (indicador: **círculo verde**).

### 🗺️ Viewport e Navegação

- **Zoom** — Scroll do mouse com foco na posição do cursor.
- **Pan** — Arrastar com o botão do meio do mouse.
- **Grade Adaptativa** — Grade cartesiana que se adapta automaticamente ao nível de zoom (ex: escala de 1, 5, 10, 50, 100 unidades).
- **Eixos Cartesianos** — Eixo X em vermelho e Eixo Y em verde visíveis quando próximos da tela.

### 📂 Importação / Exportação DXF

Usando a biblioteca **netDxf** (versão 3.0.1, licença LGPL):

- **Abrir DXF** — Importa linhas, círculos, arcos e polilinhas (`Polyline2D`). As polilinhas são decompostas automaticamente em segmentos de linhas simples. As camadas do arquivo DXF são recriadas no banco de dados interno.
- **Salvar DXF** — Exporta o desenho atual para um arquivo `.dxf` compatível com AutoCAD, LibreCAD, QCAD e outros softwares CAD.

### 🎨 Interface e Temas

- **Painel de Propriedades** — Exibe as coordenadas e a camada da entidade selecionada em tempo real.
- **Gerenciador de Camadas** — Cria e ativa camadas personalizadas com cores únicas.
- **Barra de Status** — Coordenadas do cursor em espaço de mundo real e status do snap ativo.
- **Alternância de Tema** — Suporte dinâmico a **Dark Mode** e **Light Mode** sem reinicialização.

---

## Arquitetura (MVC)

O projeto segue rigorosamente o padrão MVC e é organizado nos seguintes pacotes:

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
    │   ├── Transaction.cs             # Transação de modificação no DB
    │   ├── TransactionManager.cs      # Gerenciador da pilha de transações
    │   └── Geometry/
    │       ├── Point3d.cs             # Ponto tridimensional
    │       └── Vector3d.cs            # Vetor tridimensional
    │
    ├── View/                          # VIEW — Interface Avalonia UI
    │   └── Controls/
    │       └── CadViewport.cs         # Controle de renderização e navegação
    │
    ├── Controller/                    # CONTROLLER — Lógica e comandos
    │   ├── CadController.cs           # Orquestrador central do MVC
    │   ├── Commands/
    │   │   ├── ICadCommand.cs         # Interface para ferramentas de desenho
    │   │   ├── SelectCommand.cs       # Ferramenta de seleção
    │   │   ├── DrawLineCommand.cs     # Ferramenta de linha
    │   │   └── DrawCircleCommand.cs   # Ferramenta de círculo
    │   └── Services/
    │       └── DxfService.cs          # Importação e exportação de DXF
    │
    ├── MainWindow.axaml               # Layout principal da janela
    ├── MainWindow.axaml.cs            # Código-behind da janela principal
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
| `netDxf.netstandard` | 3.0.1 | LGPL-3.0 | Leitura e escrita de arquivos DXF |

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
| **Desenhar Linha** | Clicar em "Linha" e clicar dois pontos no viewport |
| **Desenhar Círculo** | Clicar em "Círculo", clicar no centro, arrastar e clicar |
| **Selecionar** | Clicar em "Seleção" e clicar em uma entidade |
| **Seleção Múltipla** | `Ctrl + Clique` para acumular seleções |
| **Excluir Selecionados** | Tecla `Delete` |
| **Cancelar Ferramenta** | Tecla `Escape` |
| **Abrir DXF** | Botão "Abrir DXF" no menu superior |
| **Salvar DXF** | Botão "Salvar DXF" no menu superior |
| **Alternar Tema** | Botão "Alternar Tema" no menu superior |

---

## Branch e Fluxo de Desenvolvimento

O projeto utiliza o **GitLab Flow**:

- `main` — Branch de produção/estável.
- `feature/mvc-cad-setup` — Branch ativa da primeira fase de desenvolvimento (estrutura MVC, viewport, snapping, comandos e integração DXF).

---

## Licença

Distribuído sob a licença **MIT**. Veja o arquivo `LICENSE` para mais detalhes.
