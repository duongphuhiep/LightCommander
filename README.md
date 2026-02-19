# AI Agent Playground

An experimental playground using an LLM to interpret natural language requests and execute business logic.

## Overview

The system manages a collection of **Lights 🔦** stored in a database. Each light has:

- **Id**
- **Name** (e.g. _Porch_, _Table Lamp_, _Chandelier_)
- **State**: On / Off
- [**Color temperature**](https://en.wikipedia.org/wiki/Color_temperature) (in Kelvin, e.g. 1700K flame, 1850K candle, 2550K soft white)

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

### Requirement: LLM provider

The backend is built with **Semantic Kernel**, allowing easy switching between LLM providers (OpenAI, Azure OpenAI, Google, Anthropic, your local Ollama, etc.). 
The current setup uses ~~OpenAI~~ [Ollama](https://ollama.com/) running in your localhost (so that you can test for free with any OpenSource model).

* ~~OpenAI's API key in `backend/api/appsettings.json`. Create one [here](https://platform.openai.com/settings/organization/api-keys>)~~
* [Ollama](https://ollama.com/) running in `http://localhost:11434/`.
    * list available models in your machine with `ollama list`, choose one to put in `backend/api/appsettings.json`

### Step 1: Start Redis

```sh
docker compose up
```

- Redis server: `localhost:6379`
- Redis Insight UI: `localhost:6300`

Redis is used mainly for data visualization while the frontend is not yet available. This project also experiments with RedisJSON as a document store.

### Step 2: Start the Backend (C#)

```sh
cd backend/api 
dotnet run
```

- API available at: <http://localhost:5140/scalar>

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

## How the magic works behind the scenes (Model-driven planning)

The file [App_LLM_interraction.txt](./App_LLM_interraction.txt) logs all interraction between our application and the LLM (OpenAPI in our case).

For example:

1) User asked `Create a "table lamp" light and a "porch" light`

2a) Our App sent this request to the LLM, along with a JSON schema describing the Tools (or functions) it is capable to execute:

```json
{
    "messages": [
        {
          "role": "user",
          "content": "create a \"table lamp\" light and a \"porch\" light"
        }
      ],
     "model": "gpt-4o",
  "tools": [
    {
      "type": "function",
      "function": {
        "description": "Creates a new light with the given name",
        "name": "Lights-create_light",
        "parameters": {
          "type": "object",
          "required": ["lightName"],
          "properties": { "lightName": { "type": "string" } }
        },
        "strict": false
      }
    },
    ...
  ]
}
```

2b) The LLM response this:

```json
{
  "object": "chat.completion",
  "model": "gpt-4o-2024-08-06",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": null,
        "tool_calls": [
          {
            "id": "call_AzLL9Ra8hvWB3vTJTXx75VDn",
            "type": "function",
            "function": {
              "name": "Lights-create_light",
              "arguments": "{\"lightName\": \"table lamp\"}"
            }
          },
          {
            "id": "call_rXrAihAZXKGkq48P5hzGaDRs",
            "type": "function",
            "function": {
              "name": "Lights-create_light",
              "arguments": "{\"lightName\": \"porch\"}"
            }
          }
        ],
      },
    }
  ],
}
```

Basically, it asked our App to execute the `Lights-create_light` tool 2 times with the given input.

3a) Our App followed the LLM instruction, executed the tool and Sent the result to the LLM

```json
{
    "messages": [
        {
          "role": "tool",
          "tool_call_id": "call_AzLL9Ra8hvWB3vTJTXx75VDn",
          "content": "{\"id\":7,\"name\":\"table lamp\",\"is_on\":false,\"temperature\":0}"
        },
        {
          "role": "tool",
          "tool_call_id": "call_rXrAihAZXKGkq48P5hzGaDRs",
          "content": "{\"id\":8,\"name\":\"porch\",\"is_on\":false,\"temperature\":0}"
        }   
    ]
}
```

3b) The LLM check the execution result and returned

```json
{
  "model": "gpt-4o-2024-08-06",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "The \"table lamp\" and \"porch\" lights have been successfully created. Both lights are currently turned off and their temperature settings are at the default level.",
        "refusal": null,
        "annotations": []
      },
      "logprobs": null,
      "finish_reason": "stop"
    }
  ],
}
```

On the surface, User only see the final result:

```text
User > Create a "table lamp" light and a "porch" light
App  > The "table lamp" and "porch" lights have been successfully created. Both lights are currently turned off and their temperature settings are at the default level.
```

For complex request, there might have more round-trips between our App and the LLM.

In resume, what's happened here is called **model-driven planning**:

1. User sends a complex request
2. App sends prompt + available tools to OpenAI
3. Model responds: “Call Tool A with X”
4. App executes Tool A
5. App sends Tool A’s result back to OpenAI
6. Model responds: “Call Tool B with Y”
7. App executes Tool B
8. Repeat…
9. Final natural-language answer
