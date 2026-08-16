namespace CommunityVoting.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Proposal Proposal { get; set; } = null!;
    public User UploadedByUser { get; set; } = null!;

    public Document() { }

    public static Document Create(
        Guid proposalId,
        string title,
        string description,
        string fileName,
        string storagePath,
        string contentType,
        long fileSize,
        Guid uploadedByUserId,
        Guid? id = null)
    {
        if (proposalId == Guid.Empty) throw new ArgumentException("El ID de la propuesta es obligatorio.", nameof(proposalId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título del documento es obligatorio.", nameof(title));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("El nombre de archivo es obligatorio.", nameof(fileName));

        return new Document
        {
            Id = id ?? Guid.NewGuid(),
            ProposalId = proposalId,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            FileName = fileName.Trim(),
            StoragePath = storagePath,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow
        };
    }
}
