# RaceDay Event Management System

## Project Description

**RaceDay** is a comprehensive, full-stack, web-based event management system designed specifically for the South African road running, walking, and cycling community.

South Africa has a rich road events culture, ranging from iconic ultra-marathons such as the Comrades Marathon and Two Oceans Marathon, to events like the Cape Town Cycle Tour, Soweto Marathon, and hundreds of local weekend park runs and charity races. Despite massive participation, many of these events still rely on outdated paper-based registrations, fragmented spreadsheets, and disconnected communication channels. This leaves event organisers overwhelmed and participants underserved.

RaceDay solves these challenges by providing a centralized, API-driven platform where:

- **Event Organisers** can seamlessly create and manage events, event categories, and participant results.
- **Participants** can discover upcoming events, submit entry enrolments, track historical race performances, and prepare for race day using live weather and route information.

This project is built progressively across three parts as a fully containerised, cloud-aware, API-driven application reflecting real-world software engineering practices in sports technology.

---

# User Roles & Security Matrix

## Organiser
Organisers are administrative users responsible for hosting and managing events.
* **Event Management:** Create, update, and publish new sporting events (e.g., Road Running, Trail Run, Cycling, Fun Walk).
* **Category Configuration:** Define sub-categories per event (e.g., 42km Full Marathon, 21km Half Marathon, 10km, 5km Fun Run), including entry fees and cut-off times.
* **Participant & Result Management:** View enrolled participants, record finishing times, publish race results, and assign athlete placings.

## Participant
Participants are general users (runners, cyclists, and walkers) who engage with events on the platform.
* **Event Discovery & Entry:** Search and browse upcoming events across South Africa, view route/weather information, and enrol into specific event categories.
* **Performance Tracking:** Maintain a personalised profile showing entry history, previous finish times, rankings, and personal bests.
* **Race Day Preparation:** Access route details, start times, and location-specific weather updates before race day.

---

# Part 1: Design & Architecture
Part 1 focuses on system design, architectural planning, data modelling, and setting up repository CI/CD automation.

### ERD (Entity Relationship Diagram)
* **File Location:** `/docs/ERD.png` or `/docs/ERD.pdf`
* **Overview:** Models six primary entities (`Users`, `Roles`, `Events`, `EventCategories`, `Enrolments`, `Results`) defining field constraints, PK/FK configurations, and cardinality parameters.

### API Endpoint Plan
* **File Location:** `/docs/API_Endpoint_Plan.pdf`
* **Overview:** A comprehensive REST API specification matrix mapping HTTP verbs, target paths, request structural blocks, and status code matrices.

### SQL Legacy Schema Script
* **File Location:** `/docs/RaceDay_Schema.sql`
* **Overview:** Contains baseline DDL syntax and transactional `INSERT` declarations to evaluate relational queries directly inside SSMS.

---

# Part 2: RESTful API Development & Automated Verification
Part 2 implements the planning items into an active, highly secure **ASP.NET Core Web API using .NET 8**, supported by layered class components, Entity Framework Code-First data tables, role-based protection routines, and xUnit integration test blocks.

## Project Architecture & Layout
The solution is organized into a clean multi-project architecture to isolate structural concerns:

```text
RaceDay/
│
├── .github/
│   └── workflows/
│       └── main.yml                  # GitHub Actions CI/CD pipeline script
│
├── docs/                             # Design deliverables & verification media
│   ├── ERD.png
│   ├── API_Endpoint_Plan.pdf
│   ├── RaceDay_Schema.sql
│   └── github-actions-success.png    # Task 28 Evidence Screenshot
│
├── RaceDay.Api/                      # ASP.NET Core Web API Engine (.NET 8)
│   ├── Controllers/                  # Route entry endpoints mapping application logic
│   ├── Data/                         # EF Core DbContext & Schema Migration classes
│   ├── Models/                       # Domain entities derived from the core ERD
│   ├── Program.cs                    # Session handling, DI containers, and security maps
│   └── appsettings.json              # Connection configuration definitions
│
├── RaceDay.Contracts/                # Shared Data Transfer Object (DTO) Class Library
│   └── Dtos.cs                       # Decoupled transaction shapes (LoginRequest, EventDto)
│
├── RaceDay.Api.Tests/                # Automated Integration Testing Studio (xUnit)
│   ├── RaceDayFactory.cs             # In-memory virtual web server engine component
│   └── IntegrationTests.cs           # Task 24 Matrix Assertion validation file
│
├── RaceDay.sln                       # Unified Visual Studio Solution workspace
└── README.md                         # Product operations system manual
```

