# AutoMarket Intake (POC)

**A Cloud-Native Vertical Slice for Wholesale Vehicle Intake**

This repository contains a Proof of Concept (POC) demonstrating a modern, distributed architecture for vehicle data ingestion. It leverages **.NET Aspire** for local orchestration, mirroring a production-grade Kubernetes environment.

![Status](https://img.shields.io/badge/Status-POC_Complete-green)
![Tech](https://img.shields.io/badge/Stack-.NET_9_%7C_React_%7C_Postgres-blue)

## 🏗 Architecture

The system implements a "Vertical Slice" architecture designed for scalability and observability.

* **Frontend:** React (Vite) Single Page Application.
* **Backend:** .NET 9 Web API.
* **Orchestration:** .NET Aspire (manages containers, networking, and service discovery).
* **Data Layer:** PostgreSQL (Infrastructure-as-Code via Docker).
* **Caching:** Redis (for high-throughput performance).
* **Observability:** OpenTelemetry (Distributed Tracing & Metrics).

### The Data Flow
1.  **Ingest:** User scans a VIN via the React Frontend.
2.  **Process:** API receives the request, starts a Distributed Trace, and invokes the `VehicleGrader` domain service.
3.  **Persist:** Result is saved to PostgreSQL using EF Core.
4.  **Observe:** Metrics (Latency, Status) are captured for analysis.

## 🚀 Getting Started

### Prerequisites
* .NET 9 SDK
* Docker Desktop
* Node.js & npm

### Installation & Run

1.  **Clone the Repository**
    ```bash
    git clone [repo-url]
    ```

2.  **Launch the Backend (Orchestrator)**
    * Open `AutoMarket.Intake.sln` in Visual Studio.
    * Set `AutoMarket.Intake.AppHost` as the Startup Project.
    * Press **F5**.
    * *Note: This will automatically provision the PostgreSQL and Redis containers and apply Database Migrations.*
	* **Another Note:** Visual Studio will launch the **Aspire Dashboard** (usually on port 17020-17200).
        Click the **"Traces"** tab to see real-time visualization of the API latency and Database queries.

3.  **Launch the Frontend**
    * Open a terminal in `AutoMarket.Intake.Frontend`.
    * Run:
        ```bash
        npm install
        npm run dev
        ```
    * Access the UI at `http://localhost:5173`.

## 🛠 Key Technical Highlights

### 1. Self-Healing Infrastructure
The application implements robust **retry logic** for database connections. On startup, the API waits for the PostgreSQL container to fully initialize (`initdb`) before attempting migrations, preventing "Cold Start" crashes common in containerized environments.

### 2. Infrastructure-as-Code (Local)
`.NET Aspire` is used to define the infrastructure in C#. This replaces complex Docker Compose YAML files with strongly typed, compile-time checked resource definitions.

```csharp
// Example: Defining the Database Resource in AppHost
var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume(); // Persistent storage
```

### 3. Database Migrations
Database schema changes are managed via **EF Core Migrations**. The system automatically detects and applies pending migrations on startup, ensuring the database schema always matches the codebase without manual intervention.

### 4. Observability First
Business logic is instrumented with **OpenTelemetry**.
* **Traces:** Visualize the full path of a request (Frontend -> API -> DB).
* **Metrics:** Track VIN processing latency and success/failure rates.

## 📂 Project Structure

* `AutoMarket.Intake.AppHost`: The Orchestrator (The "Traffic Cop").
* `AutoMarket.Intake.ApiService`: The Core Business Logic (.NET 9).
* `AutoMarket.Intake.Frontend`: The User Interface (React + Vite).
* `AutoMarket.Intake.ServiceDefaults`: Shared configuration for Resilience & Telemetry.

---
*Created as a technical demonstration for AutoMarket Enterprise Technology Services.*