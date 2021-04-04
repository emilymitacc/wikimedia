using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WikiMedia.Domain;

namespace WikiMedia.Core.Interfaces
{
    public interface IWikimediaDataReader
    {
        Task<IEnumerable<WikiMediaRow>> GetDataByHourAsync(DateTime dateTime);
    }
}