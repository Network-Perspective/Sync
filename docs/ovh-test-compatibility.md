# OVH Sync review candidate — 10 November 2026

**Review candidate, not live acceptance.** PostgreSQL **fresh synthetic database** compatibility and the existing KV2 read/write/delete contract are tested locally. Following owner review, OVH now uses a separate `OvhVaultClient` provider; the HashiCorpVault project is restored byte-for-byte to the base version. Reads request the latest value and writes retain existing unconditional replacement semantics, without version tracking or coordination. Real data import and live OVH/OAuth acceptance remain absent. Passing builds/CI does not authorize deployment or real external effects.

Base: `8eb2ebe68d136cde5c9333638e184d909c14cbad`. Changes are intentionally **uncommitted/unpushed**, solely in this isolated Sync worktree. No application source fork, live service/database/cloud operation, real migration, secret-value/customer-record inspection, release or deployment was performed.

## Staging warning before owner commit

The owner has since staged the separate OVH project, and the earlier HashiCorp `AssemblyInfo.cs` discrepancy is resolved. Other candidate changes remain unstaged/untracked, including this follow-up's generalized provider-selection rule and tests. Review both `git diff --cached` and `git diff`, including untracked files, before committing; an index-only commit need not match the tested working tree. The earlier `IVersionedVault.cs` and `VaultVersionRequiredException.cs` additions remain absent from both index and worktree. The agent has not changed owner staging or committed/pushed.

## Implemented boundary

- `Database__Provider=PostgreSql` selects Npgsql EF Core 8.0.11 / Npgsql 8.0.6 for the existing unit of work, and PostgreSQL for the existing Quartz 3.18.2 persistent store. SQL Server remains the default; unknown provider names fail. No concurrency, scheduler ID, start delay, job protocol or public `/ws/v1/...` routes changed.
- PostgreSQL never registers `DbInitializerHostedService`; direct `UnitOfWork.MigrateAsync` also rejects PostgreSQL, preventing SQL Server migration-history execution. SQL Server startup/migrations remain unchanged.
- Orchestrator's `--ovh-schema-script` emits transactional, create-only PostgreSQL application DDL plus fresh version-matched Quartz tables, then exits **before host/configuration/vault/scheduler creation**. It does not connect to or create a database. Quartz's destructive upstream DROP preamble is removed; its Apache license is included. Repeat application fails rather than replacing state. There is no PostgreSQL upgrade/import runner or EF baseline history yet.
- PostgreSQL dates are `timestamp with time zone`: source/application UTC clock values returned as `Unspecified` are tagged UTC, Local values normalize to UTC, and reads return UTC. PostgreSQL's microsecond precision, source collation/name uniqueness and import timestamp assumptions still need explicit migration acceptance.
- Orchestrator can explicitly select `OvhVault` instead of its unchanged Azure default. Worker selects OVH through its own `OvhVault` subsection. The separate `NetworkPerspective.Sync.Infrastructure.Vaults.Ovh` project contains the client, config/validation, registration and real secret-read health check; it does not inherit from or modify HashiCorpVault. Worker requires exactly one of its five primary vault providers and rejects every mixed-provider combination before registration, rather than silently applying provider priority. Primary section lookup is case-insensitive, matching .NET configuration binding. Orchestrator `/` remains **alive only**, not database/vault readiness; no invented probe was added.
- The entire HashiCorpVault project matches the base version, including the original client, configuration/validator and registration. Worker retains only the HcpVault subsection bug fix and uses its original two-argument registration, without adding OVH validation or a new Hcp health check. The pre-existing recursive three-argument Hcp health overload remains unchanged and is not called by either application.
- The OVH client uses token-only Bearer auth, HTTPS `/api/<domain UUID>/` with the essential trailing slash, mount `secret`, the existing `{ "data": { "secret": "..." } }` payload, no HTTP redirects and a conservative **64,000-byte complete serialized UTF-8 request** bound. No Kubernetes role/token, mTLS client, automatic PAT renewal, secret retention changes or Azure pruning is introduced. Provider error bodies are excluded from OVH exceptions because they may echo secrets.
- The existing `IVault` interface, `CachedVault`, token callers and Orchestrator revocation behavior remain unchanged. Reads omit a version selector; writes omit CAS; deletes deactivate the latest version rather than destroying history. There is no metadata pre-read, new lock, version-aware interface, special blocking exception or caller migration.
- Microsoft/Google/Slack/Jira/customer integrations, optional customer ExternalAzureKeyVault, Core HTTP BaseUrl and worker/hub contracts are retained, not rewritten. An explicitly configured external Azure decorator still directs secrets to that customer vault; it is **not an OVH compatibility workaround**.
- NSwag client generation now consumes the already-built Orchestrator dependency and the selected Debug/Release configuration. Its old nested rebuild stalled for >200 seconds locally; generation with `noBuild=true` completed in ~1.6 seconds. No generated public contract change is intended.

