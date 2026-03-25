# Map Markers — ASP.NET Core + ADO.NET + AJAX

A full-stack web application demonstrating:
- **ASP.NET Core 8** Web API (REST)
- **ADO.NET** (raw `SqlConnection` / `SqlCommand` / `SqlDataReader`) — no ORM
- **Google Maps JS API** (Advanced Markers)
- **jQuery AJAX** for all client ↔ server communication
- **SQL Server** persistence

---

## Project Structure

```
MapMarkers/
├── Controllers/
│   └── MarkersController.cs     # REST API (GET, POST, PUT, DELETE)
├── Data/
│   └── MarkerRepository.cs      # All ADO.NET database access
├── Models/
│   └── Marker.cs                # Marker + CreateMarkerRequest models
├── wwwroot/
│   └── index.html               # SPA — Google Maps
|   └── app.js                   # jQuery AJAX
├── Database/
│   └── create_markers_table.sql # SQL script to create table + seed data
├── Program.cs                   # App startup
├── appsettings.json             # Connection string + Google Maps API key
└── MapMarkers.csproj
```

---

## Setup

### 1. SQL Server

Run `Database/create_markers_table.sql` against your SQL Server instance.
This creates the `Markers` table and inserts 8 seed locations.

```sql
-- The table schema:
CREATE TABLE Markers (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(100)  NOT NULL,
    Description NVARCHAR(500)  NULL,
    Latitude    FLOAT          NOT NULL,
    Longitude   FLOAT          NOT NULL,
    Color       NVARCHAR(20)   NOT NULL DEFAULT 'red',
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
```

### 2. Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MapMarkersDB;User Id=sa;Password=YourPass;TrustServerCertificate=True;"
  },
  "GoogleMaps": {
    "ApiKey": "YOUR_GOOGLE_MAPS_API_KEY" // for testing i have added my creds so, no need to change it.
  }
}
```

For Windows Authentication use:
```
"Server=.;Database=MapMarkersDB;Integrated Security=True;TrustServerCertificate=True;"
```


### 3. Run the App

```bash
dotnet restore
dotnet run
```

Navigate to `https://localhost:5001` (or the port shown in the console).

---

## API Endpoints

| Method | Endpoint              | Description            |
|--------|-----------------------|------------------------|
| GET    | `/api/markers`        | Return all markers     |
| GET    | `/api/markers/{id}`   | Return one marker      |
| POST   | `/api/markers`        | Create a new marker    |
| PUT    | `/api/markers/{id}`   | Update a marker        |
| DELETE | `/api/markers/{id}`   | Delete a marker        |
| GET    | `/api/markers/apikey` | Return Google Maps key |

### POST / PUT Payload

```json
{
  "title":       "Eiffel Tower",
  "description": "Iconic iron lattice tower in Paris, France.",
  "latitude":    48.8584,
  "longitude":   2.2945,
  "color":       "red"
}
```

Available colors: `red` `blue` `green` `yellow` `purple` `orange` `pink` `cyan` `white` `black`

---

## How It Works

### ADO.NET Flow (no ORM)

```csharp
// Example: Insert using raw ADO.NET
await using var conn = new SqlConnection(_connectionString);
await conn.OpenAsync();

await using var cmd = new SqlCommand(sql, conn);
cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 100).Value = req.Title;
// ... other params
await using var reader = await cmd.ExecuteReaderAsync();
```

### AJAX Flow (jQuery)

```javascript
// POST new marker
$.ajax({
  url: '/api/markers',
  method: 'POST',
  contentType: 'application/json',
  data: JSON.stringify(payload),
  success(data) { addMarkerToUI(data); }
});
```

### Features

- **Click the map** to auto-fill latitude/longitude coordinates
- **Drag** the temporary pin to fine-tune position before saving
- **Color picker** — 10 pin colors
- **Sidebar list** — click any row to pan/zoom the map to that marker
- **Edit** — pre-fills the form, sends PUT request
- **Delete** — confirms then sends DELETE, removes pin from map
- **Toast notifications** for all CRUD operations
