# MCP-Azure

Servidor MCP (Model Context Protocol) para integración con Azure DevOps, permitiendo consultar Work Items (PBIs, User Stories, Bugs, etc.) de forma programática.

## 📋 Descripción

Este proyecto implementa un servidor MCP en C# que se conecta a Azure DevOps para consultar información detallada de Work Items. Permite acceder a PBIs (Product Backlog Items) y otros elementos de trabajo a través del protocolo MCP, facilitando la integración con herramientas que soporten este estándar.

## 🚀 Características

- ✅ Consulta de Work Items por ID
- ✅ Autenticación mediante Personal Access Token (PAT)
- ✅ Información completa de Work Items incluyendo:
  - Metadatos básicos (título, tipo, estado, asignaciones)
  - Planificación (story points, esfuerzo, estimaciones)
  - Información técnica (builds, criterios de aceptación, pasos de reproducción)
  - Descripción y anexos técnicos
- ✅ Formato de salida en Markdown
- ✅ Manejo de errores y excepciones

## 🛠️ Tecnologías

- **.NET 9.0**
- **Model Context Protocol (MCP) v1.1.0**
- **Azure DevOps API** (Microsoft.TeamFoundationServer.Client v20.256.2)
- **Microsoft.Extensions.Hosting** para inyección de dependencias y configuración

## 📦 Estructura del Proyecto

```
MCP-Azure/
├── AzureDevOpsMcp/
│   ├── Program.cs                    # Punto de entrada de la aplicación
│   ├── AzureDevOpsMcp.csproj         # Archivo de proyecto
│   ├── Tools/
│   │   └── WorkItemTools.cs          # Herramientas para consultar Work Items
│   └── .vscode/                      # Configuración de Visual Studio Code
├── publish/                          # Carpeta de publicación
└── Nueva carpeta.sln                 # Solución de Visual Studio
```

## ⚙️ Configuración

### Requisitos Previos

- .NET 9.0 SDK o superior
- Cuenta de Azure DevOps
- Personal Access Token (PAT) con permisos de lectura de Work Items

### Variables de Entorno

El servidor requiere las siguientes variables de entorno:

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `AZDEVOPS_ORG_URL` | URL de tu organización de Azure DevOps | `https://dev.azure.com/mi-organizacion` |
| `AZDEVOPS_PAT` | Personal Access Token con permisos de lectura | `xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx` |

### Obtener un Personal Access Token (PAT)

1. Accede a tu Azure DevOps
2. Ve a **User Settings** > **Personal Access Tokens**
3. Crea un nuevo token con los siguientes permisos:
   - **Work Items**: Read
4. Copia el token generado (no podrás verlo nuevamente)

## 🚀 Instalación y Uso

### 1. Clonar el repositorio

```bash
git clone https://github.com/MbarriosOlimpia/MCP-Azure.git
cd MCP-Azure/AzureDevOpsMcp
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar variables de entorno

**En Windows (PowerShell):**
```powershell
$env:AZDEVOPS_ORG_URL = "https://dev.azure.com/tu-organizacion"
$env:AZDEVOPS_PAT = "tu-personal-access-token"
```

**En Linux/macOS:**
```bash
export AZDEVOPS_ORG_URL="https://dev.azure.com/tu-organizacion"
export AZDEVOPS_PAT="tu-personal-access-token"
```

### 4. Ejecutar el servidor

```bash
dotnet run
```

### 5. Compilar para producción

```bash
dotnet publish -c Release -o ./publish
```

## 📖 Uso del MCP Tool

El servidor expone la siguiente herramienta MCP:

### `GetWorkItem`

Consulta la información completa de un Work Item por su ID.

**Parámetros:**
- `workItemId` (int): ID del Work Item a consultar

**Ejemplo de salida:**
```markdown
## Work Item #12345

**Título**: Implementar autenticación de usuarios
**Tipo**: Product Backlog Item
**Estado**: Active
**Asignado a**: Juan Pérez
**Prioridad**: 1

### Planificación
**Story Points**: 8
**Trabajo restante**: 16h

### Descripción
Implementar el sistema de autenticación...
```

## 🔧 Desarrollo

### Dependencias NuGet

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.5" />
<PackageReference Include="Microsoft.TeamFoundationServer.Client" Version="20.256.2" />
<PackageReference Include="ModelContextProtocol" Version="1.1.0" />
<PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />
```

### Agregar nuevas herramientas

Para agregar nuevas herramientas MCP:

1. Crea una nueva clase en la carpeta `Tools/`
2. Decora la clase con `[McpServerToolType]`
3. Decora los métodos públicos con `[McpServerTool]`
4. Usa `[Description]` para documentar la herramienta y sus parámetros

```csharp
[McpServerToolType]
public class MiNuevaHerramienta
{
    [McpServerTool]
    [Description("Descripción de la herramienta")]
    public async Task<string> MiMetodo(
        [Description("Descripción del parámetro")] string parametro)
    {
        // Implementación
    }
}
```

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Haz fork del repositorio
2. Crea una rama para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## 📝 Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo `LICENSE` para más detalles.

## 📧 Contacto

**Autor**: MbarriosOlimpia  
**Repositorio**: [https://github.com/MbarriosOlimpia/MCP-Azure](https://github.com/MbarriosOlimpia/MCP-Azure)

## 🔗 Enlaces útiles

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [Azure DevOps REST API Reference](https://learn.microsoft.com/en-us/rest/api/azure/devops/)
- [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/)

---

⭐ Si encuentras útil este proyecto, considera darle una estrella en GitHub
