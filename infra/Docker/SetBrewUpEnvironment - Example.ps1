dotnet user-secrets set "Parameters:mongo-connection-string" "..."

dotnet user-secrets set "Parameters:eventstore-connection-string" "esdb://localhost:4113?tls=false&tlsVerifyCert=false"

dotnet user-secrets set "Parameters:servicebus-connection-string" "..."
dotnet user-secrets set "Parameters:servicebus-topic-name" "brewup"
dotnet user-secrets set "Parameters:servicebus-client-id" "brewup"

dotnet user-secrets set "Parameters:sqlserver-connection-string" "..."

dotnet user-secrets set "Parameters:azure-openai-endpoint" "..."
dotnet user-secrets set "Parameters:azure-openai-deployment-name" "mistral-small-2503"
dotnet user-secrets set "Parameters:azure-openai-api-key" "..."
dotnet user-secrets set "Parameters:azure-openai-tenant-id" "..."
dotnet user-secrets set "Parameters:azure-openai-use-managed-identity" "false"

dotnet user-secrets set "Parameters:embeddings-endpoint" "..."
dotnet user-secrets set "Parameters:embeddings-deployment-name" "text-embedding-3-small"
dotnet user-secrets set "Parameters:embeddings-dimensions" "1536"
dotnet user-secrets set "Parameters:embeddings-api-key" "..."
dotnet user-secrets set "Parameters:embeddings-tenant-id" "..."
dotnet user-secrets set "Parameters:embeddings-use-managed-identity" "false"

dotnet user-secrets set "Parameters:rabbitmq-host" "localhost"
dotnet user-secrets set "Parameters:rabbitmq-exchange-command-name" "brewup.command.exchange"
dotnet user-secrets set "Parameters:rabbitmq-exchange-event-name" "brewup.event.exchange"
dotnet user-secrets set "Parameters:rabbitmq-username" "guest"
dotnet user-secrets set "Parameters:rabbitmq-password" "guest"