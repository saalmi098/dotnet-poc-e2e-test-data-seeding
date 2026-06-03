# E2E Test DB Isolation via SQL Server Snapshots

- Requires SQL Server Developer or Enterprise — Express does not support snapshots
- Take snapshot before seeding, restore after test — reverts DB to exact pre-test state
- For xUnit Theory attributes: snapshot must be created in `GetData` before seeding, because `GetData` runs before `InitializeAsync`
- Restore must live in `IAsyncLifetime.DisposeAsync` — xUnit guarantees it runs before the next test's `InitializeAsync`; `DisposalTracker` does not have this guarantee
- `sys.databases.state_desc = 'ONLINE'` is not sufficient after restore — probe with an actual connection attempt until it succeeds
- `RESTORE DATABASE` requires `SET SINGLE_USER WITH ROLLBACK IMMEDIATE`, which kills the WebApp's active DB connections and can cause transient 500 errors
- Fix for transient errors: add `EnableRetryOnFailure()` to the WebApp's EF Core config
