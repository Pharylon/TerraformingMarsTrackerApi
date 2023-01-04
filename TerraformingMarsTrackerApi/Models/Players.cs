namespace TerraformingMarsTrackerApi.Models
{
    public class Player
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public bool ReadyToStart { get; set; } = false;
        public bool ReadyToProduce { get; set; } = false;
    }
}
