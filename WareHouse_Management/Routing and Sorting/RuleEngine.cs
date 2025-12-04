namespace WareHouse_Management
{
    public class RuleEngine
    {
        public string DetermineRoute(string barcode)
        {
            return barcode.EndsWith("A") ? "Zone A" : "Zone B";
        }
    }
}