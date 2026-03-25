using System.Data;
using MapMarkers.Models;
using Microsoft.Data.SqlClient;

namespace MapMarkers.Data;

/// <summary>
/// Pure ADO.NET data access for Markers.
/// Uses SqlConnection / SqlCommand / SqlDataReader — no ORM.
/// </summary>
public class MarkerRepository
{
    private readonly string _connectionString;

    public MarkerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // ---------------------------------------------------------------
    //  GET ALL
    // ---------------------------------------------------------------
    public async Task<IEnumerable<Marker>> GetAllAsync()
    {
        var markers = new List<Marker>();

        const string sql = @"
            SELECT Id, Title, Description, Latitude, Longitude, Color, CreatedAt
            FROM   Markers
            ORDER  BY CreatedAt DESC;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            markers.Add(MapRow(reader));

        return markers;
    }

    // ---------------------------------------------------------------
    //  GET BY ID
    // ---------------------------------------------------------------
    public async Task<Marker?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Title, Description, Latitude, Longitude, Color, CreatedAt
            FROM   Markers
            WHERE  Id = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    // ---------------------------------------------------------------
    //  INSERT
    // ---------------------------------------------------------------
    public async Task<Marker> InsertAsync(CreateMarkerRequest req)
    {
        const string sql = @"
            INSERT INTO Markers (Title, Description, Latitude, Longitude, Color)
            OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.Description,
                   INSERTED.Latitude, INSERTED.Longitude, INSERTED.Color, INSERTED.CreatedAt
            VALUES (@Title, @Description, @Latitude, @Longitude, @Color);";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Title",       SqlDbType.NVarChar, 100).Value = req.Title;
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)req.Description ?? DBNull.Value;
        cmd.Parameters.Add("@Latitude",    SqlDbType.Float).Value          = req.Latitude;
        cmd.Parameters.Add("@Longitude",   SqlDbType.Float).Value          = req.Longitude;
        cmd.Parameters.Add("@Color",       SqlDbType.NVarChar, 20).Value   = req.Color;

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    // ---------------------------------------------------------------
    //  UPDATE
    // ---------------------------------------------------------------
    public async Task<bool> UpdateAsync(int id, CreateMarkerRequest req)
    {
        const string sql = @"
            UPDATE Markers
            SET    Title       = @Title,
                   Description = @Description,
                   Latitude    = @Latitude,
                   Longitude   = @Longitude,
                   Color       = @Color
            WHERE  Id = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id",          SqlDbType.Int).Value             = id;
        cmd.Parameters.Add("@Title",       SqlDbType.NVarChar, 100).Value   = req.Title;
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value   = (object?)req.Description ?? DBNull.Value;
        cmd.Parameters.Add("@Latitude",    SqlDbType.Float).Value            = req.Latitude;
        cmd.Parameters.Add("@Longitude",   SqlDbType.Float).Value            = req.Longitude;
        cmd.Parameters.Add("@Color",       SqlDbType.NVarChar, 20).Value     = req.Color;

        int rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ---------------------------------------------------------------
    //  DELETE
    // ---------------------------------------------------------------
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Markers WHERE Id = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        int rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ---------------------------------------------------------------
    //  PRIVATE HELPER
    // ---------------------------------------------------------------
    private static Marker MapRow(SqlDataReader r) => new()
    {
        Id          = r.GetInt32(r.GetOrdinal("Id")),
        Title       = r.GetString(r.GetOrdinal("Title")),
        Description = r.IsDBNull(r.GetOrdinal("Description")) ? string.Empty : r.GetString(r.GetOrdinal("Description")),
        Latitude    = r.GetDouble(r.GetOrdinal("Latitude")),
        Longitude   = r.GetDouble(r.GetOrdinal("Longitude")),
        Color       = r.GetString(r.GetOrdinal("Color")),
        CreatedAt   = r.GetDateTime(r.GetOrdinal("CreatedAt"))
    };
}
