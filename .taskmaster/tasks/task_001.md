# Task ID: 1

**Title:** Configurações de ingestão de água (meta, horários e copo padrão)

**Status:** in-progress

**Dependencies:** None

**Priority:** high

**Description:** Permite ao usuário configurar meta diária, horário ativo, intervalo de lembretes e tamanho padrão do copo, com persistência local.

**Details:**

Implementar um módulo de configurações para o app CLI. As configurações devem incluir: meta diária (ml), horário ativo (HH:MM-HH:MM), intervalo de lembretes (min) e tamanho padrão do copo (ml). Persistir em JSON (arquivo settings.json com objeto único), carregar no startup e permitir atualização via comandos CLI. Fornecer valores padrão na primeira execução (ex.: 2000ml, 08:00-22:00, 30 min, 250ml). Validar entradas (valores positivos, formato de hora válido, fim > início). Arquitetura em camadas: Domain (entidades/validações), Application (serviço de config), Infrastructure (repositório JSON), CLI (comandos).

**Test Strategy:**

Testes unitários para validações (valores negativos, formato de hora, fim < início) e persistência (salvar e reabrir mantendo valores). Testar comandos CLI para cada parâmetro e comando de exibição das configs.

## Subtasks

### 1.1. Definir esquema JSON para armazenamento das configurações

**Status:** pending  
**Dependencies:** None  

Criar arquivo settings.json (objeto único) com campos: daily_goal_ml, active_hours_start, active_hours_end, reminder_interval_minutes, default_cup_ml e timestamps.

**Details:**

Criar inicialização que cria o arquivo se não existir e grava valores padrão. Garantir objeto único de configurações no settings.json. Implementar repositório JSON com Get/Save.

### 1.2. Implementar validação das configurações de ingestão

**Status:** pending  
**Dependencies:** None  

Validar meta diária, horários ativos, intervalo e copo padrão antes de persistir.

**Details:**

Criar um validador com regras: valores numéricos > 0; horários no formato HH:MM (24h); horário final deve ser maior que o inicial. Retornar mensagens claras de erro para uso no CLI.

### 1.3. Criar comandos CLI para visualizar e atualizar configurações

**Status:** pending  
**Dependencies:** None  

Adicionar comandos: config --show, --goal, --active-hours, --interval, --cup-size.

**Details:**

Implementar parsing de argumentos no CLI, chamar validador e repositório. Mostrar confirmação ao usuário e erros amigáveis. Integrar com DI/serviços existentes.

### 1.4. Modelar entidade Settings e validações de domínio

**Status:** pending  
**Dependencies:** None  

Criar o modelo de configurações com regras de validação.

**Details:**

Definir entidade/DTO com daily_goal_ml, active_hours_start/end, reminder_interval_minutes e default_cup_ml. Implementar validações de valores positivos, formato HH:MM e regra fim > início, retornando mensagens claras.

### 1.5. Implementar repositório JSON para settings.json

**Status:** pending  
**Dependencies:** 1.4  

Persistir e carregar configurações locais em JSON.

**Details:**

Criar repositório que lê/grava `settings.json` como objeto único, cria arquivo com valores padrão na primeira execução e registra timestamps de criação/atualização.

### 1.6. Criar serviço de aplicação para configurações

**Status:** pending  
**Dependencies:** 1.4, 1.5  

Orquestrar leitura, atualização e validação das configurações.

**Details:**

Implementar serviço com métodos Get/Update, aplicando validações antes de persistir, mesclando atualizações parciais e retornando resultado/erros para a camada CLI.

### 1.7. Adicionar comandos CLI para exibir e atualizar configs

**Status:** pending  
**Dependencies:** 1.6  

Expor comandos `config` no CLI com parâmetros de atualização.

**Details:**

Implementar `config --show`, `--goal`, `--active-hours`, `--interval`, `--cup-size`, com parsing de argumentos, mensagens de sucesso e erros amigáveis integrados ao serviço.

### 1.8. Cobrir testes de persistência e integração de comandos

**Status:** pending  
**Dependencies:** 1.4, 1.5, 1.6, 1.7  

Garantir comportamento esperado das configurações end-to-end.

**Details:**

Criar testes para valores inválidos (negativos, horário malformado, fim <= início) e para salvar/abrir mantendo valores; adicionar testes de comandos se houver harness existente.
