# Modular Monolith Backend
**Modular Monolith** Backend developed using **Domain-Driven Design**, CQRS, Postgres, EF Core, MassTransit, custom outbox implementation, and more. 
This is a backend for a demonstration application *"TeamUp"* (team managment application) as part of my [**Master's Thesis**](https://github.com/skrasekmichael/ModularInformationSystemThesis) at [BUT FIT](https://www.fit.vut.cz/.en).

### Run

```bash
# with dotnet 8 sdk
dotnet run --project src/TeamUp.Bootstrapper
```
By default, the application launches at *https://localhost:7089* by default, to change the port, change the `applicationUrl` value in `src/TeamUp.Bootstrapper/Properties/launchSettings.json`.

#### Database
By default, the application expects postgres database instance running at localhost (port 5432), and logs in using `postgres` username and `devpass` password. To change that, change connection string in `Database` section in `src/TeamUp.Bootstrapper/appsettings.json`.

Such instance can be launched using the [scripts/rundb.ps1](scripts/rundb.ps1) script.

#### Event Bus
By default, the application expects rabbitmq instance running at localhost (port 5672). To change that, change connection string in `RabbitMq` section in `src/TeamUp.Bootstrapper/appsettings.json`.

Such instance can be launched using the [scripts/runbus.ps1](scripts/runbus.ps1) script.

#### Client
By default, the application expects the client app ([frontend](https://github.com/skrasekmichael/ModularInformationSystemFrontend)) at *https://localhost:7229*, to change that, change the `Url` value in `Client` section in `src/TeamUp.Bootstrapper/appsettings.json`.
