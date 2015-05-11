
namespace WebMvc.Models
{
    public class HealthCheckEntry
    {
        public string ItemName { get; set; }
        public string ResultDescription { get; set; }
        public bool Result { get; set; }
        public bool OddRow { get; set; }

        public HealthCheckEntry()
        { }

        public HealthCheckEntry(string itemName, string resultDescription, bool result, bool oddRow)
        {
            ItemName = itemName;
            ResultDescription = resultDescription;
            Result = result;
            OddRow = oddRow;
        }

        public string CssClass
        {
            get
            {
                if (!Result) return "fail"; 
                if (OddRow) return "odd";
                return string.Empty;
            } 
        }
  
    }
}