# Unit Test Change Log 2026-04-14

## Task Background

This round focused on analyzing libraries under `src/Shared`, checking whether corresponding unit tests existed under `test`, evaluating whether those tests were complete enough for core behaviors, and then adding missing tests.

The user also requested that any newly created unit test projects must be added to `NugetPackages.slnx`.

## Inventory Summary

- `src/Shared` project count at the time of execution: `57`
- `test` project count after this round: `45`
- Shared libraries that now have at least one corresponding test project: `39`
- Shared libraries that still do not have a dedicated corresponding test project: `18`

Remaining projects without dedicated test coverage after this round:

- `Azrng.Cache.Core`
- `Azrng.Cache.FreeRedis`
- `Azrng.DistributeLock.Core`
- `Azrng.SettingConfig`
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
11. `test/Azrng.SqlMigration.Test/Azrng.SqlMigration.Test.csproj`
12. `test/Common.QRCode.Test/Common.QRCode.Test.csproj`
13. `test/Azrng.AspNetCore.Core.Test/Azrng.AspNetCore.Core.Test.csproj`

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
22. `test/Azrng.SqlMigration.Test/SqlMigrationTests.cs`
23. `test/Common.QRCode.Test/QrCodeHelpTests.cs`
24. `test/Azrng.AspNetCore.Core.Test/CoreFeatureTests.cs`

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

### 11. Azrng.SqlMigration

This project did not have a dedicated corresponding test project before this continuation round.

Covered behaviors:

- rejecting blank migration names
- rejecting duplicate migration-name registrations
- rejecting `AddAutoMigration()` when no migration configuration exists
- registering named options, keyed migration handlers, keyed init-version setters, and startup filters
- `SqlVersionLogOption.OrderByColumn` fallback behavior
- end-to-end migration execution through `SqlMigrationStartupFilter`
- lock-provider execution and disposal around migration
- selecting only newer SQL scripts and applying them in version order
- transaction commit on successful script execution
- rollback and failure callbacks when a script throws
- validating `PgSqlDbVersionService` configuration when a custom `OrderByColumn` requires custom table-init SQL

Test count:

- `net8.0`: 7 passed
- `net9.0`: 7 passed

### 12. Common.QRCode

This project did not have a dedicated corresponding test project before this continuation round.

Covered behaviors:

- service registration through `AddQrCode`
- generating QR codes as BMP byte arrays
- generating CODE_128 barcodes as BMP byte arrays
- honoring requested output image dimensions for both QR code and barcode generation

Test count:

- `net8.0`: 5 passed

### 13. Azrng.AspNetCore.Core

This project did not have a dedicated corresponding test project before this continuation round.

Covered behaviors:

- `CollectionNotEmptyAttribute` validation for empty and non-empty enumerables
- `MinValueAttribute` validation across multiple numeric types
- `LongToStringConverter` string-based serialization and dual-mode deserialization
- ordered execution in `PreConfigureActionList<TOptions>`
- object accessor registration, retrieval, and duplicate-registration protection
- reuse of pre-configure action lists via `PreConfigure` and `GetPreConfigureActions`
- permissive CORS policy registration through `AddAnyCors`
- origin-restricted CORS registration through `AddCorsByOrigins`
- snapshotting service metadata through `AddShowAllServices`
- custom invalid-model-state response creation through `AddMvcModelVerifyFilter`

Test count:

