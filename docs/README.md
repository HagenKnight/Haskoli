# Clean Architecture template

> By Giovanni García R :man_technologist: and Github Copilot :rocket:

The purpose of this project is to create a template for Clean Architecture based projects using software design patterns such as Repository Pattern, Unit Of Work Pattern, CQRS Pattern and the use of ORM (EFCore).

As an example, only was implemented an API with a simple CRUD of Country entity.

## Prerequisites

- **.NET 10 SDK** (`net10.0`). Verify with `dotnet --list-sdks`.
- Entity Framework Core 10 (`10.0.x`) is the ORM. Install the matching EF tools with `dotnet tool update --global dotnet-ef --version 10.0.10`.

## Installing the template with DotNet

You must run the follow command from root folder of this project.
```
dotnet new install .
```

Confirm the installation runnnig the follow command

```
dotnet new list
```

## New project from new template

```
dotnet new kasei.arch -o yourNewProjectName
```


## Selecting the database provider
By default the project uses **SQL Server** (`"Database": "mssql"` in `appsettings.json`).

> **MySQL is temporarily unavailable.** `Pomelo.EntityFrameworkCore.MySql` has no EF Core 10 release yet, so the MySQL provider was parked during the .NET 10 upgrade. Selecting `"Database": "mysql"` now throws a clear `NotSupportedException`. The `mysql` branches are kept in `HaskoliDbContextFactory`, the Persistence `ServiceExtension`, and the Identity registration so support can be restored once Pomelo ships an EF Core 10 build (re-add the `Pomelo.EntityFrameworkCore.MySql` package and the `UseMySql(...)` calls).

If you want to use another provider, change the connection string in the `appsettings.json` file and install the proper NuGet package. For example, for PostgreSQL install `Npgsql.EntityFrameworkCore.PostgreSQL` and update the connection string.

You could change the database provider in the HaskoliDbContextFactory class in the Haskoli.Infrastructure.Persistence project changing the line below:

* If you want to use SQLServer, you have to change the line below:

```csharp
optionsBuilder.UseSqlServer(connectionString); //SQLServer
```


## Settings file
You could find the settings file in the Haskoli.Api project called appsettings.json. Here you could change the connection string, the JWT settings and the Swagger settings.
[appsettings](../Haskoli/src/Haskoli.Api/appsettings.json) 

## Entities
You could find the entities in the Domain project called Haskoli.Core in the path \Haskoli.Core\Entities. The approach is to create a folder for each entity and inside of it, create the entity class and the entity configuration class. Remember, here we're using Entity Framework Code First approach. Keep in mind to establish the relationship using the proper conventions. In case you have to add special configurations, you could do it using Fluent API.

### Country entity example
**Country.cs**

```csharp
public class Country : EntityBase<int>
{
    public string NameEs { get; set; }
    public string NameEn { get; set; }
    public string ISO2 { get; set; }
    public string ISO3 { get; set; }

    // Navigation property
}
```


## Migrations Execution
Open a console from the root of the project and execute the following commands:

### Execution for Initial snapshot

```console
cd Haskoli.Infrastructure.Persistence

dotnet ef migrations add InitialModel --context HaskoliDbContext --project Haskoli.Infrastructure.Persistence.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj --output-dir Data/Migrations --verbose
```

### Update EF

```console
dotnet ef database update --context HaskoliDbContext --project Haskoli.Infrastructure.Persistence.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj --verbose
```


### Identity Migrations

### Execution for Initial snapshot
```console
cd Haskoli.Infrastructure.Identity

dotnet ef migrations add InitialIdenity --context HaskoliIdentityDbContext --project Haskoli.Infrastructure.Identity.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj --output-dir Data/Migrations --verbose
```

### Update EF
```console
dotnet ef database update --context HaskoliIdentityDbContext --project Haskoli.Infrastructure.Identity.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj --verbose
```

## Repositories Interface creation
You could find the repositories interfaces in the Domain project called Haskoli.Core in the path \Haskoli.Core\Interfaces\Repository. 

The approach is to create the repository interface inside of it. Remember, here we're using the Repository Pattern. There's no need to add the basic CRUD methods because they're already implemented in the BaseRepository class. Keep in mind to create the proper methods for each entity in case you need extra functionalities.

You must follow the convention "I" + EntitieName + "Repository". 
For example, if you want to create an interface repository for the Country entity, you have to create the interface called *ICountryRepository*.


## Repositories Implementation
You could find the repositories implementation in the Infrastructure project called Haskoli.Infrastructure.Persistence in the path \Haskoli.Infrastructure.Persistence\Repositories.