## Configuration (names, never secret values)

| Component | Supported key | Contract |
|---|---|---|
| Orchestrator | `Database__Provider` | `SqlServer` (default) or `PostgreSql`, case-insensitive; no numeric/unknown fallback |
| Orchestrator | `ConnectionStrings__Database` | Existing key; Npgsql connection string for PostgreSQL. Owner supplies isolated DB/role and TLS verification settings, e.g. `SSL Mode=VerifyFull` plus the appropriate trust chain. Never log it. |
| Orchestrator | `App__Scheduler__UsePersistentStore`, `App__Scheduler__StartDelay`, existing `App__Scheduler__Sync__*` / `SecretRotation__*` | Existing bindings and concurrency retained. Set persistent store deliberately; legacy `Connector__Scheduler__*` is not this contract. |
| Orchestrator | `Infrastructure__Vault__Provider` | `AzureKeyVault` default (existing flat `Infrastructure:Vault` options), or `OvhVault` with nested options below |
| Orchestrator | `Infrastructure__Vault__OvhVault__{BaseUrl,Token,MountPoint,TestSecretName}` | HTTPS origin + `/api/<domain UUID>` (slash normalized), scoped runtime PAT, `MountPoint=secret`, existing test-secret path |
| Worker | `Infrastructure__Vaults__OvhVault__{BaseUrl,Token,MountPoint,TestSecretName}` | Same OVH profile. Do not also supply `AzureKeyVault`, `HcpVault`, `GoogleSecretManager` or `AmazonSecretsManager`; mixed primary-provider configuration is rejected. |
| Worker legacy HcpVault only | `Infrastructure__Vaults__HcpVault__{BaseUrl,Token,VaultRole,MountPoint,TestSecretName}` | Original client and validation unchanged, including the original validator's required fields. This is not the OVH provider. |
| Worker | `Infrastructure__Core__BaseUrl`, `Infrastructure__Orchestrator__BaseUrl` | Existing same-namespace `http://core:8080/` and `http://orchestrator:8080/` support retained. No public/Azure fallback or HTTPS enforcement added. |
| Both | existing data-source, worker identity, telemetry and authentication settings | Still required; this table is the changed provider contract, not a complete environment secret inventory. |

Worker must configure exactly one primary provider: `AzureKeyVault`, `HcpVault`, `OvhVault`, `GoogleSecretManager` or `AmazonSecretsManager`. This also rejects previously accepted legacy combinations that relied on implicit provider priority. Optional `ExternalAzureKeyVault` is a customer-vault decorator, not a second primary provider, and remains supported alongside one primary provider; it cannot replace the required primary provider.

The earlier candidate's `HcpVault__UseOvh` configuration is no longer supported for OVH: use the separate `OvhVault` sections above, and change Orchestrator's provider selector to `OvhVault`. No `UseOvh` flag or Kubernetes `VaultRole` exists on the OVH configuration. Legacy defaults are not changed in appsettings. Neither application has been configured here to select PostgreSQL/OVH in a deployment. OVH tests use synthetic tokens only. Existing vault key names, including `orchestrator-api-key`, worker identity/hash keys and connector-specific keys, must be provisioned independently with the preserved `secret` field; nothing seeds them during ordinary startup.

## Owner decision: retain existing secret-write semantics

The owner chose the smaller compatibility change: one Orchestrator and workers owning separate secrets, without adding version tracking or redesigning token refresh. Latest-value reads and unconditional writes remain the contract, including through the existing cache. The Microsoft consent/user/MSAL-cache, Google OAuth/refresh, Slack OAuth/rotation, Jira OAuth/refresh, Worker `SetSecretsHandler` and Orchestrator `ITokenService` callers therefore require no version-aware changes.

**Operational prerequisite, not a verified live setting:** the selected OVH domain/secrets must permit writes without CAS (`cas_required=false`). The app does not change that policy; an API rejection remains an error, with no metadata reread or retry to work around it. Existing cache invalidation, sequential/concurrent token-pair writes and best-effort Orchestrator revocation are unchanged. Reading the latest value means the latest value at the time of the vault request, not bypassing `CachedVault` on every access.

**Known limit:** single-writer ownership is an operating assumption, not an enforced lock. If two operations ever write the same secret—including during rollout overlap or within one worker—the last successful write wins. Revisit coordination only if shared ownership or a concrete same-key race is established. No claim of transactional token pairs, new retention safety or multi-writer correctness is made; no such redesign is part of this migration slice. Ten-version retention for confirmed new mutable caches remains separately gated on retention/restore acceptance; this simplification neither applies nor validates it.

