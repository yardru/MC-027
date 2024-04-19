using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_027
{
    [AddINotifyPropertyChangedInterface]
    public class Param
    {
        public string? Description { get; set; }
        public ushort Address { get; set; }
    }
}
