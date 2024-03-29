# csharp_containers_api

Solution name fr4nc3.com.containers
open project and build no error or warning

- controller
  - ContainersController
- DTO
  - ErrorResponse
  - ContainerFile
  - Middleware (to read the payload body) // not used but implemented
  - enableMultipleStreamReadMiddleware
  - MiddlewareEntensions
- Models
- Enum
- Development configuration using secrets
- Product Configuration without variables
- swagger

## Init secrets

```
dotnet user-secrets init --project fr4nc3.com.containers
```

```
dotnet user-secrets set "StorageConnection" "DefaultEndpointsProtocol=https;AccountName=fr;AccountKey=KEY;EndpointSuffix=core.windows.net" --project fr4nc3.com.containers
```

```
dotnet user-secrets set "ApplicationInsights:InstrumentationKey" "InstrumentationKey=GUID;IngestionEndpoint=https://eastus-0.in.applicationinsights.azure.com/" --project fr4nc3.com.containers
Successfully saved ApplicationInsights:InstrumentationKey = InstrumentationKey=GUID;IngestionEndpoint=https://eastus-0.in.applicationinsights.azure.com/ to the secret store.
```

```
dotnet user-secrets list --project fr4nc3.com.containers

```

## ApplicationInsights implementation

### Application Insights Configuration

![Screenshot](/Images/appinsighs_configuration.png)

### Debug Logs

![Screenshot](/Images/debug_logs.png)

### Application Map

![Screenshot](/Images/insights_application_map.png)

### Insights Home

![Screenshot](/Images/insights_home.png)

### Insights Performance

![Screenshot](/Images/insights_performance.png)

### Console Logs

![Screenshot](/Images/consolelogs.png)

### Insights API Access

![Screenshot](/Images/insights_api_access.png)

### Insights Errors

![Screenshot](/Images/insights_failures.png)

### Insisghts live Metrics

![Screenshot](/Images/insights_live_metrics.png)

### Insisghts role Assingment

![Screenshot](/Images/insights_role_assign.png)

## Unitests

22 unitests
I modifed the RestSDKClient to make it work with the upload files

## References

https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows

https://blog.elmah.io/asp-net-core-not-that-secret-user-secrets-explained/
