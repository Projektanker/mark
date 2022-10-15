FROM mcr.microsoft.com/dotnet/sdk:6.0 as stage
WORKDIR /src
COPY src/. .
RUN wget -O "pandoc.deb" \
    'https://github.com/jgm/pandoc/releases/download/2.19.2/pandoc-2.19.2-1-amd64.deb' 

# BUILD
FROM stage as build
WORKDIR /src

RUN dotnet restore --use-current-runtime
RUN dotnet build --no-restore

# TEST
FROM build as test
WORKDIR /src

# install pandoc
RUN dpkg -i "pandoc.deb"

# install playwright
RUN dotnet ./Mark.Web/bin/Debug/net6.0/Mark.Web.dll install && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /tmp/*

# run tests
RUN dotnet test --no-build 

FROM test as publish
RUN dotnet publish --use-current-runtime --no-restore -c Release -o /app --no-self-contained Mark.Web/Mark.Web.csproj

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=publish /app .

# install pandoc
COPY --from=stage /src/pandoc.deb ./pandoc.deb
RUN dpkg -i "pandoc.deb" && \
    rm -f "pandoc.deb"

# install playwright
RUN dotnet Mark.Web.dll install && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /tmp/*

USER ${APP_USER}

ENV ASPNETCORE_URLS=http://+:8080
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "Mark.Web.dll"]
