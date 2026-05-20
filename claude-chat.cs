  ```text                                                            
  You are a Senior C# Test Automation Engineer and Software          
  Architect. I need your help designing a clean architecture for     
  End-to-End tests using C#, Playwright, and xUnit V3.               
                                                                     
  My goal is to strictly separate the setup and seeding of test data 
   from the actual test logic (UI interactions).                     
                                                                     
  Here are my specific requirements:                                 
                                                                     
  1. Tech Stack: C#, Playwright, xUnit V3, Entity Framework Core     
  (for database access).                                             
  2. Entities: I have an `Employee` object (with various metadata)   
  and an associated `Apartment` object.                              
  3. Test Data Seeding: Before the test starts, these objects        
  (Employee + Apartment) must be inserted into the database.         
  4. Parameter Resolving: The seeded objects should be provided      
  directly to the test as parameters. The test itself should not     
  need to call any manual database setup methods.                    
  5. Teardown: After the test finishes, the generated test data must 
   be cleanly removed from the database (teardown).                  
                                                                     
  The actual Playwright test should be extremely clean and look      
  conceptually something like this:                                  
                                                                     
  ```csharp                                                          
  [Fact/Theory]                                                      
  public async Task                                                  
  Employee_Should_See_Correct_Apartment_Address(Employee             
  seededEmployee)                                                    
  {                                                                  
      await Page.GotoAsync($"/employees/{seededEmployee.Id}");       
      await Expect(Page.Locator(".apartment-street")).ToHaveTextAsyn 
  c(seededEmployee.Apartment.Street);                                
  }                                                                  
                                                                     
  ```                                                                
                                                                     
  Please provide a complete architectural solution for this. Show    
  me:                                                                
                                                                     
  * How to structure the base class or fixture.                      
  * How to handle the asynchronous test data insertion and teardown. 
  * How to implement parameter resolving cleanly in xUnit V3 (e.g.,  
  via Custom Data Attributes, Fixtures, or a Func/Action Wrapper     
  pattern, especially since native async resolving can be tricky in  
  xUnit).                                                            
  ```                                                                                                
                                                                                                     
  Please provide a complete architectural solution for this. Show me:                                
                                                                                                     
  * How to structure the base class or fixture.                                                      
  * How to handle the asynchronous test data insertion and teardown.                                 
  * How to implement parameter resolving cleanly in xUnit V3 (e.g., via Custom Data Attributes,      
  Fixtures, or a Func/Action Wrapper pattern, especially since native async resolving can be tricky  
  in xUnit).                                                                                         
  * An exemplary, executable Playwright test utilizing this architecture.                            
                                                                                                     
  Finally, please briefly evaluate which of the approaches (e.g., Wrapper vs. Custom Attribute) is   
  the most stable and idiomatic for asynchronous database operations in xUnit.                       
                                                                                                     
  ```                                                                                                
                                                                                                     
  ```                                                                                                

● Reading prompt. Will design full architecture now.

  Recalled 1 memory (ctrl+o to expand)

