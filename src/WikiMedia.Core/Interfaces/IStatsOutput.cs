using System;
using System.Text;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    public interface IStatsOutput
    {
        Task WriteOutput(WikiMediaOutputContext outputContext);
    }
}
