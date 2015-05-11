
using System.Collections.Generic;

namespace WebMvc.Models
{
    public class HealthCheckSection
    {
        public string Title { get; set; }
        public List<HealthCheckEntry> Entries { get; set; } 
    }
}