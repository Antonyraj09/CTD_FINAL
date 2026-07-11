using System.Globalization;
using System.Reflection;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Infrastructure.Masters;

public record MasterFieldDef(string Key, string Label, string InputType, bool Required);

public class MasterTabConfig
{
    public string Key { get; init; } = string.Empty;
    public Type EntityType { get; init; } = null!;
    public string Title { get; init; } = string.Empty;
    public string EntityLabel { get; init; } = string.Empty;
    public List<(string Key, string Label)> Columns { get; init; } = new();
    public List<MasterFieldDef> Fields { get; init; } = new();
}

/// <summary>
/// Server-side equivalent of the prototype's MASTER_CONFIG object — one config
/// entry per master tab, driving the shared generic CRUD controller/service
/// instead of duplicating a controller and service per entity.
/// </summary>
public static class MasterRegistry
{
    public static readonly Dictionary<string, MasterTabConfig> Tabs = new()
    {
        ["importer"] = new MasterTabConfig
        {
            Key = "importer", EntityType = typeof(Importer), Title = "Importer Master", EntityLabel = "Importer",
            Columns = new() { ("Name", "Importer Name"), ("Gstin", "GSTIN"), ("City", "City"), ("Phone", "Phone"), ("Email", "Email") },
            Fields = new()
            {
                new("Name", "Importer Name", "text", true),
                new("Gstin", "GSTIN", "text", true),
                new("City", "City", "text", true),
                new("Phone", "Phone", "text", false),
                new("Email", "Email", "email", false),
            }
        },
        ["agent"] = new MasterTabConfig
        {
            Key = "agent", EntityType = typeof(Agent), Title = "Agent (CHA) Master", EntityLabel = "Agent",
            Columns = new() { ("Name", "Agent Name"), ("License", "CHA License"), ("City", "City"), ("Phone", "Phone"), ("Email", "Email") },
            Fields = new()
            {
                new("Name", "Agent Name", "text", true),
                new("License", "CHA License No.", "text", true),
                new("City", "City", "text", true),
                new("Phone", "Phone", "text", false),
                new("Email", "Email", "email", false),
            }
        },
        ["transporter"] = new MasterTabConfig
        {
            Key = "transporter", EntityType = typeof(Transporter), Title = "Transporter Master", EntityLabel = "Transporter",
            Columns = new() { ("Name", "Transporter Name"), ("Fleet", "Fleet"), ("City", "City"), ("Phone", "Phone"), ("Email", "Email") },
            Fields = new()
            {
                new("Name", "Transporter Name", "text", true),
                new("Fleet", "Fleet Details", "text", false),
                new("City", "City", "text", true),
                new("Phone", "Phone", "text", false),
                new("Email", "Email", "email", false),
            }
        },
        ["commodity"] = new MasterTabConfig
        {
            Key = "commodity", EntityType = typeof(Commodity), Title = "Commodity Master", EntityLabel = "Commodity",
            Columns = new() { ("Name", "Commodity Name"), ("HsCode", "HS Code") },
            Fields = new()
            {
                new("Name", "Commodity Name", "text", true),
                new("HsCode", "HS Code", "text", true),
            }
        },
        ["route"] = new MasterTabConfig
        {
            Key = "route", EntityType = typeof(TransitRoute), Title = "Transit Route Master", EntityLabel = "Route",
            Columns = new() { ("Name", "Route"), ("Distance", "Distance") },
            Fields = new()
            {
                new("Name", "Route Description", "text", true),
                new("Distance", "Approx. Distance", "text", false),
            }
        },
        ["border"] = new MasterTabConfig
        {
            Key = "border", EntityType = typeof(BorderPoint), Title = "Nepal Border Point Master", EntityLabel = "Border Point",
            Columns = new() { ("Name", "Border Point"), ("State", "State / District") },
            Fields = new()
            {
                new("Name", "Border Point Name", "text", true),
                new("State", "State / District", "text", false),
            }
        },
        ["customshouse"] = new MasterTabConfig
        {
            Key = "customshouse", EntityType = typeof(CustomsHouse), Title = "Customs House Master", EntityLabel = "Customs House",
            Columns = new() { ("Name", "Customs House"), ("Code", "Code") },
            Fields = new()
            {
                new("Name", "Customs House Name", "text", true),
                new("Code", "Code", "text", true),
            }
        },
    };

    public static MasterTabConfig Get(string tab) =>
        Tabs.TryGetValue(tab, out var cfg) ? cfg : Tabs["importer"];

    public static string GetString(object entity, string propName)
    {
        var prop = entity.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(entity)?.ToString() ?? string.Empty;
    }

    public static int GetId(object entity) => (int)(entity.GetType().GetProperty("Id")!.GetValue(entity) ?? 0);

    public static void BindFromForm(object entity, IFormCollection form, List<MasterFieldDef> fields)
    {
        foreach (var field in fields)
        {
            var prop = entity.GetType().GetProperty(field.Key, BindingFlags.Public | BindingFlags.Instance);
            if (prop is null) continue;
            var value = form[field.Key].ToString();
            prop.SetValue(entity, string.IsNullOrEmpty(value) ? null : value);
        }
    }

    public static (bool Valid, string? MissingLabel) Validate(IFormCollection form, List<MasterFieldDef> fields)
    {
        foreach (var field in fields.Where(f => f.Required))
        {
            if (string.IsNullOrWhiteSpace(form[field.Key]))
                return (false, field.Label);
        }
        return (true, null);
    }
}
