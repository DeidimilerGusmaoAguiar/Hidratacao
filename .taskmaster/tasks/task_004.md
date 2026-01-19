# Task ID: 4

**Title:** Histórico diário e exportação CSV

**Status:** pending

**Dependencies:** 3

**Priority:** medium

**Description:** Exibe histórico dos últimos dias e exporta CSV de totais e eventos para análise.

**Details:**

Implementar consulta de histórico diário (últimos N dias) a partir de daily_summary.json ou agregação dos eventos e comandos `history` e `export`. O history deve mostrar data, total em ml, status (completo/incompleto) e progresso. O export deve gerar CSV em conformidade com RFC 4180, com opção de salvar em arquivo. Usar arquivos JSON locais como fonte de dados (settings.json, water_events.json, daily_summary.json).

**Test Strategy:**

Testar consultas de histórico com vários dias, exportação CSV e escrita em arquivo. Validar formato e conteúdo.

## Subtasks

### 4.1. Consulta de histórico diário no JSON

**Status:** pending  
**Dependencies:** None  

Criar leitura/agrupamento para recuperar totais diários (últimos N dias).

**Details:**

Implementar WaterHistoryRepository lendo daily_summary.json ou agregando water_events.json e retornando Date, TotalMl, IsCurrentDay.

### 4.2. Formatador CSV para totais diários (RFC 4180)

**Status:** pending  
**Dependencies:** None  

Gerar CSV com cabeçalho e escaping correto, datas ISO 8601.

**Details:**

Implementar CsvExportFormatter com escape de aspas e campos com vírgula/quebras de linha. Incluir cabeçalho e status do dia.

### 4.3. Escrita do CSV em arquivo com local configurável

**Status:** pending  
**Dependencies:** None  

Salvar CSV em arquivo com nome timestamped e diretório configurável.

**Details:**

Implementar serviço de exportação que cria diretórios se necessário e escreve arquivo UTF-8. Retornar caminho salvo.

### 4.4. Comando CLI para exibir histórico

**Status:** pending  
**Dependencies:** None  

Exibir tabela no console com totais diários e progresso.

**Details:**

Implementar HistoryCommand e formatador de tabela com colunas de data, total, status e progresso (%).
