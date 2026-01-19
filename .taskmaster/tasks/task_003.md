# Task ID: 3

**Title:** Registro de ingestão com log flexível e desfazer

**Status:** pending

**Dependencies:** 1 ⧖

**Priority:** high

**Description:** Permite registrar água em ml ou pelo copo padrão e desfazer o último registro do dia.

**Details:**

Implementar comandos de log: `log <ml>` e `log --cup` (usa tamanho padrão). Registrar eventos com timestamp UTC no arquivo water_events.json (lista de eventos). Atualizar total diário e disponibilizar comando para desfazer o último registro do dia. Validar valores de entrada e manter consistência dos totais.

**Test Strategy:**

Testar logs com valores válidos/invalidos, uso do copo padrão, e desfazer último evento. Verificar totais diários após operações.

## Subtasks

### 3.1. Serviço de registro de água (ml e copo)

**Status:** pending  
**Dependencies:** None  

Criar serviço que registra eventos de ingestão e atualiza total diário.

**Details:**

Implementar WaterEntryService para inserir eventos no water_events.json (lista de eventos) com timestamp UTC e recalcular total diário (agregação por data em memória e/ou arquivo daily_summary.json). Suportar entrada em ml e copo padrão.

### 3.2. Desfazer último registro do dia

**Status:** pending  
**Dependencies:** None  

Implementar undo do último evento de ingestão do dia corrente.

**Details:**

Buscar o último evento do dia (UTC) e removê-lo, atualizando o total diário. Retornar mensagem de confirmação.

### 3.3. Comando CLI para log de água

**Status:** pending  
**Dependencies:** None  

Adicionar comando `log` com parsing flexível para ml ou --cup.

**Details:**

Implementar parsing de argumentos, validação e chamada ao serviço de registro. Exibir confirmação com total do dia.

### 3.4. Cobertura de testes para registro e totais diários

**Status:** pending  
**Dependencies:** None  

Adicionar testes unitários para validações e cálculos de totais diários.

**Details:**

Testar cálculo do total diário, regras de validação e comportamento do undo. Inclui bordas (virada do dia UTC).
