using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_Projekat_2
{
     struct PalindromeResponse
     {
        public PalindromeResponse() 
        {
            Results = Array.Empty<Tuple<string, int>>();
            LastRequested = DateTime.Now;
        }

        public PalindromeResponse(Tuple<string, int>[] results)
        {
            Results = results;
            LastRequested = DateTime.Now;
        }

        public PalindromeResponse(PalindromeResponse otherResponse)
        {
            Results = otherResponse.Results;
            LastRequested = otherResponse.LastRequested;
        }

        public Tuple<string, int>[] Results { get; set; }
        public DateTime LastRequested { get; set; }
     }
}   
