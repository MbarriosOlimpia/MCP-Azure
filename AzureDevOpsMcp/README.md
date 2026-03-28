# Azure DevOps MCP Server

Servidor [Model Context Protocol (MCP)](https://modelcontextprotocol.io) en .NET 9 que permite consultar Work Items de Azure DevOps desde cualquier cliente MCP compatible (GitHub Copilot, Claude Desktop, etc.).

## Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Una organización en Azure DevOps
- Un [Personal Access Token (PAT)](https://learn.microsoft.com/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate) con permiso **Work Items (Read)**

## Configuración

El servidor lee la configuración desde **variables de entorno**:

| Variable            | Descripción                                             | Ejemplo                                   |
|---------------------|---------------------------------------------------------|-------------------------------------------|
| `AZDEVOPS_ORG_URL`  | URL de la organización Azure DevOps                     | `https://dev.azure.com/mi-organizacion`   |
| `AZDEVOPS_PAT`      | Personal Access Token con permisos de lectura de WI     | `abc123...`                               |

## Compilar y publicar

```bash
dotnet build
dotnet publish -c Release -o ./publish
```

## Registrar en VS Code (GitHub Copilot Agent)

Agrega la siguiente entrada en tu archivo de configuración MCP (`.vscode/mcp.json` o en la configuración de usuario):

```json
{
  "servers": {
    "azure-devops": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/ruta/a/AzureDevOpsMcp/AzureDevOpsMcp.csproj",
        "--no-build"
      ],
      "env": {
        "AZDEVOPS_ORG_URL": "https://dev.azure.com/mi-organizacion",
        "AZDEVOPS_PAT": "tu-pat-aqui"
      }
    }
  }
}
```

O usando el ejecutable publicado:

```json
{
  "servers": {
    "azure-devops": {
      "type": "stdio",
      "command": "C:/ruta/a/publish/AzureDevOpsMcp.exe",
      "env": {
        "AZDEVOPS_ORG_URL": "https://dev.azure.com/mi-organizacion",
        "AZDEVOPS_PAT": "tu-pat-aqui"
      }
    }
  }
}
```

## Herramientas disponibles

### `GetWorkItem`

Consulta la información completa de un Work Item por su ID.

**Parámetros:**

| Parámetro    | Tipo | Descripción              |
|--------------|------|--------------------------|
| `workItemId` | int  | ID del Work Item a consultar |

**Campos retornados:**
- Título, Tipo, Estado, Razón
- Asignado a, Creado por
- Fechas de creación y última actualización
- Área e Iteración
- Prioridad, Severidad, Tags
- Descripción (HTML removido)
- URL directa al Work Item

**Ejemplo de uso en Copilot:**
```
Dame la información del Work Item 1234
```

## Estructura del proyecto

```
AzureDevOpsMcp/
├── Program.cs              # Punto de entrada: configura el host y MCP stdio
├── Tools/
│   └── WorkItemTools.cs    # Herramienta GetWorkItem
├── AzureDevOpsMcp.csproj
├── NuGet.config            # Usa solo nuget.org
└── README.md
```

## Seguridad

- El PAT **nunca** se pasa como argumento de la herramienta; se lee de variables de entorno.
- Usa el mínimo privilegio necesario: solo permiso **Work Items (Read)** en el PAT.
- Todos los logs se escriben en `stderr` para no contaminar el canal MCP.
