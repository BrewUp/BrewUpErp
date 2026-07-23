@description('Azure region for the Container App.')
param location string

@description('Name of the Container App.')
param name string

@description('Container Apps environment resource ID.')
param environmentId string

@description('Container Apps environment default domain.')
param environmentDefaultDomain string

@description('Container image repository name without registry server or tag.')
param imageName string

@description('Container image tag.')
param imageTag string = 'latest'

@description('Container port exposed by the application.')
param targetPort int = 8080

@description('Enable public external ingress.')
param externalIngress bool = true

@description('Container registry login server.')
param registryServer string

@description('Container registry username.')
param registryUsername string

@secure()
@description('Container registry password.')
param registryPassword string

@description('Additional environment variables for the container.')
param environmentVariables array = []

@secure()
@description('Additional Container App secrets.')
param secrets object = {}

@description('Additional environment variables backed by Container App secrets.')
param secretEnvironmentVariables array = []

@description('Minimum replica count.')
@minValue(0)
param minReplicas int = 1

@description('Maximum replica count.')
@minValue(1)
param maxReplicas int = 3

@description('CPU cores allocated to the container.')
param cpu string = '0.5'

@description('Memory allocated to the container.')
param memory string = '1Gi'

@description('Tags applied to the Container App.')
param tags object = {}

var containerName = replace(name, '-', '')
var image = '${registryServer}/${imageName}:${imageTag}'
var baseEnvironmentVariables = [
  {
    name: 'ASPNETCORE_HTTP_PORTS'
    value: string(targetPort)
  }
]
var baseSecrets = [
  {
    name: 'acr-password'
    value: registryPassword
  }
]
var additionalSecrets = [
  for secret in items(secrets): {
    name: secret.key
    value: secret.value
  }
]
var fqdn = '${name}.${environmentDefaultDomain}'

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: externalIngress
        targetPort: targetPort
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: registryServer
          username: registryUsername
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: concat(baseSecrets, additionalSecrets)
    }
    template: {
      containers: [
        {
          name: containerName
          image: image
          env: concat(baseEnvironmentVariables, environmentVariables, secretEnvironmentVariables)
          resources: {
            cpu: json(cpu)
            memory: memory
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output name string = app.name
output fqdn string = fqdn
output url string = 'https://${fqdn}'
