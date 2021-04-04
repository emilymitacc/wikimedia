using System;
using System.Text;
using System.Threading.Tasks;

namespace WikiMedia.Core.Interfaces
{
    public interface IStatsOutput
    {
        Task WriteOutput(WikiMediaOutputContext outputContext);
    }
}
