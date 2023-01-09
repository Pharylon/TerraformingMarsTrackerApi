namespace TerraformingMarsTrackerApi.Models
{
    public interface IUpdateModel
    {
        public string Resource { get; set; }
        public int AdjustmentAmount { get; set; }
        public bool Production { get; set; }
    }
}
