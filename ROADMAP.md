# NAV Metadata — Roadmap

This document is the source of truth for planned work. Contributors can pick items here, discuss them in [GitHub Issues](https://github.com/taher-el-mehdi/nav-metadata-app/issues), and open pull requests against a clear scope.

**Status legend**

| Status | Meaning |
|--------|---------|
| **Shipped** | Available in a released build |
| **In progress** | Actively being worked on or targeted for the next minor release |
| **Planned** | Agreed direction; not started yet |
| **Ideas** | Valuable follow-ups; priority may change |

---

## v1.0 — Metadata Explorer (Shipped)

Core module: browse NAV objects and view decompressed metadata XML from SQL Server.

- [x] SQL connection dialog with live database list
- [x] Windows Authentication and SQL Server Authentication
- [x] Object browser for Tables, Pages, Reports, and Queries
- [x] Metadata XML viewer with syntax highlighting
- [x] Export single-object metadata to XML file
- [x] Filter objects by ID and name
- [x] Save last connection profile (encrypted password via Windows DPAPI)
- [x] GitHub Releases update check
- [x] Report issues link (GitHub new-issue form)

**Relevant code:** `MetadataReader`, `WorkspaceService`, `MainForm`, `MetadataViewForm`, `ConnectionDialog`

---

## v1.1 — Productivity (In progress · Q2 2026)

Day-to-day improvements for developers and consultants.

- [ ] **Codeunit & XmlPort object types** — extend `NavObjectCatalog.BrowsableTypes` (enum and SQL layer already support them)
- [ ] **Recent / named connection profiles** — multiple saved connections instead of a single `connection.json`
- [ ] **Enhanced search & filter** — e.g. global search across types, “modified only”, version-list filter
- [ ] **Keyboard shortcuts & toolbar** — e.g. Enter to view, Ctrl+E export, Ctrl+F filter (F5 refresh exists today)
- [ ] **Copy XML to clipboard** — from the metadata viewer
- [ ] **Find in XML** — search inside `MetadataViewForm`
- [ ] **Export object list to CSV** — grid data for spreadsheets

---

## v1.2 — Metadata Compare (Planned · Q3 2026)

Differentiator for upgrades, environment audits, and customization reviews.

- [ ] **Side-by-side object comparison** — diff metadata XML for two objects (same or different databases)
- [ ] **Database snapshot comparison** — compare two workspace snapshots or two live connections
- [ ] **Compare with file on disk** — diff database metadata vs an exported `.xml` file
- [ ] **Export comparison reports** — HTML or text summary for sharing

**Suggested new areas:** comparison service, diff UI module, snapshot export/import format

---

## v1.3 — Export & Documentation (Planned · Q4 2026)

Bulk output and human-readable docs from raw metadata.

- [ ] **Batch XML export** — multi-select objects and export to a folder (enable `MultiSelect` on the object list)
- [ ] **JSON export** — structured metadata for scripts and CI pipelines
- [ ] **Documentation generator (first release)** — readable docs from table/page metadata (fields, keys, etc.)
- [ ] **Open export folder after export** — small UX polish

---

## v2.0 — Deep Analysis (Planned · 2027)

Move from viewing XML to understanding structure.

- [ ] **Parse table field definitions** from metadata XML
- [ ] **Page control tree visualization** — tree view of page controls
- [ ] **AL generator** — generate AL stubs from NAV object definitions
- [ ] **Cross-type global search** — find objects by name across all types in one query
- [ ] **Dependency hints** — e.g. tables referenced from page metadata
- [ ] **Version list analytics** — group or filter objects by customization tags

---

## v3.0 — Ecosystem (Planned · 2027+)

Extensibility and larger workflows.

- [ ] **Migration toolkit** — assist metadata migration and upgrade workflows
- [ ] **AI assistant** — ask questions about local metadata (privacy-preserving, on-machine)
- [ ] **Plugin system** — custom analyzers without forking the core app
- [ ] **CLI / headless mode** — e.g. `export --server … --table 18` for automation
- [ ] **Offline snapshot mode** — browse saved workspace + metadata without a live SQL connection
- [ ] **Auto-download updates** — extend `GitHubUpdateService` / `IUpdateService` beyond opening the release page

---

## Ideas backlog

Lower priority or scope TBD; still welcome as contributions if you open an issue first.

- [ ] Dark theme
- [ ] Localization (e.g. English / French)
- [ ] Business Central `.app` / AL project support (different data source than `[Object Metadata]`)
- [ ] Favorites / bookmarked objects
- [ ] MenuSuite object type in the browser

---

## How to contribute

1. **Check [open issues](https://github.com/taher-el-mehdi/nav-metadata-app/issues)** — someone may already be working on an item.
2. **Open an issue** for roadmap items you want to tackle (bug, feature, or roadmap pick). Use labels if available.
3. **Keep PRs focused** — one roadmap item (or a small cohesive slice) per pull request.
4. **Match existing patterns** — WinForms UI in `UI/Forms`, services in `Application/Services`, constants in `Core/Constants`.
5. **No telemetry** — new features must keep the app local-first; network use only where already established (e.g. update checks, opening GitHub).

Questions? Open a [discussion or issue](https://github.com/taher-el-mehdi/nav-metadata-app/issues/new) on GitHub.
