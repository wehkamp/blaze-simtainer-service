FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

ARG tag
ENV ASPNETCORE_URLS http://*:5000
EXPOSE 5000
WORKDIR /app
ENTRYPOINT [ "dotnet", "Blaze.SimTainer.Service.Api.dll" ]

COPY publish /app

## Metadata for Consul (see platform docs, services, packaging)
LABEL blaze.service.id="simtainer" \
      blaze.service.name="blaze-simtainer-service" \
      blaze.service.version="${tag}" \
      blaze.service.team="ccoe" \
      blaze.service.main-language="dotnet" \
      blaze.service.features.metrics.enabled="true" \
      blaze.service.features.health-check.enabled="true" \
      blaze.service.features.health-check.endpoint="/status" \
      blaze.service.deployment.cpu="0.1" \
      blaze.service.deployment.memory="100" \
      blaze.service.deployment.minimum-instances="1" \
      blaze.service.deployment.internal-port="5000" \
      blaze.service.deployment.promotion.accept.functional-test-sets.list="" \
      blaze.service.deployment.promotion.prod.manual-step="false" \
      blaze.service.routing.consumer.exposed="false" \
      blaze.service.routing.third-party.exposed="false" \
      blaze.service.routing.trusted.exposed="true"
