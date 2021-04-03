using System.Collections.Generic;

namespace WikiMedia.Terminal
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
