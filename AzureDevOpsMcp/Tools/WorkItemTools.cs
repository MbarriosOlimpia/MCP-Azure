using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools;

[McpServerToolType]
public class WorkItemTools(IConfiguration config)
{
    /// <summary>
    /// Crea una conexión autenticada contra Azure DevOps usando PAT.
    /// La URL de organización y el PAT se leen de variables de entorno:
    ///   AZDEVOPS_ORG_URL  — ej. https://dev.azure.com/mi-org
    ///   AZDEVOPS_PAT      — Personal Access Token
    /// </summary>
    private WorkItemTrackingHttpClient CreateClient()
    {
        var orgUrl = config["AZDEVOPS_ORG_URL"]
            ?? throw new InvalidOperationException(
                "La variable de entorno 'AZDEVOPS_ORG_URL' no está configurada.");

        var pat = config["AZDEVOPS_PAT"]
            ?? throw new InvalidOperationException(
                "La variable de entorno 'AZDEVOPS_PAT' no está configurada.");

        var credentials = new VssBasicCredential(string.Empty, pat);
        var connection = new VssConnection(new Uri(orgUrl), credentials);
        return connection.GetClient<WorkItemTrackingHttpClient>();
    }

    [McpServerTool]
    [Description(
        "Consulta la información de un Work Item de Azure DevOps a partir de su ID. " +
        "Requiere las variables de entorno AZDEVOPS_ORG_URL y AZDEVOPS_PAT configuradas.")]
    public async Task<string> GetWorkItem(
        [Description("ID del Work Item a consultar (número entero).")] int workItemId)
    {
        var client = CreateClient();

        WorkItem? item;
        try
        {
            item = await client.GetWorkItemAsync(
                workItemId,
                expand: WorkItemExpand.Fields);
        }
        catch (Exception ex)
        {
            return $"Error al consultar el Work Item {workItemId}: {ex.Message}";
        }

        if (item is null)
            return $"No se encontró el Work Item con ID {workItemId}.";

        return FormatWorkItem(item);
    }

    private static string FormatWorkItem(WorkItem item)
    {
        var f = item.Fields;
        var sb = new StringBuilder();

        sb.AppendLine($"## Work Item #{item.Id}");
        sb.AppendLine();
        AppendField(sb, "Título",            f, "System.Title");
        AppendField(sb, "Tipo",              f, "System.WorkItemType");
        AppendField(sb, "Estado",            f, "System.State");
        AppendField(sb, "Razón",             f, "System.Reason");
        AppendField(sb, "Asignado a",        f, "System.AssignedTo");
        AppendField(sb, "Creado por",        f, "System.CreatedBy");
        AppendField(sb, "Fecha de creación", f, "System.CreatedDate");
        AppendField(sb, "Última actualización", f, "System.ChangedDate");
        AppendField(sb, "Área",              f, "System.AreaPath");
        AppendField(sb, "Iteración",         f, "System.IterationPath");
        AppendField(sb, "Prioridad",         f, "Microsoft.VSTS.Common.Priority");
        AppendField(sb, "Severidad",         f, "Microsoft.VSTS.Common.Severity");
        AppendField(sb, "Valor de negocio",  f, "Microsoft.VSTS.Common.BusinessValue");
        AppendField(sb, "Riesgo",            f, "Microsoft.VSTS.Common.Risk");
        AppendField(sb, "Área de valor",     f, "Microsoft.VSTS.Common.ValueArea");
        AppendField(sb, "Tags",              f, "System.Tags");

        // --- Planificación / Estimaciones ---
        var hasScheduling =
            f.ContainsKey("Microsoft.VSTS.Scheduling.StoryPoints") ||
            f.ContainsKey("Microsoft.VSTS.Scheduling.Effort") ||
            f.ContainsKey("Microsoft.VSTS.Scheduling.OriginalEstimate") ||
            f.ContainsKey("Microsoft.VSTS.Scheduling.RemainingWork") ||
            f.ContainsKey("Microsoft.VSTS.Scheduling.CompletedWork");

        if (hasScheduling)
        {
            sb.AppendLine();
            sb.AppendLine("### Planificación");
            AppendField(sb, "Story Points",        f, "Microsoft.VSTS.Scheduling.StoryPoints");
            AppendField(sb, "Esfuerzo",            f, "Microsoft.VSTS.Scheduling.Effort");
            AppendField(sb, "Estimación original", f, "Microsoft.VSTS.Scheduling.OriginalEstimate");
            AppendField(sb, "Trabajo restante",    f, "Microsoft.VSTS.Scheduling.RemainingWork");
            AppendField(sb, "Trabajo completado",  f, "Microsoft.VSTS.Scheduling.CompletedWork");
        }

        // --- Información técnica ---
        var hasTechnical =
            f.ContainsKey("Microsoft.VSTS.Build.FoundIn") ||
            f.ContainsKey("Microsoft.VSTS.Build.IntegrationBuild") ||
            f.ContainsKey("Microsoft.VSTS.TCM.ReproSteps") ||
            f.ContainsKey("Microsoft.VSTS.Common.AcceptanceCriteria") ||
            f.ContainsKey("Custom.Technicalannex") ||
            f.ContainsKey("Microsoft.VSTS.Common.Resolution");

        if (hasTechnical)
        {
            sb.AppendLine();
            sb.AppendLine("### Información Técnica");
            AppendField(sb, "Encontrado en build",   f, "Microsoft.VSTS.Build.FoundIn");
            AppendField(sb, "Build de integración",  f, "Microsoft.VSTS.Build.IntegrationBuild");
            AppendField(sb, "Resolución",            f, "Microsoft.VSTS.Common.Resolution");
            AppendField(sb, "Anexo técnico",         f, "Custom.Technicalannex");

            if (f.TryGetValue("Microsoft.VSTS.TCM.ReproSteps", out var repro) && repro is not null)
            {
                sb.AppendLine();
                sb.AppendLine("#### Pasos para reproducir");
                sb.AppendLine(StripHtml(repro.ToString()!));
            }

            if (f.TryGetValue("Microsoft.VSTS.Common.AcceptanceCriteria", out var ac) && ac is not null)
            {
                sb.AppendLine();
                sb.AppendLine("#### Criterios de aceptación");
                sb.AppendLine(StripHtml(ac.ToString()!));
            }
        }

        // --- Descripción ---
        if (f.TryGetValue("System.Description", out var desc) && desc is not null)
        {
            sb.AppendLine();
            sb.AppendLine("### Descripción");
            // Quitar HTML básico para una lectura más limpia
            sb.AppendLine(StripHtml(desc.ToString()!));
        }

        if (item.Url is not null)
        {
            sb.AppendLine();
            sb.AppendLine($"**URL**: {item.Url}");
        }

        return sb.ToString();
    }

    private static void AppendField(
        StringBuilder sb,
        string label,
        IDictionary<string, object> fields,
        string fieldName)
    {
        if (fields.TryGetValue(fieldName, out var value) && value is not null)
        {
            // IdentityRef muestra displayName
            var display = value is Microsoft.VisualStudio.Services.WebApi.IdentityRef ir
                ? ir.DisplayName
                : value.ToString();

            sb.AppendLine($"**{label}**: {display}");
        }
    }

    private static string StripHtml(string html)
    {
        // Eliminación básica de etiquetas HTML
        var noTags = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        var decoded = System.Net.WebUtility.HtmlDecode(noTags);
        // Colapsar espacios múltiples
        return System.Text.RegularExpressions.Regex.Replace(decoded, @"\s{2,}", " ").Trim();
    }
}
