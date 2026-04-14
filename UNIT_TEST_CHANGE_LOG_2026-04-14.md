# Unit Test Change Log 2026-04-14

## Task Background

This round focused on analyzing libraries under `src/Shared`, checking whether corresponding unit tests existed under `test`, evaluating whether those tests were complete enough for core behaviors, and then adding missing tests.

The user also requested that any newly created unit test projects must be added to `NugetPackages.slnx`.

## Inventory Summary

- `src/Shared` project count at the time of execution: `57`
- `test` project count after this round: `42`
- Shared libraries that now have at least one corresponding test project: `36`
- Shared libraries that still do not have a dedicated corresponding test project: `21`

Remaining projects without dedicated test coverage after this round:

- `Azrng.AspNetCore.Core`
- `Azrng.Cache.Core`
- `Azrng.Cache.FreeRedis`
- `Azrng.DistributeLock.Core`
- `Azrng.SettingConfig`
- `Azrng.SqlMigration`
- `Azrng.Swashbuckle`
- `Azrng.YuntongxunSms`
- `Common.Cache.CSRedis`
- `Common.Dapper`
- `Common.Db.Core`
- `Common.EFCore.InMemory`
- `Common.EFCore.MySQL`
- `Common.EFCore.SQLite`
- `Common.EFCore.SQLServer`
- `Common.EFCore`
- `Common.Email`
- `Common.QRCode`
- `Common.YuQueSdk`
- `Azrng.EventBus.RabbitMQ`
- `StudyUse`

## New Test Projects Added

The following new unit test projects were created:

1. `test/Azrng.AspNetCore.Authentication.Basic.Test/Azrng.AspNetCore.Authentication.Basic.Test.csproj`
2. `test/Azrng.AspNetCore.Authentication.JwtBearer.Test/Azrng.AspNetCore.Authentication.JwtBearer.Test.csproj`
3. `test/Azrng.AspNetCore.Authorization.Default.Test/Azrng.AspNetCore.Authorization.Default.Test.csproj`
4. `test/Azrng.SettingConfig.BasicAuthorization.Test/Azrng.SettingConfig.BasicAuthorization.Test.csproj`
5. `test/Azrng.EventBus.Core.Test/Azrng.EventBus.Core.Test.csproj`
6. `test/Azrng.EventBus.InMemory.Test/Azrng.EventBus.InMemory.Test.csproj`
7. `test/Azrng.Core.Test/Azrng.Core.Test.csproj`
8. `test/Azrng.AspNetCore.Inject.Test/Azrng.AspNetCore.Inject.Test.csproj`
9. `test/Azrng.ConsoleApp.DependencyInjection.Test/Azrng.ConsoleApp.DependencyInjection.Test.csproj`
10. `test/Azrng.AspNetCore.DbEnvConfig.Test/Azrng.AspNetCore.DbEnvConfig.Test.csproj`

## Test Files Added

1. `test/Azrng.AspNetCore.Authentication.Basic.Test/BasicAuthenticationHandlerTests.cs`
2. `test/Azrng.AspNetCore.Authentication.JwtBearer.Test/JwtBearerAuthServiceTests.cs`
3. `test/Azrng.AspNetCore.Authorization.Default.Test/PathBasedAuthorizationTests.cs`
4. `test/Azrng.SettingConfig.BasicAuthorization.Test/BasicAuthAuthorizationFilterTests.cs`
5. `test/Azrng.EventBus.Core.Test/EventBusBuilderExtensionsTests.cs`
6. `test/Azrng.EventBus.InMemory.Test/InMemoryEventBusTests.cs`
7. `test/Azrng.Core.Test/Results/ResultModelTests.cs`
8. `test/Azrng.Core.Test/CommonDto/RefAsyncTests.cs`
9. `test/Azrng.Core.Test/Exceptions/ExceptionTests.cs`
10. `test/Azrng.Core.Test/Extension/CollectionAndByteExtensionsTests.cs`
11. `test/Azrng.Core.Test/Helpers/StringObjectAndFileHelperTests.cs`
12. `test/Azrng.Core.Test/Helpers/UrlAndSqlHelperTests.cs`
13. `test/Azrng.Core.Test/Helpers/TaskHelperTests.cs`
14. `test/Azrng.Core.Test/Extension/DateTimeExtensionTests.cs`
15. `test/Azrng.Core.Test/Helpers/ExpressionAndPredicateTests.cs`
16. `test/Azrng.Core.Test/Helpers/SnowflakeTests.cs`
17. `test/Azrng.Core.Test/Extension/AssemblyExtensionsTests.cs`
18. `test/Azrng.Core.Test/Helpers/RandomArraySelectorTests.cs`
19. `test/Azrng.AspNetCore.Inject.Test/ServiceCollectionExtensionsTests.cs`
20. `test/Azrng.ConsoleApp.DependencyInjection.Test/ConsoleAppServerTests.cs`
21. `test/Azrng.AspNetCore.DbEnvConfig.Test/DbConfigurationProviderTests.cs`

