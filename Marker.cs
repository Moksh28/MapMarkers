namespace MapMarkers.Models;

public class Marker
{
    public int    Id          { get; set; }
    public string Title       { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude    { get; set; }
    public double Longitude   { get; set; }
    public string Color       { get; set; } = "red";
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO used when the client POSTs a new marker.</summary>
public class CreateMarkerRequest
{
    public string Title       { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude    { get; set; }
    public double Longitude   { get; set; }
    public string Color       { get; set; } = "red";
}
