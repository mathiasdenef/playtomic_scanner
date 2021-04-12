using System;
using System.Collections.Generic;
using System.Text;

namespace playtomic_scanner
{
    public class Availability
    {
        public Guid Resource_id { get; set; }
        public DateTime Start_date { get; set; }
        public List<Slot> Slots { get; set; }
    }
}
