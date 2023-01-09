namespace TerraformingMarsTrackerApi.Models
{
    public class UpdateModelNew: IUpdateModel
    {
        public string GameId { get; set; } = "";
        public string Resource { get; set; } = "";
        public int AdjustmentAmount { get; set; }
        public bool Production { get; set; }
    }
}
