namespace BrewUp.Shared.Agents;

public class ConversationRoot(Guid conversationId)
{
    Guid _conversationId = conversationId;
    IEnumerable<AgentResponse> _responses = [];

    public void RaiseConversation(AgentResponse response)
    {
        _responses = _responses.Append(response);
    }
    
    protected void ClearConversation()
    {
        _responses =  [];
    }
}