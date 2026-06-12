using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;
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
        IFormFile file,
        string scope,
        IngestKnowledgeDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await using var stream = file.OpenReadStream();

        var command = new IngestKnowledgeFile(
            file.FileName,
            stream,
            DocumentScope.FromName(scope),
            []);

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
}
