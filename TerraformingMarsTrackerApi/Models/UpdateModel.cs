namespace TerraformingMarsTrackerApi.Models
{
    public class UpdateModel : IUpdateModel
    {
        public string GameCode { get; set; } = "";
        public string Resource { get; set; } = "";
        public int AdjustmentAmount { get; set; }
        public bool Production { get; set; }
    }
}
