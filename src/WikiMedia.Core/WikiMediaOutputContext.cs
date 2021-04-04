using System.Collections.Generic;
using WikiMedia.Domain;

namespace WikiMedia.Core
{
    public class WikiMediaOutputContext
    {
        public WikiMediaOutputContext(IEnumerable<WikiMediaRow> wikiMediaRows)
        {
            WikiMediaRows = wikiMediaRows;
        }

        public IEnumerable<WikiMediaRow> WikiMediaRows { get; private set; }
    }
}
