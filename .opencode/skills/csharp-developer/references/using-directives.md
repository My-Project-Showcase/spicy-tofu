# Using Directives

## Rule

Order `using` directives into fixed groups, separated by exactly one blank line:

1. External packages: `System.*` first (alphabetical), then `Microsoft.*` and any other third-party namespaces (alphabetical).
2. `Application.*`
3. `Domain.*`
4. `Infrastructure.*`
5. `SharedKernel.*`
6. `Web.*`
7. `Mobile.*`

Within a group, sort alphabetically by namespace. A `using Alias = Namespace.Type;` alias belongs to the group of its target `Namespace` and sorts by that target namespace. Omit empty groups. Keep `using` directives outside the namespace, so the file-scoped namespace follows the last group.

`dotnet format` does not enforce this order: the import sorter is intentionally disabled in `.editorconfig` (no `dotnet_sort_system_directives_first` and no `dotnet_separate_import_directive_groups`). Apply the order by hand and check it in review.

## Example

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Application.Automation;
using Application.Automation.Mobile;
using Application.Automation.Web;
using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Events;
using Domain.Events.EventsRegistry;
using Domain.Runtime.Environment.Configuration;

using Infrastructure.Automation.Mobile;
using Infrastructure.Automation.Web;
using Infrastructure.Events;
using Infrastructure.Logging;
using Infrastructure.Runtime.EventsService;
using Infrastructure.Runtime.JsonService;
using Infrastructure.Runtime.RunServices;
using Infrastructure.Runtime.TestExecution;

namespace Infrastructure.Extensions;
```

With `System.*` and third-party namespaces present, the external group comes first:

```csharp
using System.Text.Json;
using Microsoft.Extensions.Options;

using Application.Runtime.JsonService;

using Domain.Entities.TestCases;
using Domain.Runtime.Environment.Configuration;
```

## Notes

- `System.*` stays inside the external group, ahead of `Microsoft.*` and third-party namespaces; there is no blank line between them.
- `SharedKernel.*` is its own group, after `Infrastructure.*`.
- The executable entry points place `Web.*` and `Mobile.*` last, after `SharedKernel.*`.
