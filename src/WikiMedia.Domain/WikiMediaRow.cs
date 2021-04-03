using System;
using System.Collections.Generic;
using System.Text;

namespace WikiMedia.Terminal
{
    public class WikiMediaRow
    {
        public string hour { get; set; }
        public string domain_code { get; set; }
        public string page_title { get; set; }
        public long count_views { get; set; }
        public long total_response_size { get; set; }
    }
}
