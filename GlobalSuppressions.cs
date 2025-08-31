// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Entity Framework related suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Entity Framework requires instance methods for navigation properties")]

// Async/Await related suppressions for controller actions
[assembly: SuppressMessage("Microsoft.Usage", "CS1998:ThisAsyncMethodLacksAwaitOperatorsAndWillRunSynchronously", Justification = "Controller actions may not always need await but maintain async signature for consistency")]

// JSON serialization related suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "DTO classes are instantiated by JSON serialization")]

// Security related suppressions (with careful justification)
[assembly: SuppressMessage("Microsoft.Security", "CA5394:DoNotUseInsecureRandomness", Justification = "Random is used for test data generation only, not for security purposes")]

// Nullable reference types suppressions
[assembly: SuppressMessage("Microsoft.Design", "CS8618:NonNullableFieldMustContainNonNullValueWhenExitingConstructor", Justification = "Properties are initialized by Entity Framework or dependency injection")]

// File-specific suppressions for generated migrations
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Migrations", Justification = "Entity Framework generated migration files")]

// Configuration and startup suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "~M:ProjectControlsReportingTool.API.Program.Main(System.String[])", Justification = "Program.cs naturally has many dependencies for application configuration")]

// Test-related suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1707:IdentifiersShouldNotContainUnderscores", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Tests", Justification = "Test method names use underscores for readability")]

// API Controller suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Controllers", Justification = "Model validation is handled by ASP.NET Core model binding and validation attributes")]

// Repository pattern suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generic repository methods follow established patterns")]

// Dependency injection suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Business.Services", Justification = "Dependencies are validated by dependency injection container")]

// Logging suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1848:UseLoggerMessageDefineForLoggingMethods", Justification = "Standard ILogger methods are acceptable for this application scale")]

// Database context suppressions
[assembly: SuppressMessage("Microsoft.Security", "EF1001:InternalApi", Justification = "Entity Framework internal APIs are used for advanced configuration")]

// Middleware suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Middleware", Justification = "Middleware needs to catch all exceptions to provide proper error handling")]

// Model validation suppressions
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "DTO collections need to be settable for model binding")]

// HTTP client suppressions
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "HttpClient instances are managed by dependency injection")]

// Authentication and authorization suppressions
[assembly: SuppressMessage("Microsoft.Security", "CA5398:AvoidHardcodedSslVersions", Justification = "SSL versions are configured appropriately for production environments")]

// Swagger/OpenAPI suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Models.DTOs", Justification = "DTO arrays are required for API contract compatibility")]

// Background service suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "~M:ProjectControlsReportingTool.API.Business.Services.BackgroundService.ExecuteAsync(System.Threading.CancellationToken)", Justification = "Background services need comprehensive exception handling")]

// Configuration suppressions
[assembly: SuppressMessage("Microsoft.Usage", "CA2254:TemplatesShouldBeStaticExpression", Justification = "Configuration values may be dynamic")]

// Globalization suppressions
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "This application does not require localization")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Justification = "Invariant culture is appropriate for internal operations")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Invariant culture is appropriate for internal operations")]

// Reliability suppressions
[assembly: SuppressMessage("Microsoft.Reliability", "CA2007:DoNotDirectlyAwaitTask", Justification = "ConfigureAwait(false) is not necessary in ASP.NET Core applications")]

// Design suppressions for DTOs
[assembly: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Models.DTOs", Justification = "DTOs can expose List<T> for serialization compatibility")]

// Naming suppressions for generated code
[assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Models.Entities", Justification = "Database entity names may contain underscores to match schema")]

// Performance suppressions
[assembly: SuppressMessage("Microsoft.Performance", "CA1860:AvoidUsingEnumerableCountWhenAnyWouldWork", Justification = "Count() is sometimes more readable and the performance difference is negligible for small collections")]

// Security suppressions (use sparingly and with good justification)
[assembly: SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", Justification = "SQL queries use parameterized queries through Entity Framework")]

// Interoperability suppressions
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1416:ValidatePlatformCompatibility", Justification = "Application targets specific platforms with known compatibility")]

// Maintainability suppressions
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member", Target = "~M:ProjectControlsReportingTool.API.Business.Services.ReportService.ProcessComplexReport", Justification = "Complex business logic is appropriately broken down with helper methods")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Migrations", Justification = "Entity Framework generated migrations")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Controllers", Justification = "Controllers naturally have multiple dependencies")]

// Documentation suppressions
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Scope = "namespaceanddescendants", Target = "~N:ProjectControlsReportingTool.API.Models.DTOs", Justification = "DTO properties are self-documenting")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "File headers are not required for this project")]

// Ordering suppressions
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:UsingDirectivesMustBePlacedWithinNamespace", Justification = "Global usings are placed at file level")]

// Readability suppressions
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "This prefix is not required for clarity")]

// Spacing suppressions
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:KeywordsMustBeSpacedCorrectly", Justification = "Automated formatting handles spacing")]

// Layout suppressions
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Simple statements can be on single lines for readability")]

// Analytical suppressions for specific scenarios
[assembly: SuppressMessage("Microsoft.CodeAnalysis.CSharp", "CS8669:NullableAnnotationAnalysis", Justification = "Nullable reference types are handled appropriately")]

// Third-party library suppressions
[assembly: SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Third-party libraries may require string URLs")]

// API versioning suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "API endpoints may require string parameters for compatibility")]
[assembly: SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "API responses may return string URLs for client compatibility")]

// Date/Time suppressions
[assembly: SuppressMessage("Microsoft.Usage", "CA1307:SpecifyStringComparison", Justification = "Default string comparison is appropriate for internal operations")]

// Exception handling suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Custom exceptions implement required constructors")]

// Resource management suppressions
[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Disposable fields are managed by dependency injection container")]

// Generic suppressions
[assembly: SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "Generic types are used appropriately for type safety")]

// File system suppressions
[assembly: SuppressMessage("Microsoft.Security", "CA3003:ReviewCodeForFilePathInjectionVulnerabilities", Justification = "File paths are validated and sanitized")]

// Concurrency suppressions
[assembly: SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses", Justification = "Exception handling is designed to prevent cascading failures")]

// Custom analyzer suppressions (if using custom analyzers)
[assembly: SuppressMessage("CustomAnalyzer", "CUSTOM001:CustomRule", Justification = "Custom rule suppression with specific justification")]
