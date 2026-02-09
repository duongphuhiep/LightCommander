# AI Agent playground

This playground uses a LLM to execute business logic codes.

## Business requirement

Our database stores collection of Lights 🔦. Each light has
  
* An Id
* A name ("Porch", "Table Lamp", "Chandelier")
* A state: On / Off
* [A color temperature value attribute](https://en.wikipedia.org/wiki/Color_temperature) For eg: (1700K - Match flame, 1850K - Candle flame, 2550K - Soft white...)

Users manipulate the business entity (Light) by sending request in natural language to the application. They should be able to:

* Add new lights to the database: Prompt example: `create a "table lamp" light and a "porch" light`.
* Change the light state and/or color attribute: Prompt example: `Set porch to studio light temperature and turn it on`.
* Remove light in the database: `remove the table lamp`

## Project status

Work in progress.

* Backend: operational ✅ - we can send request, get the response in natural language and observe changes in the database.
* Frontend: not yet developed ❌. We observe the changes directly in the database for now.

## How to play

### Step 1: Start a Redis database server

```sh
docker compose up
```

* It will start a Redis Server at `localhost:6379`
* You can observe the data at `localhost:6300` (Redis Insight UI). For eg when you ask the system to create new light you will see new entity created or changed in the database via this UI.

> Side note: An in memory database should be enough for the project. However I need tool to visualize my data (lights) when the Frontend is not yet ready. In the other hand, I wanted to experiment RedisJSON as a Document database (...and as a conclusion, I don't like it)

### Step 2: Start the backend Csharp

Requires: OpenApi's Api Key in `backend/api/appsettings.json`. Create one [here](https://platform.openai.com/settings/organization/api-keys)

```sh
cd backend/api
dotnet run
```

* It will start the API at `localhost:5140/scalar`

> Side note: The project uses Semantic Kernel, which can plug to [a wide range of LLM providers](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/) (Azure OpenAI, Google, Anthropic, your local Ollama...). The current implementation uses OpenAI (simply because I've had already an API Key in the Pocket)

### Step 3: Talk to the backend

The frontend had not been developed yet. But you can use the scalar UI, or Postman to talk to the backend. Checkout examples in [api.http](backend/api/api.http)

* Create a chat session

```http
POST /chat/session
```

* Send a message and get the response in natural language

```http
POST /chat/{{ChatSession}}/message
```

* In order to get the response in streaming use

```http
POST /chat/{{ChatSession}}/sse
```
