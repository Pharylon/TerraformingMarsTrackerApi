namespace TerraformingMarsTrackerApi.Models
{
    public class UpdateModel
    {
        public string GameCode { get; set; } = "";
        public string Resource { get; set; } = "";
        public int AdjustmentAmount { get; set; }
        public bool Production { get; set; }
    }
}
