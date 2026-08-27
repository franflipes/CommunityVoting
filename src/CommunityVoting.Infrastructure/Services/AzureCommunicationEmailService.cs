using Azure;
using Azure.Communication.Email;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommunityVoting.Infrastructure.Services;

public class AzureCommunicationEmailService : IEmailService
{
    private readonly EmailClient? _emailClient;
    private readonly string _senderAddress;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<AzureCommunicationEmailService> _logger;

    public AzureCommunicationEmailService(
        IConfiguration configuration,
        IEmailTemplateRenderer templateRenderer,
        ILogger<AzureCommunicationEmailService> logger)
    {
        _templateRenderer = templateRenderer;
        _logger = logger;

        _senderAddress = configuration["Email:SenderAddress"]
            ?? configuration["AzureCommunication:SenderAddress"]
            ?? "DoNotReply@azurecomm.net";

        var connectionString = configuration.GetConnectionString("AzureCommunication")
            ?? configuration["AzureCommunication:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains("YOUR_AZURE_COMMUNICATION_CONNECTION_STRING"))
        {
            try
            {
                _emailClient = new EmailClient(connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo inicializar Azure Communication EmailClient. Se usará el modo simulado/desarrollo.");
                _emailClient = null;
            }
        }
        else
        {
            _logger.LogInformation("Azure Communication Services no está configurado con una cadena de conexión real. Operando en modo desarrollo/consola.");
            _emailClient = null;
        }
    }

    public async Task SendMeetingInvitationAsync(
        string recipient,
        MeetingInvitationEmailModel model,
        CancellationToken cancellationToken = default)
    {
        var (html, text) = await _templateRenderer.RenderMeetingInvitationAsync(model);
        var subject = $"Tienes una nueva reunión: {model.MeetingTitle}";

        if (_emailClient != null)
        {
            var content = new EmailContent(subject)
            {
                Html = html,
                PlainText = text
            };

            var emailMessage = new EmailMessage(
                senderAddress: _senderAddress,
                recipientAddress: recipient,
                content: content
            );

            _logger.LogInformation("Enviando correo vía Azure Communication Services a {Recipient} con asunto '{Subject}'", recipient, subject);
            EmailSendOperation operation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);
            _logger.LogInformation("Correo enviado con éxito a {Recipient}. OperationId: {OperationId}", recipient, operation.Id);
        }
        else
        {
            _logger.LogInformation("=== [DEV SIMULATION EMAIL ENVIADO] ===");
            _logger.LogInformation("Para: {Recipient}", recipient);
            _logger.LogInformation("Remitente: {Sender}", _senderAddress);
            _logger.LogInformation("Asunto: {Subject}", subject);
            _logger.LogInformation("Enlace: {AccessUrl}", model.AccessUrl);
            _logger.LogInformation("Código de acceso: {AccessCode}", model.AccessCode);
            _logger.LogInformation("=======================================");
        }
    }
}