## Schema/import operating prerequisites

For **offline inspection only**, after building Orchestrator:

```sh
dotnet src/SingleContainer/Orchestrator/NetworkPerspective.Sync.Orchestrator/bin/Release/net8.0/NetworkPerspective.Sync.Orchestrator.dll --ovh-schema-script > /tmp/sync-postgresql-baseline.sql
```

Execution is a separate operator action, not startup/Argo/CI: review the exact emitted script/image digest; approve destination; prepare an **empty isolated Sync database**, agreed collation, DDL owner and narrower runtime role; fence other writers and preserve recoverable state; apply transactionally with error-stop. No SQL Server Quartz rows are copied. Later upgrades require new reviewed scripts, not rerunning this baseline.

**Real import is not implemented.** It still needs an immutable consistent SQL Server snapshot, mappings for all six application tables, preserved GUIDs/worker hashes/salts/encrypted `DbSecrets` and their underlying crypto keys, identity-sequence reseeding, collation/UTC/precision validation, counts/checksums/FKs, scoped runtime-vault copy/recovery, source/target side-effect fencing and rollback/forward-recovery decisions. No production-scale or migrated-data claim follows from the fresh PostgreSQL test.

## Validation actually run

.NET SDK 8.0.133; explicitly authorized NuGet restores used `--source https://api.nuget.org/v3/index.json -p:NuGetAudit=false`. No whole-solution live integration suite was run. After generalizing primary-provider selection, **66 Worker + 28 Orchestrator + 42 application = 136 affected Release tests passed** using the three-project loop below. The earlier PostgreSQL run passed **12 tests**, including the disposable integration test below; it was not repeated for these vault-only changes.

```sh
# Each of these three exact projects: restore first as above, then the same Release command.
for project in \
  src/SingleContainer/Worker/NetworkPerspective.Sync.Worker.Tests/NetworkPerspective.Sync.Worker.Tests.csproj \
  src/SingleContainer/Orchestrator/NetworkPerspective.Sync.Orchestrator.Tests/NetworkPerspective.Sync.Orchestrator.Tests.csproj \
  src/SingleContainer/Orchestrator/NetworkPerspective.Sync.Orchestrator.Application.Tests/NetworkPerspective.Sync.Orchestrator.Application.Tests.csproj; do
  dotnet test "$project" --no-restore -c Release --filter 'SkipInCi!=true'
done
# Fourth project was also restored explicitly before this command.
SYNC_OVH_LOCAL_POSTGRES=true DOCKER_HOST=unix:///var/run/docker.sock DOCKER_CONTEXT=default \
  TESTCONTAINERS_RYUK_DISABLED=true dotnet test \
  src/SingleContainer/Orchestrator/NetworkPerspective.Sync.Orchestrator.Persistence.Tests/NetworkPerspective.Sync.Orchestrator.Persistence.Tests.csproj \
  --no-restore -c Release --filter 'FullyQualifiedName~PostgreSql'
```

The local opt-in test starts its **own** PostgreSQL 18 container with synthetic credentials/records, applies the generated fresh schema, rejects repeat application, round-trips all six entity families/Unicode/UTC/identity values, exercises cascade/`ExecuteDelete`, persists and executes a synthetic job through the application's actual PostgreSQL Quartz registration, then disposes the container. It does not start real sync jobs or use supplied database URLs. `postgres:18` was explicitly pulled locally; resolved digest `sha256:e3d24f6906d0760bf4da0dae872415391ededefad23fd471c47dc53b0ddf3c42`. Final container `8dc66c69d7ba` was deleted by async disposal. The image remains in the local Docker cache. The test skips without the opt-in and rejects a nonlocal `DOCKER_HOST`.

KV2 tests use a recording HTTP handler, never a listening/live service: latest-value reads, URL/Bearer/envelope, required token without Kubernetes fallback, redirect disabling, create/replace/delete through unchanged CachedVault, absence of CAS/metadata requests, errors without retry or response-body disclosure, payload/unsafe-path rejection and pre-cancellation. Additional tests cover independent OVH option binding/validation, all ten primary-provider pairs in canonical and mixed casing, single-provider selection in canonical/lowercase spelling, missing-primary rejection (including decorator-only configuration), retained customer Azure decoration, and healthy/unhealthy results from actual mocked health-check reads. The generalized pair tests were run before the fix and demonstrated the missing rejection, then passed after the fix. Existing legacy-provider registration and SQLite/component tests pass; HashiCorpVault's complete working-tree contents were compared byte-for-byte with the base commit. `--ovh-schema-script` previously emitted 253 lines offline. Scoped `dotnet format whitespace --folder` and `git diff --check` passed.

