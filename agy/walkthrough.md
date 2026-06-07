# Walkthrough - NormalCAD

Este documento apresenta um resumo detalhado do protótipo desenvolvido para o **NormalCAD**, um sistema CAD 2D simples baseado em **C#**, **Avalonia UI** e a biblioteca **netDxf**, estruturado sob a arquitetura **MVC** e seguindo a filosofia da **API do AutoCAD para .NET**.

---

## O que foi implementado

### 1. Modelo de Banco de Dados CAD (Model)
Desenvolvemos um banco de dados estruturado exatamente como o da API .NET do AutoCAD:
*   [Database](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Database.cs): Container principal do desenho contendo as tabelas de símbolos (Symbol Tables) e gerenciando transações.
*   [ObjectId](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/ObjectId.cs): Identificadores únicos para referenciar objetos no banco de dados.
*   [Symbol Tables](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/SymbolTable.cs): Tabelas que armazenam configurações e metadados. Criamos:
    *   [LayerTable](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/LayerTable.cs) e [LayerTableRecord](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/LayerTableRecord.cs): Para gerenciamento de camadas (nome, cor e visibilidade).
    *   [BlockTable](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/BlockTable.cs) e [BlockTableRecord](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/BlockTableRecord.cs): Para armazenar blocos e coleções de entidades, incluindo a modelagem padrão do **ModelSpace** (`*Model_Space`).
*   [Transaction e TransactionManager](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Transaction.cs): Sistema simplificado para abrir transações para leitura (`ForRead`) e escrita (`ForWrite`), garantindo rollback automático de adições mal-sucedidas ou abortadas.

### 2. Entidades Geométricas (Model)
*   [Point3d](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Geometry/Point3d.cs) e [Vector3d](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Geometry/Vector3d.cs): Estruturas de dados de matemática tridimensional com suporte a operadores algébricos.
*   [Entity](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Entity.cs): Classe base abstrata contendo propriedades de camada e cor.
*   [Line](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Entities/Line.cs), [Circle](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Entities/Circle.cs) e [Arc](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Core/Entities/Arc.cs): As entidades geométricas concretas do protótipo.

### 3. Viewport de Desenho e Visualização (View)
*   [CadViewport](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/View/Controls/CadViewport.cs): Um controle customizado do Avalonia UI responsável por renderizar a geometria 2D do banco de dados na tela usando primitivas vetoriais:
    *   **Navegação:** Implementa Zoom dinâmico focado na posição do cursor do mouse (scroll) e Pan infinito (arrastar com o botão do meio).
    *   **Grade Inteligente (Grid):** Exibe uma grade cartesiana cuja escala de divisão se adapta automaticamente com base no nível de zoom (ex: 1, 5, 10, 50, 100 unidades).
    *   **Eixos Cartesianos:** Eixo X desenhado em vermelho e Eixo Y desenhado em verde.
    *   **Atração Magnética (Snapping):** Detecta dinamicamente a proximidade do mouse com pontos notáveis:
        *   *Endpoint* (extremidades de linhas/arcos) - exibe uma **caixa verde**.
        *   *Midpoint* (ponto médio de linhas) - exibe um **triângulo verde**.
        *   *Center* (centro de círculos/arcos) - exibe um **círculo verde**.

### 4. Controlador e Ferramentas Interativas (Controller)
*   [CadController](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Controller/CadController.cs): Gerencia a máquina de estados das ferramentas ativas e encaminha as coordenadas convertidas de tela para coordenadas do CAD.
*   [SelectCommand](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Controller/Commands/SelectCommand.cs): Ferramenta de seleção de objetos por clique com tolerância de distância. Suporta seleção cumulativa (Ctrl pressionado) e exclusão física das entidades ao pressionar a tecla `Delete`.
*   [DrawLineCommand](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Controller/Commands/DrawLineCommand.cs): Permite desenhar linhas em cadeia (estilo AutoCAD), com previews dinâmicos tracejados durante a movimentação do mouse.
*   [DrawCircleCommand](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Controller/Commands/DrawCircleCommand.cs): Permite desenhar círculos clicando para definir o centro e arrastando para ajustar o raio.

### 5. Janela Principal e Temas (View)
*   [MainWindow](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/MainWindow.axaml): A interface gráfica completa do CAD.
    *   **Barra de Ferramentas:** Botões de Selecionar, Desenhar Linha, Desenhar Círculo, Excluir e Limpar desenho.
    *   **Barra Lateral (Tabs):**
        *   *Propriedades:* Exibe coordenadas precisas dos pontos de início/fim das linhas, centro/raio dos círculos e as respectivas camadas das entidades selecionadas em tempo real.
        *   *Camadas:* Permite criar novas camadas personalizadas com cores geradas aleatoriamente e alternar a camada ativa.
    *   **Barra de Status:** Exibe a coordenada do cursor no espaço real do CAD em tempo real, a ferramenta ativa e o status atual do Snap.
    *   **Alternância de Temas:** Permite chavear toda a interface de forma transparente entre o **Tema Escuro** (Dark Mode clássico) e o **Tema Claro** (Light Mode) dinamicamente.

### 6. Serviço de Importação/Exportação DXF (Controller/Services)
*   [DxfService](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/Controller/Services/DxfService.cs): Realiza a tradução bidirecional usando a API `netDxf`:
    *   **Importar:** Abre arquivos DXF, criando as camadas correspondentes e importando linhas, círculos, arcos e decompondo polilinhas (`LwPolyline/Polyline2D`) em segmentos de linhas simples.
    *   **Exportar:** Salva o banco de dados interno de volta para arquivos DXF (compatíveis com AutoCAD, LibreCAD, etc.) preservando geometrias e camadas.

---

## Verificação e Compilação

O projeto foi compilado utilizando o compilador da CLI do .NET SDK 9.0 com sucesso.
*   **Comando:** `dotnet build`
*   **Status:** Concluído com êxito (0 erros).
*   **Alvo de compilação:** [NormalCAD.dll](file:///c:/Users/LENOVO/OneDrive/DOCS/DEV/NormalCAD/NormalCAD/bin/Debug/net9.0/NormalCAD.dll)
