using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IProposalRepository> _proposalRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _proposalRepositoryMock = new Mock<IProposalRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();

        _documentService = new DocumentService(
            _documentRepositoryMock.Object,
            _proposalRepositoryMock.Object,
            _userRepositoryMock.Object,
            _fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnFalse_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _documentRepositoryMock
            .Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _documentService.DeleteDocumentAsync(documentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldDeleteFromStorageAndDb_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var document = Document.Create(
            proposalId: proposalId,
            title: "Presupuesto 2026",
            description: "Documento de presupuesto",
            fileName: "Presupuesto.pdf",
            storagePath: "proposals/doc.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            uploadedByUserId: userId,
            id: documentId);

        _documentRepositoryMock
            .Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _documentService.DeleteDocumentAsync(documentId);

        // Assert
        result.Should().BeTrue();
        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync(document.StoragePath), Times.Once);
        _documentRepositoryMock.Verify(r => r.DeleteAsync(documentId), Times.Once);
    }
}