- `net8.0`: 10 passed
- `net9.0`: 10 passed

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
- `test/Azrng.SqlMigration.Test/Azrng.SqlMigration.Test.csproj`
- `test/Common.QRCode.Test/Common.QRCode.Test.csproj`
- `test/Azrng.AspNetCore.Core.Test/Azrng.AspNetCore.Core.Test.csproj`

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
dotnet test test/Azrng.SqlMigration.Test/Azrng.SqlMigration.Test.csproj
dotnet test test/Common.QRCode.Test/Common.QRCode.Test.csproj
dotnet test test/Azrng.AspNetCore.Core.Test/Azrng.AspNetCore.Core.Test.csproj
```

## Important Notes

- Some existing projects emit warnings during build. These warnings were already present in the referenced projects and were not introduced by the newly added tests.
- `Azrng.Swashbuckle` was analyzed during this round, but no test project was created yet because the current source file for `FileUploadOperationFilter` is commented out and does not provide a stable, testable active implementation.
- External-service-heavy libraries were intentionally deprioritized in favor of pure unit-testable components first.
- `Common.QRCode` currently references `System.Drawing.Common` `5.0.0`, and the restore/test output reports an existing critical vulnerability advisory for that dependency. This warning comes from the source project dependency graph and was not introduced by the tests.

## Recommended Next Batch

If the repository continues this effort, the next reasonable batch is:

1. `Azrng.Swashbuckle`
2. `Common.Db.Core`
3. `Azrng.Cache.Core`
4. `Azrng.Cache.FreeRedis`
5. `Azrng.SettingConfig`
6. `Common.Email`

These are likely to provide the best next step between value and testability.

## Continuation Round: Common.Db.Core

This continuation round focused on `src/Shared/Common.Db.Core`, which previously had no dedicated corresponding test project.

### New Test Project Added

- `test/Common.Db.Core.Test/Common.Db.Core.Test.csproj`

### Test Files Added

- `test/Common.Db.Core.Test/CommonDto/RefAsyncTests.cs`
- `test/Common.Db.Core.Test/CommonDto/PredicateExpressionVisitorTests.cs`
- `test/Common.Db.Core.Test/Extensions/ExpressExtensionsTests.cs`
- `test/Common.Db.Core.Test/Extensions/QueryableExtensionsTests.cs`
- `test/Common.Db.Core.Test/Requests/RequestAndResultTests.cs`

### Coverage Added For Common.Db.Core

Covered behaviors:

- `RefAsync<T>` implicit conversion and `ToString`
- `PredicateExpressionVisitor` parameter replacement
- `ExpressExtensions.MarkEqual`
- `ExpressExtensions.MergeAnd` and `MergeOr` for single and multi-predicate composition
- `QueryableExtensions.SelectMapper`
- `QueryableExtensions.OrderBy` for lambda, `SortContent`, string field name, and multi-field ordering
- `QueryableExtensions.PagedBy` overloads and `CountBy`
- `QueryableExtensions.WhereAny`
- `QueryableExtensions.EqualWhere`, `LessWhere`, and `GreaterWhere`
- request/result DTO constructors and pagination metadata calculations

Test count:

- `net8.0`: 19 passed
- `net9.0`: 19 passed

### Additional Production Fixes Made

Two production-code defects were exposed and corrected while adding these tests:

- File: `src/Shared/Common.Db.Core/Extensions/ExpressExtensions.cs`
- Change:
  - fixed `MergeAnd` and `MergeOr` multi-expression overloads so they no longer dereference a null intermediate expression and they retain the incoming base predicate when one is supplied
- Reason:
  - the newly added tests exposed that both overloads threw `NullReferenceException` before they could combine the expressions

- File: `src/Shared/Common.Db.Core/Extensions/QueryableExtensions.cs`
- Change:
  - corrected `GreaterWhere` to use the greater-than comparison branch instead of the less-than branch
- Reason:
  - the newly added comparison helper tests exposed that `GreaterWhere` returned records below the threshold instead of above it

Additional quality cleanup made during this round:

- File: `src/Shared/Common.Db.Core/CommonDto/RefAsync.cs`
- Change:
  - initialized `Value` with `default!`
- Reason:
  - this removes the nullable warning from the parameterless constructor while preserving the existing API shape

### Solution File Changes

Confirmed entry added to `NugetPackages.slnx`:

- `test/Common.Db.Core.Test/Common.Db.Core.Test.csproj`

### Verification Commands Executed

The following command was executed successfully:

```powershell
dotnet test test/Common.Db.Core.Test/Common.Db.Core.Test.csproj
```

### Inventory Update After This Continuation

Remaining projects without dedicated corresponding test coverage after adding `Common.Db.Core`:

- `Azrng.Cache.Core`
- `Azrng.Cache.FreeRedis`
- `Azrng.DistributeLock.Core`
- `Azrng.SettingConfig`
- `Azrng.Swashbuckle`
- `Azrng.YuntongxunSms`
- `Common.Cache.CSRedis`
- `Common.Dapper`
- `Common.EFCore.InMemory`
- `Common.EFCore.MySQL`
- `Common.EFCore.SQLite`
- `Common.EFCore.SQLServer`
- `Common.EFCore`
- `Common.Email`
- `Common.YuQueSdk`
- `Azrng.EventBus.RabbitMQ`
- `StudyUse`

### Recommended Next Batch

Based on testability after finishing `Common.Db.Core`, the next best candidates are:

1. `Azrng.SettingConfig`
2. `Azrng.Cache.Core`
3. `Azrng.Swashbuckle`
4. `Common.Email`

## Continuation Round: Azrng.SettingConfig

This continuation round focused on `src/Shared/Azrng.SettingConfig`, which previously had no dedicated corresponding test project beyond the already covered `Azrng.SettingConfig.BasicAuthorization` subpackage.

### New Test Project Added

- `test/Azrng.SettingConfig.Test/Azrng.SettingConfig.Test.csproj`

### Test Files Added

- `test/Azrng.SettingConfig.Test/TestInfrastructure.cs`
- `test/Azrng.SettingConfig.Test/DashboardOptionsAndFiltersTests.cs`
- `test/Azrng.SettingConfig.Test/ContextAndWrapperTests.cs`
- `test/Azrng.SettingConfig.Test/MiddlewareAndBuilderTests.cs`
- `test/Azrng.SettingConfig.Test/ServiceRegistrationTests.cs`

### Coverage Added For Azrng.SettingConfig

Covered behaviors:

- `DashboardOptions` default values and parameter validation
- default local-only authorization filter registration and local request authorization behavior
- `AspNetCoreDashboardContextExtensions.GetHttpContext`
- `AspNetCoreDashboardRequest` query, path, IP, and form access
- `AspNetCoreDashboardResponse` content type, status code, body, and write behavior
- `ManifestResourceService` embedded dashboard resource loading
- `AspNetCoreDashboardMiddleware` redirect flow, unauthorized flow, authorized HTML rendering, and security headers
- `ApplicationBuilderExtensions.UseSettingDashboard` data source initialization and middleware registration
- `ServiceCollectionExtensions.AddSettingConfig` options binding, route prefix propagation, and service registration
- `DefaultConnectInterface` default callback behavior

Test count:

- `net8.0`: 17 passed
- `net9.0`: 17 passed

### Additional Production Fixes Made

This round exposed and corrected several null-safety gaps in the dashboard request/response wrappers:

- File: `src/Shared/Azrng.SettingConfig/Dto/AspNetCoreDashboardRequest.cs`
- Change:
  - made `Path`, `PathBase`, `LocalIpAddress`, `RemoteIpAddress`, and `GetQuery` return empty strings instead of throwing when ASP.NET Core provides null backing values
- Reason:
  - the new wrapper tests exposed that requests without connection IPs could crash authorization and request inspection paths with `NullReferenceException`

- File: `src/Shared/Azrng.SettingConfig/Dto/AspNetCoreDashboardResponse.cs`
- Change:
  - made the `ContentType` getter null-safe
- Reason:
  - this keeps the response wrapper stable before ASP.NET Core initializes a response content type

- File: `src/Shared/Azrng.SettingConfig/AspNetCoreDashboardMiddleware.cs`
- Change:
  - normalized request path reads with an empty-string fallback
- Reason:
  - this avoids nullable path issues when matching dashboard routes

Additional quality cleanup made during this round:

- File: `src/Shared/Azrng.SettingConfig/DashboardContext.cs`
- Change:
  - initialized `Request` and `Response` with `default!`
- Reason:
  - this removes constructor exit nullable warnings while preserving the current inheritance pattern

- File: `src/Shared/Azrng.SettingConfig/Dto/UpdateConfigDetailsRequest.cs`
- Change:
  - initialized `Description` and `Value` with `default!`
- Reason:
  - this removes nullable warnings without changing validation behavior

### Solution File Changes

Confirmed entry added to `NugetPackages.slnx`:

- `test/Azrng.SettingConfig.Test/Azrng.SettingConfig.Test.csproj`

### Verification Commands Executed

The following command was executed successfully:

```powershell
dotnet test test/Azrng.SettingConfig.Test/Azrng.SettingConfig.Test.csproj
```

### Inventory Update After This Continuation

Remaining projects without dedicated corresponding test coverage after adding `Azrng.SettingConfig`:

- `Azrng.Cache.Core`
- `Azrng.Cache.FreeRedis`
- `Azrng.DistributeLock.Core`
- `Azrng.Swashbuckle`
- `Azrng.YuntongxunSms`
- `Common.Cache.CSRedis`
- `Common.Dapper`
- `Common.EFCore.InMemory`
- `Common.EFCore.MySQL`
- `Common.EFCore.SQLite`
- `Common.EFCore.SQLServer`
- `Common.EFCore`
- `Common.Email`
- `Common.YuQueSdk`
- `Azrng.EventBus.RabbitMQ`
- `StudyUse`

### Recommended Next Batch

Based on testability after finishing `Azrng.SettingConfig`, the next best candidates are:

1. `Azrng.Cache.Core`
2. `Azrng.Swashbuckle`
3. `Common.Email`
4. `Azrng.Cache.FreeRedis`

## Continuation Round: Azrng.Swashbuckle

This continuation round focused on `src/Shared/Azrng.Swashbuckle`, which previously had no dedicated corresponding test project.

### New Test Project Added

- `test/Azrng.Swashbuckle.Test/Azrng.Swashbuckle.Test.csproj`

### Test Files Added

- `test/Azrng.Swashbuckle.Test/ApplicationBuilderExtensionsTests.cs`
- `test/Azrng.Swashbuckle.Test/ServiceCollectionExtensionsTests.cs`

### Coverage Added For Azrng.Swashbuckle

Covered behaviors:

- `SetUrlEditable` script injection and preservation of existing `HeadContent`
- `UseDefaultSwagger` behavior when the development-only switch is enabled and the environment variable is missing
- `UseDefaultSwagger` production short-circuit behavior
- `UseDefaultSwagger` custom swagger and swagger-ui setup callbacks
- `AddDefaultSwaggerGen` default title overload
- `AddDefaultSwaggerGen` explicit `OpenApiInfo` overload
- JWT security definition and requirement registration
- custom swagger document registration via the optional configuration callback

Test count:

- `net8.0`: 8 passed
- `net9.0`: 8 passed

### Additional Production Fixes Made

One production-code defect was exposed and corrected while adding these tests:

- File: `src/Shared/Azrng.Swashbuckle/IApplicationBuilderExtensions.cs`
- Change:
  - replaced the null-forgiving environment variable comparison with a null-safe `string.Equals` check
- Reason:
  - the new tests exposed that `UseDefaultSwagger(onlyDevelopmentEnabled: true)` threw `NullReferenceException` when `ASPNETCORE_ENVIRONMENT` was not set

### Solution File Changes

Confirmed entry added to `NugetPackages.slnx`:

- `test/Azrng.Swashbuckle.Test/Azrng.Swashbuckle.Test.csproj`

### Verification Commands Executed

The following command was executed successfully:

```powershell
dotnet test test/Azrng.Swashbuckle.Test/Azrng.Swashbuckle.Test.csproj
```

### Inventory Update After This Continuation

Remaining projects without dedicated corresponding test coverage after adding `Azrng.Swashbuckle`:

- `Azrng.Cache.Core`
- `Azrng.Cache.FreeRedis`
- `Azrng.DistributeLock.Core`
- `Azrng.YuntongxunSms`
- `Common.Cache.CSRedis`
- `Common.Dapper`
- `Common.EFCore.InMemory`
- `Common.EFCore.MySQL`
- `Common.EFCore.SQLite`
- `Common.EFCore.SQLServer`
- `Common.EFCore`
- `Common.Email`
- `Common.YuQueSdk`
- `Azrng.EventBus.RabbitMQ`
- `StudyUse`

### Recommended Next Batch

Based on testability after finishing `Azrng.Swashbuckle`, the next best candidates are:

1. `Common.Email`
2. `Azrng.Cache.FreeRedis`
3. `Azrng.Cache.Core`