---

## Database Setup (EF Core Code-First)

The storage infrastructure runs off **Microsoft SQL Server** generated directly from C# domain models via Entity Framework Core. 

### Applying Database Migrations Local Execution
To establish or refresh your storage tables from scratch inside Visual Studio 2022:
1. Open the **Package Manager Console** (`Tools` `NuGet Package Manager` `Package Manager Console`).
2. Confirm the **Default project** dropdown menu is locked on **`RaceDay.Api`**.
3. Run the following command to update the local instance:
   ```powershell
   Update-Database
   ```
4. Launch **SQL Server Management Studio (SSMS)**, connect to your database engine, and expand the `RaceDay` catalog to confirm tables have structural matching constraints (`NOT NULL`, `FOREIGN KEY`).

---

##  How to Run the API Application

1. Pull the project workspace onto your local development machine.
2. Launch the solution file **`RaceDay.sln`** inside Visual Studio 2022.
3. Open **`RaceDay.Api/appsettings.json`** and modify the `DefaultConnection` string target parameters to align with your personal local SQL Server authentication requirements:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```
4. Press **`F5`** or click the **Play Debug** button at the top toolbar.
5. Your default web browser will automatically open to display the interactive **Swagger UI interactive page** at `http://localhost:XXXX/swagger/index.html`.

---

##  Authentication & API Endpoints (Swagger Specifications)

The backend enforces server-managed authentication profiles via session mappings. Route layers utilize `[Authorize(Roles = "...")]` parameters to prevent authorization bypass attempts.

### Core REST Route Catalog Matrix
Through the interactive Swagger engine, consumers can invoke and analyze the following paths:

* **Authentication Core (`/api/auth`)**
  * `POST /api/auth/register` - Creates a new user record profile while applying secure salt hashing algorithms to input password values.
  * `POST /api/auth/login` - Evaluates credentials and attaches identity cookie strings tracking the corresponding session state.
  * `POST /api/auth/logout` - Discards active token session descriptors immediately from the browser storage layer.
* **Profile Interface (`/api/profile`)**
  * `GET /api/profile/me` - Resolves identity records for the currently authenticated session space. Returns `401 Unauthorized` for anonymous connections.
* **Race Event Management (`/api/events`)**
  * `GET /api/events` - Open endpoint for all roles to view race events.
  * `POST /api/events` - Restricted to **Organiser** access. Submits new race event metadata records to storage. Returns `403 Forbidden` if invoked by a Participant.
* **Category Configuration (`/api/events/{eventId}/categories`)**
  * `POST /api/events/{eventId}/categories` - Restricted to **Organiser** accounts to declare sub-track ranges (e.g., 21KM half-marathon profile).
* **Event Enrolments (`/api/enrolments`)**
  * `POST /api/enrolments` - Restricted to **Participant** role. Attaches an athlete tracker registration slot onto an open target category block.

---

##  Automated Unit & Integration Testing (xUnit)

To guarantee the underlying security parameters conform exactly to the required specification criteria, an automated integration test block runs within **`RaceDay.Api.Tests`**.

By utilizing a customized `WebApplicationFactory<Program>` framework implementation, the tests spin up a private web api sandbox inside memory, exchange actual SQL drivers for volatile `InMemoryDatabase` instances, and run realistic integration checks:

```csharp
[Fact]
public async Task Participant_CreatesEvent_Returns403Forbidden()
{
    // Arrange: Establish an active authenticated test client logged in as a 'Participant'
    var participant = await LoggedInClient("Participant");

    // Act: Attempt to post a protected event administrative data package
    var response = await participant.PostAsJsonAsync("/api/events", NewEventBody());

    // Assert: Verify that the security interceptor blocks execution with a 403 status
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

---

##  Continuous Integration & Continuous Delivery (CI/CD)

The platform runs systematic building pipelines powered via **GitHub Actions** on every codebase update event. The automated script is located inside **`.github/workflows/main.yml`**.
