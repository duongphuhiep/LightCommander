# AI Agent Playground

An experimental playground using an LLM to interpret natural language requests and execute business logic.

## Overview

The system manages a collection of **Lights 🔦** stored in a database. Each light has:

- **Id**
- **Name** (e.g. _Porch_, _Table Lamp_, _Chandelier_)
- **State**: On / Off
- **Color temperature** (in Kelvin, e.g. 1700K flame, 1850K candle, 2550K soft white)

Users interact with the system using natural language to manipulate these entities.

### Supported actions

- **Create lights**  
    Example: `Create a "table lamp" light and a "porch" light`

- **Update state and/or color temperature**  
    Example: `Set porch to studio light temperature and turn it on`

- **Remove lights**  
    Example: `Remove the table lamp`

## Project Status

**Backend**: Functional ✅

- Accepts natural language requests
- Returns natural language responses
- Persists changes to the database

**Frontend**: Not implemented ❌

- Changes are currently inspected directly in the database

## Getting Started

### Step 1: Start Redis

```sh
docker compose up
```

- Redis server: `localhost:6379`
- Redis Insight UI: `localhost:6300`

Redis is used mainly for data visualization while the frontend is not yet available. This project also experiments with RedisJSON as a document store.

### Step 2: Start the Backend (C#)

**Requirement:** OpenAI API key in `backend/api/appsettings.json`  

Create one here: <https://platform.openai.com/settings/organization/api-keys>

```sh
cd backend/api 
dotnet run
```

- API available at: <http://localhost:5140/scalar>

The backend is built with **Semantic Kernel**, allowing easy switching between LLM providers (OpenAI, Azure OpenAI, Google, Anthropic, your local Ollama, etc.). The current setup uses OpenAI.

### Step 3: Interact with the API

There is no frontend yet. Use **Scalar UI** or **Postman** instead. Example requests are available in `backend/api/api.http`.

- Create a chat session:

```http
POST /chat/session
```

- Send a message:

```http
POST /chat/{{ChatSession}}/message
```

- Stream responses (SSE):

```http
POST /chat/{{ChatSession}}/sse
```
