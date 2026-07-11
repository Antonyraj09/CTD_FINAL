using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Models.Jobs;

public class JobWizardViewModel
{
    public CtdJob? Job { get; set; }
    public bool IsNew => Job is null;

    public IReadOnlyList<Party> Importers { get; set; } = Array.Empty<Party>();
    public IReadOnlyList<Party> Agents { get; set; } = Array.Empty<Party>();
    public IReadOnlyList<Party> Transporters { get; set; } = Array.Empty<Party>();
    public IReadOnlyList<BorderPoint> BorderPoints { get; set; } = Array.Empty<BorderPoint>();
    public IReadOnlyList<Commodity> Commodities { get; set; } = Array.Empty<Commodity>();
    public IReadOnlyList<CustomsHouse> CustomsHouses { get; set; } = Array.Empty<CustomsHouse>();
    public IReadOnlyList<TransitRoute> TransitRoutes { get; set; } = Array.Empty<TransitRoute>();
    public IReadOnlyList<JobChecklistItemDto> DefaultChecklist { get; set; } = Array.Empty<JobChecklistItemDto>();

    public static readonly string[] PortArrivals =
    {
        "Kolkata Sea Port", "Haldia Port", "Visakhapatnam Port", "Patparganj ICD (Delhi)", "Nhava Sheva Port (JNPT)"
    };
    public static readonly string[] ContainerSizes =
    {
        "20ft Standard", "40ft Standard", "40ft High Cube", "20ft Open Top", "20ft Flat Rack", "40ft Reefer"
    };
}
