# PRD - Hidratacao (Water Reminder + Water Log) - v1

## 1. Objetivo
Criar um app local (CLI) para:
- lembrar o usuario de beber agua em intervalos configuraveis
- registrar a quantidade ingerida (ml) ao longo do dia
- mostrar progresso vs meta diaria
- manter historico diario local

## 2. Publico-alvo
Dev (power user) que quer algo rapido, sem conta, sem nuvem, rodando no Windows.

## 3. Escopo (v1)
3.1 Configuracao
- Definir meta diaria (ml)
- Definir horario ativo (ex: 08:00-22:00)
- Definir intervalo de lembrete (min)
- Definir tamanho padrao de um "copo" (ml)

3.2 Lembretes
- Um modo "daemon" que fica rodando e lembra no intervalo, apenas dentro do horario ativo
- O lembrete deve mostrar: hora, quanto falta pra meta, sugestao (ex: "beba 250ml")
- Para v1, lembrete pode ser texto no console + beep (sem depender de integracoes)

3.3 Registro de consumo
- Comando para registrar ingestao: "log <ml>" e "log --cup" (usa tamanho padrao)
- Permitir desfazer o ultimo registro do dia (undo last)
- Guardar timestamp de cada registro

3.4 Visualizacao
- "status": total hoje, percentual, faltante, proximo lembrete (se daemon ativo)
- "history": lista dos ultimos N dias com total por dia
- "export": exportar CSV do historico (por dia e por eventos)

3.5 Persistencia
- Armazenamento local em SQLite (arquivo no diretorio do app)
- Tabelas:
  - settings (1 row)
  - water_events (id, timestamp_utc, ml)
  - daily_summary (date, total_ml) (pode ser view/derivado)

## 4. Nao-escopo (v1)
- Nuvem/sync
- App mobile / UI grafica
- Push notifications do sistema

## 5. Requisitos tecnicos
- .NET 10
- App console com subcomandos
- Separar camadas: Domain / Application / Infrastructure / Cli
- Testes unitarios para regras:
  - calculo de progresso diario
  - horario ativo
  - agendamento do proximo lembrete
  - undo do ultimo evento

## 6. Criterios de aceite (v1)
- Consigo configurar meta/horario/intervalo/copo
- Consigo rodar "daemon" e receber lembretes apenas no horario ativo
- Consigo registrar agua e ver "status" correto
- Consigo ver historico e exportar CSV
- Persistencia funciona (fecha e abre e nao perde dados)
- Testes passando
