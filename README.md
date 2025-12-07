# Baubit.DI.Autofac

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.DI.Autofac/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.DI.Autofac)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.DI.Autofac.svg)](https://www.nuget.org/packages/Baubit.DI.Autofac/)
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)<br/>
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.DI.Autofac/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.DI.Autofac)

Autofac support for [Baubit.DI](https://github.com/pnagoorkar/Baubit.DI) modular dependency injection framework.

## Table of Contents

- [Installation](#installation)
- [Overview](#overview)
- [Quick Start](#quick-start)
  - [1. Define a Configuration](#1-define-a-configuration)
  - [2. Create an Autofac Module](#2-create-an-autofac-module)
- [Application Creation Patterns](#application-creation-patterns)
  - [Pattern 1: Modules from appsettings.json](#pattern-1-modules-from-appsettingsjson)
  - [Pattern 2: Modules from Code (IComponent)](#pattern-2-modules-from-code-icomponent)
  - [Pattern 3: Hybrid Loading](#pattern-3-hybrid-loading-appsettingsjson--icomponent)
  - [Pattern 4: Mixed Autofac and Standard DI Modules](#pattern-4-mixed-autofac-and-standard-di-modules)
- [Module Configuration in appsettings.json](#module-configuration-in-appsettingsjson)
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

## Quick Start

### 1. Define a Configuration

```csharp
public class MyModuleConfiguration : Baubit.DI.Autofac.AConfiguration
{
    public string ConnectionString { get; set; }
    public int PoolSize { get; set; } = 10;
}
```

### 2. Create an Autofac Module

```csharp
public class MyModule : Baubit.DI.Autofac.AModule<MyModuleConfiguration>
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

---

## Application Creation Patterns

Baubit.DI.Autofac supports four patterns for creating applications.

### Pattern 1: Modules from appsettings.json

Load ALL modules from configuration. Module types, their configurations, and nested modules are defined in JSON.

```csharp
// appsettings.json specifies serviceProviderFactoryType and modules
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build()
          .RunAsync();
```

**appsettings.json:**
```json
{
  "serviceProviderFactoryType": "Baubit.DI.Autofac.ServiceProviderFactory, Baubit.DI.Autofac",
  "modules": [
    {
      "type": "MyNamespace.MyModule, MyAssembly",
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
// Define a component that builds modules in code
public class MyComponent : AComponent
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<MyModule, MyModuleConfiguration>(cfg =>
        {
            cfg.ConnectionString = "Server=localhost;Database=mydb";
            cfg.PoolSize = 20;
        });
    }
}

// Load modules from component only (no appsettings.json)
var configDict = new Dictionary<string, string?>
{
    { "serviceProviderFactoryType", typeof(Baubit.DI.Autofac.ServiceProviderFactory).AssemblyQualifiedName }
};
var configuration = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();

await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory(
              configuration: configuration,
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

Combine BOTH configuration-based and code-based module loading.

```csharp
// Load modules from BOTH appsettings.json AND code
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new MyComponent()])
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

## Mixing Autofac and Standard DI Modules

You can use **BOTH** Autofac modules and standard DI modules in the same application **as long as** you use `Baubit.DI.Autofac.ServiceProviderFactory`.

```csharp
// Autofac module using ContainerBuilder
public class AutofacModule : Baubit.DI.Autofac.AModule<MyConfig>
{
    public override void Load(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<MyService>().As<IMyService>().SingleInstance();
    }
}

// Standard DI module using IServiceCollection
public class StandardModule : Baubit.DI.AModule<StandardConfig>
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

Module configurations follow the same patterns as Baubit.DI:

### Direct Configuration

```json
{
  "serviceProviderFactoryType": "Baubit.DI.Autofac.ServiceProviderFactory, Baubit.DI.Autofac",
  "modules": [
    {
      "type": "MyNamespace.MyModule, MyAssembly",
      "configuration": {
        "connectionString": "Server=localhost;Database=mydb",
        "poolSize": 20,
        "modules": [
          {
            "type": "MyNamespace.NestedModule, MyAssembly",
            "configuration": { }
          }
        ]
      }
    }
  ]
}
```

### Indirect Configuration

```json
{
  "serviceProviderFactoryType": "Baubit.DI.Autofac.ServiceProviderFactory, Baubit.DI.Autofac",
  "modules": [    
    {
      "type": "MyNamespace.MyModule, MyAssembly",
      "configurationSource": {
        "jsonUriStrings": ["file://path/to/config.json"]
      }
    }
  ]
}
```

### Hybrid Configuration

Combine direct values with external sources as documented in [Baubit.DI](https://github.com/pnagoorkar/Baubit.DI#module-configuration-in-appsettingsjson).

---

## API Reference

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
<summary><strong>AModule / AModule&lt;TConfiguration&gt;</strong></summary>

Abstract base classes for Autofac modules.

| Constructor | Description |
|-------------|-------------|
| `AModule(TConfiguration, List<IModule>)` | Create with config and nested modules |
| `AModule(IConfiguration)` | Create from IConfiguration section |

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
<summary><strong>AConfiguration</strong></summary>

Abstract base class for Autofac module configurations. Extends `Baubit.DI.AConfiguration`.

</details>

---

For additional API reference on base interfaces and classes (`IComponent`, `AComponent`, `ComponentBuilder`, `ModuleBuilder`, etc.), see [Baubit.DI API Reference](https://github.com/pnagoorkar/Baubit.DI#api-reference).

## License

MIT
