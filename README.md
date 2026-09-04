# RaceDay Event Management System

## Project Description
**RaceDay** is a comprehensive, full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. 

South Africa boasts a rich road events culture, ranging from iconic ultra-marathons like the Comrades Marathon and Two Oceans, to the Cape Town Cycle Tour, Soweto Marathon, and hundreds of local weekend park runs and charity races. Despite massive participation, many of these events rely on outdated, paper-based registrations, fragmented spreadsheets, and disconnected communication channels. This leaves event organisers overwhelmed and participants underserved.

RaceDay solves these challenges by providing a centralized, API-driven platform where:
- **Event Organisers** can seamlessly create and manage events, event categories, and participant results.
- **Participants** can discover upcoming events, submit entry enrolments, track their historical race performances, and prepare for race day using live weather and route information.

This project is built progressively across three parts as a fully containerised, cloud-aware, API-driven application reflecting real-world software engineering practices in sports technology.

---

## User Roles

### Organiser
Organisers are administrative users responsible for hosting and managing events.
- **Event Management**: Create, update, and publish new sporting events (e.g., Road Running, Trail Run, Cycling, Fun Walk).
- **Category Configuration**: Define sub-categories per event (e.g., 42km Full Marathon, 21km Half Marathon, 10km, 5km Fun Run) along with entry fees and cut-off times.
- **Participant & Result Management**: View all enrolled participants per event, record finishing times, publish race results, and assign athlete placings.

### Participant
Participants are general users (runners, cyclists, walkers) who engage with events on the platform.
- **Event Discovery & Entry**: Search and browse upcoming events across South Africa, view detailed route/weather information, and enroll/enter into specific event categories.
- **Performance Tracking**: Maintain a personalized profile showing entry history, past finish times, rankings, and personal bests.
- **Race Day Preparation**: Access real-time route details, start times, and location-specific weather updates prior to race day.

---

## Part 1
Part 1 focuses on system design, architectural planning, data modeling, and setting up repository CI/CD automation.

### ERD (Entity Relationship Diagram)
- **File**: Located at `/docs/ERD.png` (or `/docs/ERD.pdf`)
- **Overview**: The ERD models 6 primary entities (`Users`, `Roles`, `Events`, `EventCategories`, `Enrolments`, and `Results`). It explicitly details Primary Keys (PK), Foreign Keys (FK), field constraints, and relationship cardinalities (1:N, M:N via bridge tables).

### API Endpoint Plan
- **File**: Located at `/docs/API_Endpoint_Plan.pdf` (or `/docs/API_Endpoint_Plan.md`)
- **Overview**: A comprehensive REST API specification matrix outlining HTTP Methods (`GET`, `POST`, `PUT`, `DELETE`), URL routes (`/api/...`), required roles (`Public`, `Participant`, `Organiser`), Request Body parameters, and Expected Response HTTP Status Codes for:
  - Authentication (`/api/auth/register`, `/api/auth/login`)
  - User Profiles (`/api/users/profile`)
  - Event Management (`/api/events`, `/api/events/{id}`)
  - Event Categories (`/api/events/{id}/categories`)
  - Event Enrolments (`/api/events/{id}/enrol`)
  - Results & Performance (`/api/results`, `/api/events/{id}/results`)

### SQL Database Script
- **File**: Located at `/docs/RaceDay_Schema.sql`
- **Overview**: A complete T-SQL database creation script compatible with Microsoft SQL Server Management Studio (SSMS). Contains `CREATE TABLE` DDL statements with constraints (`NOT NULL`, `UNIQUE`, `DEFAULT`, `CHECK`, `PRIMARY KEY`, `FOREIGN KEY`), followed by `INSERT` DML statements seeding sample data (2 Organisers, 2 Participants, 3 Events, Categories, and Enrolments).

---

## Repository Structure

```text
├── .github/
│   └── workflows/
│       └── main.yml           # GitHub Actions CI/CD workflow configuration
├── docs/
│   ├── ERD.png                # Entity Relationship Diagram (System Data Model)
│   ├── API_Endpoint_Plan.pdf  # Comprehensive REST API Specification Document
│   └── RaceDay_Schema.sql     # Complete SQL Server Database Schema & Seed Script
├── .gitignore                 # Environment and build artifact ignores
└── README.md                  # Main project documentation and guide

##Database Setup
RaceDay uses Microsoft SQL Server as its database. The database schema and sample data are provided in:

/docs/RaceDay_Schema.sql

#Running the Database Script in SSMS: 
Open SQL Server Management Studio (SSMS).

Connect to your SQL Server instance (e.g., localhost or (localdb)\MSSQLLocalDB)
Select File → Open → File.

Navigate to the project's docs folder.

5. Open RaceDay_Schema.sql.

6. Click Execute or press F5 to run the script.

7. Once execution is complete, refresh the database in Object Explorer.

8. Verify that the RaceDay tables and sample data have been created successfully.

The SQL script contains all required database tables, primary keys, foreign keys, constraints, and sample data for testing the RaceDay system.