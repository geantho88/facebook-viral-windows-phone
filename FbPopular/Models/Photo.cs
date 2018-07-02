using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FbPopular.Models
{
    public class Photo
    {
        public int LikeCounts { get; set; }
        public string Url { get; set; }
        public double Percentage { get; set; }
    }
}
