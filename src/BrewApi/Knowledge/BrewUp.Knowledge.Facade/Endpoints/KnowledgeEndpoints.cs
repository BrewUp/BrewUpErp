using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Facade.Governance;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;
using BrewUp.Knowledge.SharedKernel.Wiki;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewUp.Knowledge.Facade.Endpoints;

public static class KnowledgeEndpoints
{
    public static IEndpointRouteBuilder MapKnowledgeEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/knowledge")
            .WithTags("Knowledge");

        group.MapPost("/ingest", HandleIngestKnowledgeDocument)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Ingest a new Knowledge Document")
            .WithDescription(
                "Ingests a new Knowledge Document. This endpoint is used to add a new Knowledge Document.")
            .WithName("IngestKnowledgeDocument");

        group.MapPost("/ingest-file", HandleIngestKnowledgeFile)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Ingest a new Knowledge Document from a file")
            .WithDescription(
                "Ingests a new Knowledge Document from a file. This endpoint is used to add a new Knowledge Document.")
            .WithName("IngestKnowledgeDocumentFromFile");

        group.MapGet("/documents/{documentId:guid}/chunks", HandleGetKnowledgeDocumentChunks)
            .Produces<GetKnowledgeDocumentChunksResult>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get the chunks generated for a Knowledge Document")
            .WithDescription(
                "Returns the generated chunks for inspection, ordered by sequence.")
            .WithName("GetKnowledgeDocumentChunks");

        group.MapPost("/search", HandleSearchKnowledge)
            .Produces<SearchKnowledgeResult>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Search Knowledge chunks")
            .WithDescription(
                "Returns the most relevant Knowledge chunks without generating an answer.")
            .WithName("SearchKnowledge");

        group.MapGet("/documents", HandleGetKnowledgeDocuments)
            .Produces<GetKnowledgeDocumentsResult>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("List Knowledge Documents")
            .WithDescription("Returns all ingested Knowledge Documents.")
            .WithName("GetKnowledgeDocuments");

        group.MapGet("/documents/{documentId:guid}", HandleGetKnowledgeDocument)
            .Produces<GetKnowledgeDocumentResult>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a Knowledge Document")
            .WithDescription("Returns an ingested Knowledge Document and its chunk count.")
            .WithName("GetKnowledgeDocument");

        group.MapDelete("/documents/{documentId:guid}", HandleDeleteKnowledgeDocument)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Delete a Knowledge Document")
            .WithDescription("Deletes the document and its associated chunks and vectors.")
            .WithName("DeleteKnowledgeDocument");

        group.MapPost("/documents/{documentId:guid}/reindex", HandleReindexKnowledgeDocument)
            .Produces<ReindexKnowledgeDocumentResult>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Reindex a Knowledge Document")
            .WithDescription(
                "Regenerates chunks and embeddings from the persisted document content.")
            .WithName("ReindexKnowledgeDocument");

        group.MapPost("/wiki/query", HandleQueryWiki)
            .Produces<WikiSearchResult>()
            .ProducesValidationProblem()
            .WithSummary("Search synthesized Wiki knowledge")
            .WithDescription("Searches derived Wiki pages, not raw document chunks or operational ERP state.")
            .WithName("QueryKnowledgeWiki");

        group.MapGet("/wiki/pages/{key}", HandleGetWikiPage)
            .Produces<WikiPageResult>()
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get a Wiki page")
            .WithName("GetKnowledgeWikiPage");

        group.MapGet("/wiki/pages/{pageId:guid}/evidence", HandleGetWikiPageEvidence)
            .Produces<WikiPageEvidenceResult>()
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get the evidence supporting a Wiki page")
            .WithName("GetKnowledgeWikiPageEvidence");

        group.MapGet("/documents/{documentId:guid}/wiki-job", HandleGetWikiProcessingJob)
            .Produces<WikiProcessingJobResult>()
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get Wiki synthesis status for a document")
            .WithName("GetKnowledgeWikiProcessingJob");

        return app;
    }
    
    private static async Task<IResult> HandleIngestKnowledgeDocument(
        IngestKnowledgeDocument command,
        IngestKnowledgeDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var result = await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(result);
    }
    
    private static async Task<IResult> HandleIngestKnowledgeFile(
        IFormCollection form,
        IngestKnowledgeDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var file = form.Files.GetFile("file")
                   ?? throw new ArgumentException("A file is required.", nameof(form));
        var scope = form["scope"].ToString();
        var tags = form["tags"]
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag!)
            .ToArray();

        await using var stream = file.OpenReadStream();

        var command = new IngestKnowledgeFile(
            file.FileName,
            stream,
            DocumentScope.FromName(scope),
            tags);

        var result = await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetKnowledgeDocumentChunks(
        Guid documentId,
        GetKnowledgeDocumentChunksHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetKnowledgeDocumentChunksQuery(documentId),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> HandleSearchKnowledge(
        SearchKnowledgeQuery query,
        SearchKnowledgeHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException exception)
        {
            var key = string.IsNullOrWhiteSpace(exception.ParamName)
                ? "request"
                : exception.ParamName;

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [key] = [exception.Message]
            });
        }
    }

    private static async Task<IResult> HandleGetKnowledgeDocuments(
        GetKnowledgeDocumentsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetKnowledgeDocument(
        Guid documentId,
        GetKnowledgeDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(documentId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> HandleDeleteKnowledgeDocument(
        Guid documentId,
        DeleteKnowledgeDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        var deleted = await handler.HandleAsync(documentId, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> HandleReindexKnowledgeDocument(
        Guid documentId,
        ReindexKnowledgeDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(documentId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> HandleQueryWiki(
        QueryWiki query,
        QueryWikiHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await handler.HandleAsync(query, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [exception.ParamName ?? "request"] = [exception.Message]
            });
        }
    }

    private static async Task<IResult> HandleGetWikiPage(
        string key,
        GetWikiPageHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(key, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> HandleGetWikiPageEvidence(
        Guid pageId,
        GetWikiPageEvidenceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(pageId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> HandleGetWikiProcessingJob(
        Guid documentId,
        GetWikiProcessingJobHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(documentId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
