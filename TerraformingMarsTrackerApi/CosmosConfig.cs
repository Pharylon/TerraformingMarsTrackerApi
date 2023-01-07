namespace TerraformingMarsTrackerApi
{
    public class CosmosConfig
    {
        public string Endpoint { get; set; } = "";
        public string Key { get; set; } = "";

        public override string ToString() => $"{Endpoint} / {Key}";
    }
}
