FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
# copy ALL the projects
COPY Base.Contracts/*.csproj ./Base.Contracts/
COPY Base.DAL.Contracts/*.csproj ./Base.DAL.Contracts/
COPY Base.BLL.Contracts/*.csproj ./Base.BLL.Contracts/
COPY Base.Domain/*.csproj ./Base.Domain/
COPY Base.Helpers/*.csproj ./Base.Helpers/
COPY Base.DAL.EF/*.csproj ./Base.DAL.EF/
COPY Base.BLL/*.csproj ./Base.BLL/

COPY Shared.Contracts/*.csproj ./Shared.Contracts/
COPY Modules.Identity/*.csproj ./Modules.Identity/
COPY Modules.Restaurants/*.csproj ./Modules.Restaurants/
COPY Modules.Orders/*.csproj ./Modules.Orders/
COPY App.Resources/*.csproj ./App.Resources/
COPY WebApp/*.csproj ./WebApp/

RUN dotnet restore WebApp/WebApp.csproj

# copy everything else and build app
# copy all the projects
COPY Base.Contracts/. ./Base.Contracts/
COPY Base.DAL.Contracts/. ./Base.DAL.Contracts/
COPY Base.BLL.Contracts/. ./Base.BLL.Contracts/
COPY Base.Domain/. ./Base.Domain/
COPY Base.Helpers/. ./Base.Helpers/
COPY Base.DAL.EF/. ./Base.DAL.EF/
COPY Base.BLL/. ./Base.BLL/

COPY Shared.Contracts/. ./Shared.Contracts/
COPY Modules.Identity/. ./Modules.Identity/
COPY Modules.Restaurants/. ./Modules.Restaurants/
COPY Modules.Orders/. ./Modules.Orders/
COPY App.Resources/. ./App.Resources/
COPY WebApp/. ./WebApp/

WORKDIR /app/WebApp
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:latest AS runtime
EXPOSE 8083
WORKDIR /app
COPY --from=build /app/WebApp/out ./
ENTRYPOINT ["dotnet", "WebApp.dll"]
