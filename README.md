# Hidratacao

## Task Master - passo a passo (basico)

Execute sempre a partir da raiz do projeto.

### 1) Preparar o ambiente
- Copie `.env.example` para `.env`
- Preencha `OPENAI_API_KEY` com sua chave

### 2) Gerar tarefas a partir do PRD
```powershell
task-master parse-prd .taskmaster/docs/prd.md
```

### 2.1) Opcional - TryHamster
Usamos o gerador de tarefas do TryHamster e depois importamos/ajustamos no Task Master.
Link: `https://tryhamster.com/`

### 3) Ver tarefas
```powershell
task-master list
task-master show 1
```

### 4) Atualizar status
```powershell
task-master set-status --id 1 --status in-progress
task-master set-status --id 1 --status done
```

### 5) Detalhar uma tarefa em subtasks
```powershell
task-master expand --id 1
```

### 6) Atualizar descricao de tarefa
```powershell
task-master update-task --id 1 --prompt "ajuste de requisitos"
```

Observacoes:
- Se o Task Master estiver usando `tasks.json`, as tarefas ficam em `.taskmaster/tasks/tasks.json`.
- A chave da API fica apenas no `.env` (nao commitado).
- Se o Codex CLI falhar com erro de schema (ex.: `Invalid schema for response_format 'codex_output_schema'`), isso e um erro do provedor/CLI e nao do PRD. Tente outro provider ou valide a chave/limites da API.