● No prior memory. Building fresh.

  ---
  Architecture: E2E Test Data Seeding — C# / Playwright / xUnit V3

  Project Structure

  TestSuite/
  ├── Infrastructure/
  │   ├── DbContextFactory.cs          # Creates AppDbContext per test
  │   ├── SeedingContext.cs            # Seeds + tracks cleanup
  │   └── PlaywrightTestBase.cs        # Base class: browser + DB lifecycle
  ├── Builders/
  │   ├── EmployeeBuilder.cs
  │   └── ApartmentBuilder.cs
  ├── Attributes/
  │   └── SeedEmployeeAttribute.cs     # xUnit v3 DataAttribute (Approach B)
  └── Tests/
      └── EmployeeTests.cs

  ---
  Entities

  public class Apartment
  {
      public int Id { get; set; }
      public string Street { get; set; } = "";
      public string City   { get; set; } = "";
      public string ZipCode{ get; set; } = "";
  }

  public class Employee
  {
      public int    Id        { get; set; }
      public string Name      { get; set; } = "";
      public string Email     { get; set; } = "";
      public int?   ApartmentId { get; set; }
      public Apartment? Apartment { get; set; }
  }

  ---
  DbContextFactory

  public static class DbContextFactory
  {
      private const string ConnectionString = "Server=...;Database=TestDb;...";

      public static AppDbContext Create()
      {
          var options = new DbContextOptionsBuilder<AppDbContext>()
              .UseSqlServer(ConnectionString)
              .Options;
          return new AppDbContext(options);
      }
  }

  ---
  SeedingContext — tracks inserts, handles teardown

  public sealed class SeedingContext : IAsyncDisposable
  {
      private readonly AppDbContext _db;
      private readonly Stack<Func<Task>> _cleanups = new();

      public SeedingContext() => _db = DbContextFactory.Create();

      public async Task<T> SeedAsync<T>(T entity) where T : class
      {
          _db.Set<T>().Add(entity);
          await _db.SaveChangesAsync();

          // capture entity reference for cleanup
          _cleanups.Push(async () =>
          {
              _db.Set<T>().Remove(entity);
              await _db.SaveChangesAsync();
          });

          return entity;
      }

      public async ValueTask DisposeAsync()
      {
          while (_cleanups.TryPop(out var cleanup))
              await cleanup();
          await _db.DisposeAsync();
      }
  }

  Stack = reverse-order teardown. Employee deleted before Apartment if Apartment seeded first.

  ---
  Approach A — PlaywrightTestBase + SeedingContext (Recommended)

  Seed inside test body. Base class owns browser + DB lifecycle. Zero manual setup/teardown calls in
  test.

  public abstract class PlaywrightTestBase : IAsyncLifetime
  {
      private IPlaywright  _playwright  = null!;
      private IBrowser     _browser     = null!;

      protected IBrowserContext BrowserContext { get; private set; } = null!;
      protected IPage           Page           { get; private set; } = null!;
      protected SeedingContext  Seed           { get; private set; } = null!;

      public virtual async Task InitializeAsync()
      {
          Seed = new SeedingContext();

          _playwright   = await Playwright.CreateAsync();
          _browser      = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
          BrowserContext = await _browser.NewContextAsync();
          Page          = await BrowserContext.NewPageAsync();
      }

      public virtual async Task DisposeAsync()
      {
          await Seed.DisposeAsync();          // DB cleanup first
          await BrowserContext.DisposeAsync();
          await _browser.DisposeAsync();
          _playwright.Dispose();
      }
  }

  Builders:

  public static class EmployeeBuilder
  {
      public static Employee Default(Action<Employee>? configure = null)
      {
          var uid = Guid.NewGuid().ToString("N")[..8];
          var e = new Employee
          {
              Name  = $"Test Employee {uid}",
              Email = $"test_{uid}@example.com",
              Apartment = new Apartment
              {
                  Street  = "Hauptstraße 42",
                  City    = "Vienna",
                  ZipCode = "1010"
              }
          };
          configure?.Invoke(e);
          return e;
      }
  }

  Test:

  public class EmployeeTests : PlaywrightTestBase
  {
      [Fact]
      public async Task Employee_Should_See_Correct_Apartment_Address()
      {
          var employee = await Seed.SeedAsync(EmployeeBuilder.Default());

          await Page.GotoAsync($"/employees/{employee.Id}");

          await Expect(Page.Locator(".apartment-street"))
              .ToHaveTextAsync(employee.Apartment!.Street);
      }

      [Fact]
      public async Task Employee_Form_Shows_Correct_City()
      {
          var employee = await Seed.SeedAsync(
              EmployeeBuilder.Default(e => e.Apartment!.City = "Graz"));

          await Page.GotoAsync($"/employees/{employee.Id}/edit");

          await Expect(Page.Locator("#city-field"))
              .ToHaveValueAsync("Graz");
      }
  }

  Teardown automatic. No [Teardown], no manual delete.

  ---
  Approach B — xUnit V3 DataAttribute with true parameter injection

  xUnit v3 changed DataAttribute.GetData signature to:

  ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
      DisposalTracker disposalTracker, MethodInfo testMethod)

  DisposalTracker accepts IAsyncDisposable. Use it for per-dataset cleanup.

  Step 1 — disposable seed wrapper:

  public sealed class SeededEmployee : IAsyncDisposable
  {
      public Employee Value { get; }
      private readonly SeedingContext _seed;

      internal SeededEmployee(Employee value, SeedingContext seed)
      {
          Value = value;
          _seed = seed;
      }

      public async ValueTask DisposeAsync() => await _seed.DisposeAsync();
  }

  Step 2 — custom DataAttribute:

  [AttributeUsage(AttributeTargets.Method)]
  public sealed class SeedEmployeeAttribute : DataAttribute
  {
      public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
          DisposalTracker disposalTracker,
          MethodInfo testMethod)
      {
          var seed = new SeedingContext();
          var employee = await seed.SeedAsync(EmployeeBuilder.Default());

          var wrapper = new SeededEmployee(employee, seed);
          disposalTracker.Add(wrapper);                  // xUnit v3 calls DisposeAsync after test

          return [new TheoryDataRow<SeededEmployee>(wrapper)];
      }
  }

  Test:

  public class EmployeeTheoryTests : PlaywrightTestBase
  {
      [Theory]
      [SeedEmployee]
      public async Task Employee_Should_See_Correct_Apartment_Address(SeededEmployee ctx)
      {
          var employee = ctx.Value;

          await Page.GotoAsync($"/employees/{employee.Id}");

          await Expect(Page.Locator(".apartment-street"))
              .ToHaveTextAsync(employee.Apartment!.Street);
      }
  }

  Multiple seed sets — yield multiple rows:

  return
  [
      new TheoryDataRow<SeededEmployee>(await CreateAndTrack(disposalTracker, "Vienna")),
      new TheoryDataRow<SeededEmployee>(await CreateAndTrack(disposalTracker, "Graz")),
  ];

  ---
  Evaluation

  Criterion: Stability
  Approach A (SeedingContext in base): ★★★★★ — no xUnit internals
  Approach B (Custom DataAttribute): ★★★★☆ — depends on DisposalTracker semantics
  ────────────────────────────────────────
  Criterion: Idiomatic
  Approach A (SeedingContext in base): ★★★★★ — standard IAsyncLifetime
  Approach B (Custom DataAttribute): ★★★☆☆ — xUnit v3 only, less known
  ────────────────────────────────────────
  Criterion: True param inject
  Approach A (SeedingContext in base): ✗ seed is first line in test
  Approach B (Custom DataAttribute): ✓ parameter arrives seeded
  ────────────────────────────────────────
  Criterion: Teardown guarantee
  Approach A (SeedingContext in base): ✓ DisposeAsync always runs
  Approach B (Custom DataAttribute): ✓ DisposalTracker always flushes
  ────────────────────────────────────────
  Criterion: Async safety
  Approach A (SeedingContext in base): ✓ full async throughout
  Approach B (Custom DataAttribute): ✓ xUnit v3 ValueTask native
  ────────────────────────────────────────
  Criterion: Debuggability
  Approach A (SeedingContext in base): High — stack trace simple
  Approach B (Custom DataAttribute): Medium — xUnit framework layer
  ────────────────────────────────────────
  Criterion: DI / config injection
  Approach A (SeedingContext in base): Easy via constructor
  Approach B (Custom DataAttribute): Harder — attribute has no ctor DI

  Verdict: Approach A wins for production. SeedingContext in the base class — single seeding call,
  guaranteed cleanup, no xUnit internals. The test reads cleanly:

  var employee = await Seed.SeedAsync(EmployeeBuilder.Default());

  One line. Same conceptual clarity as parameter injection. Zero framework coupling.

  Use Approach B only when: (a) parametrizing same test over multiple seed variants via [Theory], or
  (b) team convention demands parameter-in-signature style.

  ---
  Save this to memory? (project context + architectural decision)

✻ Churned for 2m 48s