## Image inputs and review-only CI

Unchanged build inputs: repository **`src/` is the Docker context**, `src/Orchestrator.Dockerfile` and `src/Worker.Dockerfile`; .NET SDK/ASP.NET 8.0 base images; Release output; both final images use UID 1000. Only Worker accepts `VERSION`; the draft uses `1.0.0-ovh.<run-number>+<source-SHA>`. Existing local Core OpenAPI input is used for code generation; runtime `UseBaseUrl:false` and HTTP BaseAddress support remain. No CLI/Helm/standalone connector image is published by the OVH draft.

`.github/workflows/ovh-images.yml` is **review-only, not run**. It targets pushes/PRs to `ovh/main` and manual dispatch. Four scoped test projects plus opt-in synthetic PostgreSQL gate publication. Unlike local dependency preparation, CI restore enables transitive moderate/high/critical NuGet auditing and treats restore warnings as errors. Audit failures must be resolved, not bypassed. It does not claim full-solution/customer-provider acceptance.

Publication requires successful tests, exact `refs/heads/ovh/main`, a non-PR event and GitHub environment **`ovh-publish`**. Configure that protected environment for owner approval and `ovh/main` only; protect branch/workflow changes. Supply variables `OVH_REGISTRY_HOST` (bare lowercase DNS hostname, no scheme/path/port) and `OVH_REGISTRY_PROJECT` (one lowercase project name), and environment secrets `OVH_REGISTRY_USERNAME` / `OVH_REGISTRY_PASSWORD` for a project-scoped push robot. No Azure/ACR, GitOps, cloud-admin or Kubernetes credentials.

The native buildx draft pushes only `<host>/<project>/{orchestrator,sync-worker}`, `linux/amd64`, unique `sha-<source SHA>-<run ID>-<attempt>` tags plus OCI source/revision labels. Password enters `docker login` via stdin, not build args; Docker auth lives outside the build context and is removed in an always-cleanup step. Each successful image has a JSON receipt with `repository`, immutable `digest`, `sourceSHA`; the pair is saved as an immutable-named artifact. An interrupted second build can leave the first image published; this is not deployment approval. No Git tag, CLI release, legacy infrastructure file or Argo update occurs.

Only the inherited GitHub all-push workflow trigger was narrowed to exclude `ovh/**` branches while preserving all other branches and all tags. `cli-release.yml` and Azure pipeline contents are unchanged; never push/mirror `cli*` tags to obtain OVH images. References reviewed: `azure-pipelines-connectors-{build-and-test,publish-stage,publish-prod}.yml`.

Validation: YAML parse, all embedded Bash syntax, SHA-pin/branch/permissions assertions and synthetic validation/receipt checks; actual GitHub CI, NuGet security gate, Docker image builds, registry auth/push and digest download are **unrun**. `actionlint` was not installed. Checkout pin was owner-approved; additional action commit pins were verified from public [setup-dotnet v4.3.1](https://github.com/actions/setup-dotnet/releases/tag/v4.3.1) and [upload-artifact v4.6.2](https://github.com/actions/upload-artifact/releases/tag/v4.6.2). No publication authorization follows from this file existing.

## Scoped files and evidence

Changed source is confined to: the new OVH vault project and solution/project references; Orchestrator Application provider/scheduler, Persistence provider/schema, Program/runtime-vault registration, client-generation build configuration; Worker vault configuration/registration; their focused tests; the two GitHub workflow files; this report. HashiCorpVault and the vault contracts are unchanged from the base in the working tree; Orchestrator token-service changes from the earlier candidate were removed. Existing customer providers, Dockerfiles, historical SQL Server migrations, Azure pipelines and CLI release workflow remain untouched.

Primary contracts checked through BrowserOS: [OVH KV2](https://docs.ovhcloud.com/en/guides/manage-and-operate/secret-manager/kv2-api), [OVH data-plane token authentication](https://docs.ovhcloud.com/en/guides/manage-and-operate/kms/okms-authentication-methods), [Quartz 3.18.2 PostgreSQL schema](https://github.com/quartznet/quartznet/blob/v3.18.2/database/tables/tables_postgres.sql). Documentation and mock wire tests do not prove OVH account authorization, deployed API behavior, token lifetime, payload limits, retention or independent recovery. VaultSharp's signed integer version range is also narrower than OVH's uint32 contract.

Remaining release/activation gates: real importer and schema upgrade contract; provider/TLS/least-privilege/restore acceptance with the existing secret-write semantics; internal HTTP service-token transport/redirect/host tests against exact Core release; full OAuth/sync end-to-end tests; measured performance/rolling overlap; immutable image build/provenance; owner review. No live activation is safe merely because this candidate builds.
