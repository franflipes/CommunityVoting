using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.Infrastructure.Templates;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly string _templatesDirectory;

    public EmailTemplateRenderer()
    {
        _templatesDirectory = Path.Combine(AppContext.BaseDirectory, "Templates", "Meeting");
    }

    public async Task<(string html, string text)> RenderMeetingInvitationAsync(MeetingInvitationEmailModel model)
    {
        var htmlPath = Path.Combine(_templatesDirectory, "Invitation.html");
        var txtPath = Path.Combine(_templatesDirectory, "Invitation.txt");

        string htmlTemplate = File.Exists(htmlPath)
            ? await File.ReadAllTextAsync(htmlPath)
            : GetDefaultHtmlTemplate();

        string txtTemplate = File.Exists(txtPath)
            ? await File.ReadAllTextAsync(txtPath)
            : GetDefaultTxtTemplate();

        var html = ReplaceTokens(htmlTemplate, model);
        var text = ReplaceTokens(txtTemplate, model);

        return (html, text);
    }

    private static string ReplaceTokens(string template, MeetingInvitationEmailModel model)
    {
        return template
            .Replace("{{FirstName}}", model.FirstName ?? "Miembro")
            .Replace("{{CommunityName}}", model.CommunityName ?? "Comunidad")
            .Replace("{{MeetingTitle}}", model.MeetingTitle ?? "Reunión")
            .Replace("{{MeetingDate}}", model.MeetingDate ?? string.Empty)
            .Replace("{{MeetingTime}}", model.MeetingTime ?? string.Empty)
            .Replace("{{AccessUrl}}", model.AccessUrl ?? string.Empty)
            .Replace("{{AccessCode}}", model.AccessCode ?? string.Empty);
    }

    private static string GetDefaultHtmlTemplate() => @"
<h2>Hola {{FirstName}},</h2>
<p>Tienes una nueva reunión: <strong>{{MeetingTitle}}</strong> en {{CommunityName}}.</p>
<p>Fecha: {{MeetingDate}} {{MeetingTime}}</p>
<p><a href=""{{AccessUrl}}"">Acceder a la reunión</a></p>
<p>Código de acceso: <strong>{{AccessCode}}</strong></p>";

    private static string GetDefaultTxtTemplate() => @"
Hola {{FirstName}},
Tienes una nueva reunión: {{MeetingTitle}} en {{CommunityName}}.
Fecha: {{MeetingDate}} {{MeetingTime}}
Acceso: {{AccessUrl}}
Código: {{AccessCode}}";
}