## Coverage Added By Area

### 1. Azrng.AspNetCore.Authentication.Basic

Covered behaviors:

- missing `Authorization` header
- non-Basic authorization scheme
- invalid Base64 credentials
- invalid username or password
- successful authentication and custom claims creation
- `ChallengeAsync` response payload verification
- `ForbidAsync` response payload verification
- service registration through `AddBasicAuthentication`

Test count:

- `net8.0`: 8 passed
- `net9.0`: 8 passed

### 2. Azrng.AspNetCore.Authentication.JwtBearer

Covered behaviors:

- token creation and validation round-trip
- extracting `NameIdentifier`
- reading payload information through `GetJwtInfo`
- failure on tampered token
- failure when validating with another secret
- secret length validation
- secret complexity validation
- DI/options registration through `AddJwtBearerAuthentication`

Test count:

- `net8.0`: 7 passed
- `net9.0`: 7 passed

### 3. Azrng.AspNetCore.Authorization.Default

Covered behaviors:

- default policy provider registration
- named policy registration
- fallback policy behavior
- anonymous path authorization
- unauthorized protected path rejection
- permission denied rejection
- permission granted success path

Test count:

- `net8.0`: 5 passed
- `net9.0`: 5 passed

### 4. Azrng.SettingConfig.BasicAuthorization

Covered behaviors:

- `BasicAuthAuthorizationUser` password hashing
- correct credential validation
- case-sensitive and case-insensitive login behavior
- exception behavior for blank login or password
- HTTPS redirect behavior
- SSL-required challenge behavior
- invalid authorization header handling
- passwords containing colon (`:`)

Test count:

- `net8.0`: 7 passed
- `net9.0`: 7 passed

### 5. Azrng.EventBus.Core

Covered behaviors:

- readable generic type name formatting
- JSON serializer option configuration
- manual subscription registration
- automatic subscription scanning and registration

Test count:

- `net8.0`: 4 passed
- `net9.0`: 4 passed

### 6. Azrng.EventBus.InMemory

Covered behaviors:

- dispatch to multiple handlers
- exception isolation when one handler fails
- ignoring unknown event types
- DI registration through `AddInMemoryEventBus`

Test count:

- `net8.0`: 4 passed
- `net9.0`: 4 passed

### 7. Azrng.Core

This check confirmed that coverage was not complete before this round.

Previous state:

- there was no dedicated `Azrng.Core.Test` project
- the existing `Azrng.Core.DefaultJson.Test` and `Azrng.Core.NewtonsoftJson.Test` projects only covered serializer packages, not the `Azrng.Core` main library itself

Covered behaviors added in this round:

- `ResultModel` and `ResultModel<T>` default state and factory methods
- `RefAsync<T>` implicit conversions and `ToString`
- `BaseException`, `ParameterException`, and `RetryMarkException` constructor and throw behavior
- `CollectionExtensions.AddIfNotContains`
- `ByteExtensions` conversions, file type detection, and file persistence
- `DateTimeExtension` formatting, timestamp, calendar, and date-diff helpers
- `StringHelper` compression, Unicode conversion, and replacement helpers
- `ObjectHelper` string-property extraction and target-type conversion
- `FileHelper.FormatFileSize`
- `UrlHelper` query sorting, URL extraction, query parsing, and query merging
- `SqlHelper` injection detection and safe-input checks
- `PredicateExtensions` and `Expressionable<T>` expression composition
- `AssemblyExtensions` assembly metadata and validation helpers
- `RandomArraySelector<T>` cycle behavior
- `Snowflake` ID generation and parsing
- `TaskHelper` timeout handling, polling, and retry execution

Test count:

- `net8.0`: 62 passed
- `net9.0`: 62 passed

### 8. Azrng.AspNetCore.Inject