You must follow the convention EntitieName + "Repository". 

For example, if you want to create a repository for the Country entity, you have to create the interface called *CountryRepository*.



## DTO creation
You could find the DTOs in the Core project called Haskoli.Core in the path \Haskoli.Core\DTO. Those elements are fundamental to implement the CQRS pattern. The implementations of the Services and the Handlers require the use of DTOs.

If you want to perform Read actions, only follow the convention EntityName + "DTO" to create a new one. For example, if you want to create a DTO for the Country entity, you have to create the DTO called *CountryDTO*.

If you want to perform Create, Update or Delete actions, you have to create a DTO for each action following the convention "action" + EntitieName + "DTO". For example, if you want to create a DTO for the Create action for the Country entity, you have to create the DTO called *CreateCountryDTO*.
 

## Services Interface creation
You could find the services interfaces in the Domain project called Haskoli.Core in the path \Haskoli.Core\Interfaces\Services. 

Follow the convention "I" + EntitieName + "Service" to create a new one. For example, if you want to create a service interface for the Country entity, you have to create the interface called *ICountryService*.

## Services Implementation
You could find the services implementation in the Application project called Haskoli.Application in the path \Haskoli.Infrastructure.Common\Services.

Follow the convention EntitieName + "Service" to create a new one. For example, if you want to create a service for the Country entity, you have to create the interface called *CountryService*.

## Dependency Injection for Services and Repositories

You could find the dependency injection configuration in the Infrastructure project called Haskoli.Infrastructure.Common in the path \Haskoli.Infrastructure.Common\ServiceCollection, in the class called *ServiceCollection*, in the method called *AddCommonLayer*, as the example below:

```csharp

public static IServiceCollection AddCommonLayer(this IServiceCollection services)
{

     /* Add your injection dependencies for Repositories here */
    services.AddTransient<ICountryRepository<HaskoliDbContext>, CountryRepository>();

    /* Add your injection dependencies for Services here*/
    services.AddTransient<ICountryService, CountryService>();

    return services;
}
```


## Mappings creation
You could find the mappings in the Core project called Haskoli.Application in the path \Haskoli.Application\Mappings. Find the constructor of the AutoMapperProfile class and add the mapping for each DTO. For example, if you want to create a mapping for the Country entity, you have to add the mapping below:

```csharp
 CreateMap<Country, CountryDTO>().ReverseMap();
 CreateMap<Country, CreateCountryDTO>().ReverseMap();
```

## Handlers and Validators creation
The Handlers and Validators are the classes that implement the CQRS pattern. You could find them in the Application project called Haskoli.Application in the path \Haskoli.Application\Features. 

Each folder represents an entity. Inside of it, you could find the Commands and Queries folders. The commands folder contains the handlers and validators for the Create, Update and Delete actions. The Queries folder contains the handlers and validators for the Read actions. For example, the commands folder for the Country entity contains the following classes:

```console
├──Country
│   ├──Commands
│   │   ├──CreateCountry
│   │   │   ├──CreateCountryCommandValidator.cs
│   │   │   └──CreateCountryCommandHandler.cs
│   │   ├──DeleteCountry
│   │   │   ├──DeleteCountryCommandValidator.cs
│   │   │   └──DeleteCountryCommandHandler.cs
│   │   └──UpdateCountry
│   │       ├──UpdateCountryCommandValidator.cs
│   │       └──UpdateCountryCommandHandler.cs
│   └──Queries
│       ├──GetAllCountriesHandler.cs
│       ├──GetCountrydHandler.cs
│       └──CountryQuery.cs
```

## Controllers creation
You could find the controllers in the Api project called Haskoli.Api in the path \Haskoli.Api\Controllers. 

You must follow the convention EntitieName + "Controller" to create a new one. For example, if you want to create a controller for the Country entity, you have to create the controller called *CountryController*.

In your controller, you must inject the service interface for the entity. For example, if you want to create a controller for the Country entity, you have to inject the *IMediator* interface. 

```csharp
private readonly IMediator _mediator;

public CountryController(IMediator mediator)
{
    _mediator = mediator;
}

[HttpGet]
public async Task<IEnumerable<CountryDTO>> GetCountries() =>
    await _mediator.Send(new GetAllCountryQuery());

```

If you see, the method GetCountries() calls the GetAllCountryQuery() method. This method is the one that calls the handler for the GetAllCountryQuery. The handler is the one that calls the service and the service is the one that calls the repository. The result is the DTO of your entity. In this case, is a read-action DTO, CouyntryDTO.








