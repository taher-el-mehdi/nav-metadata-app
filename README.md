# NAV Metadata

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-WinForms-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/desktop/winforms/)
[![Windows](https://img.shields.io/badge/Platform-Windows-0078D4?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Dynamics NAV](https://img.shields.io/badge/Dynamics-NAV-00A1F1)](https://navmetadata.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)

**The open-source Windows desktop toolkit for Microsoft Dynamics NAV metadata.**

Browse application objects, view decompressed metadata XML, and export to disk — directly from your NAV SQL database. No CAL. No finsql.

**Website:** [https://navmetadata.com/](https://navmetadata.com)

---

## Highlights

- **Metadata Explorer** — connect to SQL Server and browse Tables, Pages, Reports, Queries, and more
- **XML viewer** — open decompressed `[Object Metadata]` with syntax highlighting
- **Export** — save metadata XML files for diffs, backups, or tooling
- **Filter & search** — find objects by ID or name in large databases
- **Private & local** — runs on your machine; no telemetry or cloud dependency

Works with NAV databases that expose the standard `[Object]` and `[Object Metadata]` system tables (NAV 2009–2018 and).

## Download

Releases are available on GitHub:

**[Download latest release](https://github.com/taher-el-mehdi/nav-metadata/releases/latest)**

## Build from source

```bash
git clone https://github.com/taher-el-mehdi/nav-metadata.git
cd nav-metadata
dotnet build -c Release
dotnet run -c Release
```

## Tech stack

- **C#** · **.NET 10** · **Windows Forms**
- **Microsoft.Data.SqlClient** — SQL Server connectivity
- **Serilog** — file & console logging
- **Microsoft.Extensions.DependencyInjection** — service composition

## Privacy

NAV Metadata does not collect analytics or send database content anywhere. The only optional network call is a GitHub Releases check for updates.

## License

MIT © Taher el mehdi.

