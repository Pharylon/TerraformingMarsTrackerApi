namespace TerraformingMarsTrackerApi.Models
{
    public class BoardState
    {
        public Resource MegaCredits { get; set; } = new Resource { Amount = 20 };
        public Resource Steel { get; set; } = new Resource();
        public Resource Plants { get; set; } = new Resource();
        public Resource Titanium { get; set; } = new Resource();
        public Resource Energy { get; set; } = new Resource();
        public Resource Heat { get; set; } = new Resource();
        public int TerraformRating { get; set; }

        public Player Player { get; set; } = new Player();


        public void Produce()
        {
            MegaCredits.Amount = MegaCredits.Amount + MegaCredits.Production + TerraformRating;
            Plants.Amount = Plants.Amount + Plants.Production;
            Steel.Amount = Steel.Amount + Steel.Production;
            Titanium.Amount = Titanium.Amount + Titanium.Production;
            Heat.Amount = Heat.Amount + Energy.Amount + Heat.Production;
            Energy.Amount = Energy.Production;
            Player.ReadyToProduce = false;
        }
    }
}
