# Baubit.DI.Autofac

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.DI.Autofac/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.DI.Autofac)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.DI.Autofac.svg)](https://www.nuget.org/packages/Baubit.DI.Autofac/)
[![NuGet](https://img.shields.io/nuget/dt/Baubit.DI.Autofac.svg)](https://www.nuget.org/packages/Baubit.DI.Autofac) <br/>
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)<br/>
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.DI.Autofac/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.DI.Autofac)

Autofac support for [Baubit.DI](https://github.com/pnagoorkar/Baubit.DI) modular dependency injection framework.

## Table of Contents

- [Installation](#installation)
- [Overview](#overview)
- [Security](#security)
- [Quick Start](#quick-start)
  - [1. Define a Configuration](#1-define-a-configuration)
  - [2. Create an Autofac Module](#2-create-an-autofac-module)
- [Application Creation Patterns](#application-creation-patterns)
  - [Pattern 1: Modules from appsettings.json](#pattern-1-modules-from-appsettingsjson)
  - [Pattern 2: Modules from Code (IComponent)](#pattern-2-modules-from-code-icomponent)
  - [Pattern 3: Hybrid Loading](#pattern-3-hybrid-loading-appsettingsjson--icomponent)
  - [Pattern 4: Mixed Autofac and Standard DI Modules](#pattern-4-mixed-autofac-and-standard-di-modules)
- [Module Configuration in appsettings.json](#module-configuration-in-appsettingsjson)
- [Consumer Module Registration](#consumer-module-registration)
- [API Reference](#api-reference)
- [License](#license)

## Installation

```bash
dotnet add package Baubit.DI.Autofac
```

## Overview

Baubit.DI.Autofac extends Baubit.DI's modular dependency injection framework with Autofac container support. It enables:

- Module-based service registration using Autofac's `ContainerBuilder`
- Configuration-driven module composition
- Hybrid loading from both configuration files and code
- **Mixed module support**: Use both Autofac modules and standard DI modules in the same application
- Full access to Autofac's advanced features (interceptors, decorators, lifetime scopes)

## Security

Baubit.DI.Autofac uses compile-time module discovery to eliminate remote code execution (RCE) vulnerabilities from configuration-driven module loading. Modules must be annotated with `[BaubitModule]` and discovered at compile time. Configuration can only select from these pre-registered modules using simple string keys instead of assembly-qualified type names.

**Key security features:**
- No reflection-based type loading from configuration
- Compile-time validation of module definitions
- Configuration uses simple string keys, not type names
- Consumer assemblies can register their own modules using `[GeneratedModuleRegistry]`
- Service provider factory must be explicitly specified (no dynamic type loading from configuration)

## Quick Start

### 1. Define a Configuration

```csharp
public class MyModuleConfiguration : Baubit.DI.Autofac.Configuration
{
    public string ConnectionString { get; set; }
    public int PoolSize { get; set; } = 10;
}
```

### 2. Create an Autofac Module

```csharp
[BaubitModule("mymodule")]  // Required for configuration-based loading
public class MyModule : Baubit.DI.Autofac.Module<MyModuleConfiguration>
{
    // Constructor for loading from IConfiguration (appsettings.json)
    public MyModule(IConfiguration configuration)
        : base(configuration) { }

    // Constructor for programmatic creation
    public MyModule(MyModuleConfiguration configuration, List<Baubit.DI.IModule> nestedModules = null)
        : base(configuration, nestedModules) { }

    public override void Load(ContainerBuilder containerBuilder)
    {
        // Register using Autofac's ContainerBuilder API
        containerBuilder.RegisterType<MyService>()
                       .As<IMyService>()
                       .WithParameter("connectionString", Configuration.ConnectionString)
                       .SingleInstance();
    }
}
```

**Note**: The `[BaubitModule("key")]` attribute registers your module for compile-time validated, secure configuration loading. The key is used in `appsettings.json` instead of assembly-qualified type names.

---

## Application Creation Patterns

Baubit.DI.Autofac supports four patterns for creating applications.

> **Important:** You must call the `Register()` method on each module registry (typically 1 per library) before module loading. See [Consumer Module Registration](#consumer-module-registration).

### Pattern 1: Modules from appsettings.json

Load ALL modules from configuration. Module types, their configurations, and nested modules are defined in JSON.

```csharp
// Register consumer modules first
MyModuleRegistry.Register();

// appsettings.json defines all modules
// Must explicitly specify Autofac factory for security
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>()
          .Build()
          .RunAsync();
```

**appsettings.json:**
```json
{
  "modules": [
    {
      "key": "mymodule",
      "configuration": {
        "connectionString": "Server=localhost;Database=mydb",
        "poolSize": 20
      }
    }
  ]
}
```

**Use when:**
- Module configuration should be externally configurable
- You want to change behavior without recompiling
- All module settings can be expressed in configuration

---

### Pattern 2: Modules from Code (IComponent)

Load ALL modules programmatically using `IComponent`. No configuration file needed.

```csharp
// Register consumer modules first
MyModuleRegistry.Register();

// Define a component that builds modules in code
public class MyComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<MyModule, MyModuleConfiguration>(
            cfg =>
            {
                cfg.ConnectionString = "Server=localhost;Database=mydb";
                cfg.PoolSize = 20;
            },
            cfg => new MyModule(cfg)
        );
    }
}

// Load modules from component only (no appsettings.json)
await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>(
              componentsFactory: () => [new MyComponent()]
          )
          .Build()
          .RunAsync();
```

**Use when:**
- Module configuration is determined at compile time
- You need full control over module instantiation
- Configuration values come from code, not files

---

### Pattern 3: Hybrid Loading (appsettings.json + IComponent)

Combine BOTH configuration-based and code-based module loading. This is the most flexible approach.

```csharp
// Register consumer modules first
MyModuleRegistry.Register();

// Load modules from BOTH appsettings.json AND code
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>(
              componentsFactory: () => [new MyComponent()]
          )
          .Build()
          .RunAsync();
```

**Loading order:**
1. Components from `componentsFactory` are loaded first
2. Modules from appsettings.json `modules` section are loaded second

**Use when:**
- Some modules need external configuration (database connections, API keys)
- Some modules need compile-time configuration or code-based logic
- You want to extend configuration-based modules with additional code-based ones

---

### Pattern 4: Mixed Autofac and Standard DI Modules

You can use **BOTH** Autofac modules and standard DI modules in the same application **as long as** you use `Baubit.DI.Autofac.ServiceProviderFactory`.

```csharp
// Register consumer modules first
MyModuleRegistry.Register();

// Autofac module using ContainerBuilder
public class AutofacModule : Baubit.DI.Autofac.Module<MyConfig>
{
    public override void Load(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<MyService>().As<IMyService>().SingleInstance();
    }
}

// Standard DI module using IServiceCollection
public class StandardModule : Baubit.DI.Module<StandardConfig>
{
    public override void Load(IServiceCollection services)
    {
        services.AddSingleton<IOtherService, OtherService>();
    }
}

await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>(
              componentsFactory: () => [
                  new AutofacComponent(),    // Uses Autofac
                  new StandardComponent()     // Uses standard DI
              ]
          )
          .Build()
          .RunAsync();
```

**How it works:**
- `ServiceProviderFactory.Load` checks module types at runtime
- Autofac modules (implementing `Baubit.DI.Autofac.IModule`) register directly with `ContainerBuilder`
- Standard DI modules (implementing only `Baubit.DI.IModule`) register with `IServiceCollection`, then populate into Autofac

**Use when:**
- Gradually migrating from standard DI to Autofac
- Some modules need Autofac features (interceptors, decorators)
- Other modules work fine with standard DI

> The full set of sample code can be found [here](./SampleConsoleApp)

---

## Module Configuration in appsettings.json

Module configurations can be defined in three ways:

### Direct Configuration

Configuration values are enclosed in a `configuration` section:

```json
{
  "modules": [
    {
      "key": "mymodule",
      "configuration": {
        "connectionString": "Server=localhost;Database=mydb",
        "poolSize": 20,
        "modules": [
          {
            "key": "nested-module",
            "configuration": { }
          }
        ]
      }
    }
  ]
}
```

### Indirect Configuration

Configuration is loaded from external sources via `configurationSource`:

```json
{
  "modules": [    
    {
      "key": "mymodule",
      "configurationSource": {
        "jsonUriStrings": ["file://path/to/config.json"]
      }
    }
  ]
}
```

**config.json:**
```json
{
  "connectionString": "Server=localhost;Database=mydb",
  "poolSize": 20,
  "modules": [    
    {
      "key": "nested-module",
      "configuration": {
        "somePropKey": "some_prop_value"
      }
    }
  ]
}
```

### Hybrid Configuration

Combine direct values with external sources:

```json
{
  "modules": [
    {
      "key": "mymodule",
      "configuration": {
        "connectionString": "Server=localhost;Database=mydb"
      },
      "configurationSource": {
        "jsonUriStrings": ["file://path/to/additional.json"]
      }
    }
  ]
}
```

**additional.json:**
```json
{
  "poolSize": 20,
  "modules": [
    {
      "key": "nested-module",
      "configuration": { }
    }
  ]
}
```

---

## Consumer Module Registration

Consumer projects (test projects, libraries, plugins) can register their own modules using the `[GeneratedModuleRegistry]` attribute.

**Important:** Registry registration must be called **before** any module loading operations.

### Step 1: Install Baubit.DI.Autofac

```bash
dotnet add package Baubit.DI.Autofac
```

The generator is automatically included via the Baubit.DI dependency.

### Step 2: Create a Registry Class

```csharp
using Baubit.DI;

namespace MyProject
{
    [GeneratedModuleRegistry]
    internal static partial class MyModuleRegistry
    {
        // Register() method will be generated automatically
    }
}
```

The generator discovers all modules in your assembly marked with `[BaubitModule]` and generates a `Register()` method in your namespace.

### Step 3: Define Your Modules

```csharp
[BaubitModule("my-custom-module")]
public class MyCustomModule : Baubit.DI.Autofac.Module<MyConfig>
{
    public MyCustomModule(IConfiguration config) : base(config) { }
    
    public override void Load(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<MyService>().As<IMyService>().SingleInstance();
    }
}
```

### Step 4: Register at Startup

**Critical:** Call `Register()` before any module loading:

```csharp
// MUST be called before any module loading
MyModuleRegistry.Register();

// Now your modules are available in configuration
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>()
          .Build()
          .RunAsync();
```

**Why this is required:**
- The main `Baubit.DI.ModuleRegistry` only knows about modules in the Baubit.DI assembly
- Consumer assemblies must explicitly register their modules with `ModuleRegistry.RegisterExternal()`
- The generated `Register()` method does this automatically
- Registration must happen before `UseConfiguredServiceProviderFactory()` initializes the registry

**Configuration:**
```json
{
  "modules": [
    {
      "key": "my-custom-module",
      "configuration": { }
    }
  ]
}
```

---

## API Reference

<details>
<summary><strong>BaubitModuleAttribute</strong></summary>

Marks a module class for compile-time discovery and registration.

| Property | Description |
|----------|-------------|
| `Key` | Unique string key used in configuration to identify this module |

**Usage:**
```csharp
[BaubitModule("mymodule")]
public class MyModule : Baubit.DI.Autofac.Module<MyConfig> { }
```

**Requirements:**
- Key must be unique across all modules in the compilation
- Module must implement `IModule`
- Module must have a constructor accepting `IConfiguration`

</details>

<details>
<summary><strong>GeneratedModuleRegistryAttribute</strong></summary>

Marks a partial class to receive generated module registry methods for consumer assemblies.

**Usage:**
```csharp
[GeneratedModuleRegistry]
internal static partial class MyModuleRegistry
{
    // Register() method generated automatically
}

// At application startup:
MyModuleRegistry.Register();
```

**Purpose:**
- Allows test projects to register test-specific modules
- Enables consumer libraries to provide their own modules
- Supports plugin architectures with distributed modules

</details>

<details>
<summary><strong>IModule</strong></summary>

Interface for Autofac dependency injection modules.

| Member | Description |
|--------|-------------|
| `Configuration` | Module configuration |
| `NestedModules` | Child modules |
| `Load(ContainerBuilder)` | Register services with Autofac |

</details>

<details>
<summary><strong>Module / Module&lt;TConfiguration&gt;</strong></summary>

Abstract base classes for Autofac modules.

| Constructor | Description |
|-------------|-------------|
| `Module(TConfiguration, List<IModule>)` | Create with config and nested modules |
| `Module(IConfiguration)` | Create from IConfiguration section |

| Virtual Method | Description |
|----------------|-------------|
| `OnInitialized()` | Called after construction |
| `GetKnownDependencies()` | Return hardcoded module dependencies |
| `Load(ContainerBuilder)` | Register services (override for Autofac registration) |

**Note:** The `Load(IServiceCollection)` method is marked as obsolete with `error: true` to enforce using Autofac's ContainerBuilder.

</details>

<details>
<summary><strong>ServiceProviderFactory</strong></summary>

Service provider factory that integrates Baubit.DI module system with Autofac container.

| Constructor | Description |
|-------------|-------------|
| `ServiceProviderFactory(IConfiguration, IComponent[])` | Create with configuration and components |
| `ServiceProviderFactory(IServiceProviderFactory<ContainerBuilder>, IConfiguration, IComponent[])` | Create with custom Autofac factory, configuration, and components |

| Property | Description |
|----------|-------------|
| `InternalFactory` | The internal Autofac service provider factory |
| `Modules` | Flattened collection of all loaded modules |

| Method | Description |
|--------|-------------|
| `Load(ContainerBuilder)` | Load modules into Autofac container (handles both Autofac and standard DI modules) |
| `UseConfiguredServiceProviderFactory<T>(T)` | Configure host builder with this factory |

**Mixed Module Support:**
The `Load` method automatically detects module types:
- Autofac modules (`Baubit.DI.Autofac.IModule`) → registered directly with `ContainerBuilder`
- Standard DI modules (`Baubit.DI.IModule` only) → registered with `IServiceCollection`, then populated into Autofac

</details>

<details>
<summary><strong>Configuration</strong></summary>

Abstract base class for Autofac module configurations. Extends `Baubit.DI.Configuration`.

</details>

---

For additional API reference on base interfaces and classes (`IComponent`, `Component`, `ComponentBuilder`, `ModuleBuilder`, etc.), see [Baubit.DI API Reference](https://github.com/pnagoorkar/Baubit.DI#api-reference).

## License

MIT
