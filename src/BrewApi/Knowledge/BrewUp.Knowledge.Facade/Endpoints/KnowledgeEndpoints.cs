using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Facade.Ingestion;
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
        
        group.MapPost("/ingest",HandleIngestKnowledgeDocument)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Ingest a new Knowledge Document")
            .WithDescription(
                "Ingests a new Knowledge Document. This endpoint is used to add a new Knowledge Document.")
            .WithName("IngestKnowledgeDocument");
        group.MapPost("/ingest-file",HandleIngestKnowledgeFile)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Ingest a new Knowledge Document from a file")
            .WithDescription(
                "Ingests a new Knowledge Document from a file. This endpoint is used to add a new Knowledge Document.")
            .WithName("IngestKnowledgeDocumentFromFile");

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
}