This project did not have a dedicated corresponding test project before this continuation round.

Covered behaviors:

- invalid startup module type rejection
- circular module dependency detection
- module registration into the service collection
- child-before-parent module initialization order
- passing configuration into `ServiceContext`
- module-level service registration during `ConfigureServices`
- automatic service registration for self registration, interface registration, base-class registration, and explicit `ServicesType` registration
- duplicate registration prevention when the same assembly is scanned through multiple dependent modules

Test count:

- `net8.0`: 4 passed
- `net9.0`: 4 passed

### 9. Azrng.ConsoleApp.DependencyInjection

This project did not have a dedicated corresponding test project before this continuation round.

Covered behaviors:

- command-line configuration loading
- registration of `IConfiguration` and logging services
- replacing an existing `IServiceStart` registration during `Build<TStart>()`
- delegate-based service registration through `Build<TStart>(Action<IServiceCollection>)`
- running the start service inside a created scope
- rethrowing unhandled exceptions from `RunAsync`
- logger provider and minimum log-level behavior
- console title output formatting through `ConsoleTool`

Test count:

- `net8.0`: 6 passed
- `net9.0`: 6 passed

### 10. Azrng.AspNetCore.DbEnvConfig

This project did not have a dedicated corresponding test project before this continuation round.

Covered behaviors:

- `DBConfigOptions.ParamVerify` schema parsing and full table name normalization
- validation failures for missing connection factory, invalid table name format, and blank key-field configuration
- null-checks in `AddDbConfiguration`
- schema-aware default table initialization script generation
- loading plain database values into configuration
- flattening JSON objects and arrays into configuration key paths
- falling back to the raw string when JSON text is invalid
- skipping rows with null keys or null values
- executing table initialization and seed scripts when the target table is empty

Test count:

- `net8.0`: 6 passed
- `net9.0`: 6 passed

## Source Code Fixes Made

One production-code change was made while adding tests:

- File: `src/Shared/Azrng.SettingConfig.BasicAuthorization/BasicAuthAuthorizationFilter.cs`
- Change:
  - replaced the previous redirect flow with `context.Response.Redirect(redirectUri, permanent: true);`
- Reason:
  - the test for SSL redirect exposed that the implementation intended to perform a permanent redirect, but the response was being overwritten by the framework redirect helper; using the explicit permanent redirect API makes the behavior correct and stable.

Additional production-code fixes made while expanding `Azrng.Core` tests:

- File: `src/Shared/Azrng.Core/Helpers/Expressionable.cs`
- Change:
  - fixed parameter replacement when combining expressions in `And` and `Or`
- Reason:
  - newly added tests exposed that chaining multiple expressions produced an invalid expression tree because parameters from different lambdas were not unified.

- File: `src/Shared/Azrng.Core/Helpers/Snowflake.cs`
- Change:
  - fixed initialization of `_msStart` in `Init()`
- Reason:
  - newly added tests exposed that the timestamp offset was not initialized because `_watch` was already non-null, which could make parsed time values fall back to the base date instead of the current timeline.

Additional production-code fixes made while expanding `Azrng.AspNetCore.Inject` tests:

- File: `src/Shared/Azrng.AspNetCore.Inject/ServiceCollectionExtensions.cs`
- Change:
  - changed module dependency discovery from subclass-only filtering to `OfType<InjectModuleAttribute>()`
- Reason:
  - the newly added tests exposed that the non-generic `InjectModuleAttribute` was being ignored, which meant dependent modules were not registered and circular dependency checks did not run for those declarations.

## Solution File Changes

The new test projects were added to solution files so they are visible in IDEs and available for later execution.

### Updated `AspNetCoreNugetStudy.sln`

Added test projects:

- `Azrng.AspNetCore.Authentication.Basic.Test`
- `Azrng.AspNetCore.Authentication.JwtBearer.Test`
- `Azrng.AspNetCore.Authorization.Default.Test`
- `Azrng.SettingConfig.BasicAuthorization.Test`
- `Azrng.EventBus.Core.Test`
- `Azrng.EventBus.InMemory.Test`

### Updated `NugetPackages.slnx`

Confirmed entries added for:

