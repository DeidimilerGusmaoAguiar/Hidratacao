# Task ID: 2

**Title:** Lembretes programados dentro do horário ativo

**Status:** pending

**Dependencies:** 1 ⧖

**Priority:** high

**Description:** Executa um modo daemon que dispara lembretes em intervalos configuráveis apenas no horário ativo.

**Details:**

Implementar um processo daemon que roda em background no console e agenda lembretes conforme intervalo configurado, respeitando o horário ativo. O lembrete deve exibir hora atual, quanto falta para a meta e sugestão de ingestão (ex.: 250ml), além de beep no console. Garantir que apenas uma instância do daemon rode. Recalcular próximo lembrete ao mudar configurações.

**Test Strategy:**

Testar agendamento com diferentes intervalos, garantir que lembretes só ocorram no horário ativo, e validar comportamento ao iniciar/parar o daemon.

## Subtasks

### 2.1. Gerenciar processo daemon (start/stop e instância única)

**Status:** pending  
**Dependencies:** None  

Criar controle de ciclo de vida do daemon e impedir múltiplas instâncias simultâneas.

**Details:**

Implementar lock de instância única (arquivo/Mutex). Criar comandos start/stop para o modo daemon e tratamento de encerramento gracioso.

### 2.2. Motor de agendamento por intervalo

**Status:** pending  
**Dependencies:** None  

Agendar lembretes com base no intervalo configurado e no próximo horário válido.

**Details:**

Implementar cálculo do próximo lembrete (considerando horário ativo), usando Timer/Task.Delay e recalculando após cada disparo.

### 2.3. Validar horário ativo e bloquear lembretes fora da janela

**Status:** pending  
**Dependencies:** None  

Garantir que lembretes sejam emitidos apenas dentro do horário ativo configurado.

**Details:**

Aplicar regra start < now < end; se fora, calcular próximo horário de início e aguardar.

### 2.4. Notificação de lembrete no console com beep

**Status:** pending  
**Dependencies:** None  

Exibir mensagem no console com hora, faltante e sugestão de ingestão; emitir beep simples.

**Details:**

Usar Console.Beep e Console.WriteLine para exibir alerta. Mensagem deve incluir horário atual, ml restantes e sugestão baseada no copo padrão.
