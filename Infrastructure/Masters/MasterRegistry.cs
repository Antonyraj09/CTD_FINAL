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
        Tabs.TryGetValue(tab, out var cfg) ? cfg : Tabs["commodity"];

    public static string GetString(object entity, string propName)
    {
        var prop = entity.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(entity)?.ToString() ?? string.Empty;
    }

    public static bool GetBool(object entity, string propName)
    {
        var prop = entity.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(entity) is true;
    }

    public static int GetId(object entity) => (int)(entity.GetType().GetProperty("Id")!.GetValue(entity) ?? 0);

    public static void BindFromForm(object entity, IFormCollection form, List<MasterFieldDef> fields)
    {
        foreach (var field in fields)
        {
            var prop = entity.GetType().GetProperty(field.Key, BindingFlags.Public | BindingFlags.Instance);
            if (prop is null) continue;

            if (field.InputType == "checkbox")
            {
                prop.SetValue(entity, form[field.Key].ToString() == "true");
                continue;
            }

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

        // A checkbox group (e.g. Party's Importer/Transporter/Agent role flags) is only
        // meaningful if at least one box in the group is checked.
        var checkboxFields = fields.Where(f => f.InputType == "checkbox").ToList();
        if (checkboxFields.Count > 0 && !checkboxFields.Any(f => form[f.Key].ToString() == "true"))
            return (false, "At least one role (" + string.Join(" / ", checkboxFields.Select(f => f.Label)) + ")");

        return (true, null);
    }
}
