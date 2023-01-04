namespace TerraformingMarsTrackerApi.Models
{
    public class Resource
    {
        public int Amount { get; set; }
        public int Production { get; set; }

        public override string ToString()
        {
            return $"{Amount} / {Production}";
        }
    }
}