- `test/Azrng.AspNetCore.Authentication.Basic.Test/Azrng.AspNetCore.Authentication.Basic.Test.csproj`
- `test/Azrng.AspNetCore.Authentication.JwtBearer.Test/Azrng.AspNetCore.Authentication.JwtBearer.Test.csproj`
- `test/Azrng.AspNetCore.Authorization.Default.Test/Azrng.AspNetCore.Authorization.Default.Test.csproj`
- `test/Azrng.SettingConfig.BasicAuthorization.Test/Azrng.SettingConfig.BasicAuthorization.Test.csproj`
- `test/Azrng.EventBus.Core.Test/Azrng.EventBus.Core.Test.csproj`
- `test/Azrng.EventBus.InMemory.Test/Azrng.EventBus.InMemory.Test.csproj`
- `test/Azrng.Core.Test/Azrng.Core.Test.csproj`
- `test/Azrng.AspNetCore.Inject.Test/Azrng.AspNetCore.Inject.Test.csproj`
- `test/Azrng.ConsoleApp.DependencyInjection.Test/Azrng.ConsoleApp.DependencyInjection.Test.csproj`
- `test/Azrng.AspNetCore.DbEnvConfig.Test/Azrng.AspNetCore.DbEnvConfig.Test.csproj`

## Verification Commands Executed

The following commands were executed successfully:

```powershell
dotnet test test/Azrng.AspNetCore.Authentication.Basic.Test/Azrng.AspNetCore.Authentication.Basic.Test.csproj --framework net8.0
dotnet test test/Azrng.AspNetCore.Authentication.Basic.Test/Azrng.AspNetCore.Authentication.Basic.Test.csproj --framework net9.0

dotnet test test/Azrng.AspNetCore.Authentication.JwtBearer.Test/Azrng.AspNetCore.Authentication.JwtBearer.Test.csproj --framework net8.0
dotnet test test/Azrng.AspNetCore.Authentication.JwtBearer.Test/Azrng.AspNetCore.Authentication.JwtBearer.Test.csproj --framework net9.0

dotnet test test/Azrng.AspNetCore.Authorization.Default.Test/Azrng.AspNetCore.Authorization.Default.Test.csproj --framework net8.0
dotnet test test/Azrng.AspNetCore.Authorization.Default.Test/Azrng.AspNetCore.Authorization.Default.Test.csproj --framework net9.0

dotnet test test/Azrng.SettingConfig.BasicAuthorization.Test/Azrng.SettingConfig.BasicAuthorization.Test.csproj --framework net8.0
dotnet test test/Azrng.SettingConfig.BasicAuthorization.Test/Azrng.SettingConfig.BasicAuthorization.Test.csproj --framework net9.0

dotnet test test/Azrng.EventBus.Core.Test/Azrng.EventBus.Core.Test.csproj --framework net8.0
dotnet test test/Azrng.EventBus.Core.Test/Azrng.EventBus.Core.Test.csproj --framework net9.0

dotnet test test/Azrng.EventBus.InMemory.Test/Azrng.EventBus.InMemory.Test.csproj --framework net8.0
dotnet test test/Azrng.EventBus.InMemory.Test/Azrng.EventBus.InMemory.Test.csproj --framework net9.0

dotnet test test/Azrng.Core.Test/Azrng.Core.Test.csproj --framework net8.0
dotnet test test/Azrng.Core.Test/Azrng.Core.Test.csproj --framework net9.0

dotnet test test/Azrng.AspNetCore.Inject.Test/Azrng.AspNetCore.Inject.Test.csproj
dotnet test test/Azrng.ConsoleApp.DependencyInjection.Test/Azrng.ConsoleApp.DependencyInjection.Test.csproj
dotnet test test/Azrng.AspNetCore.DbEnvConfig.Test/Azrng.AspNetCore.DbEnvConfig.Test.csproj
```

## Important Notes

- Some existing projects emit warnings during build. These warnings were already present in the referenced projects and were not introduced by the newly added tests.
- `Azrng.Swashbuckle` was analyzed during this round, but no test project was created yet because the current source file for `FileUploadOperationFilter` is commented out and does not provide a stable, testable active implementation.
- External-service-heavy libraries were intentionally deprioritized in favor of pure unit-testable components first.

## Recommended Next Batch

If the repository continues this effort, the next reasonable batch is:

1. `Azrng.Swashbuckle`
2. `Azrng.SqlMigration`
3. `Common.Db.Core`
4. `Common.QRCode`
5. `Azrng.Cache.Core`
6. `Azrng.Cache.FreeRedis`

These are likely to provide the best next step between value and testability.
