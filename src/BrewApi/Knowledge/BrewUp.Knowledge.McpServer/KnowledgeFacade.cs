using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrewUp.Knowledge.McpServer;

internal sealed class KnowledgeFacade : IKnowledgeFacade
{
    public Task<object> SearchKnowledgeBaseAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}