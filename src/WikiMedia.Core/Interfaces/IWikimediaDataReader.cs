using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WikiMedia.Terminal
{
    public interface IWikimediaDataReader
    {
        Task<IEnumerable<WikiMediaRow>> GetDataByHourAsync(DateTime dateTime);
    }
}