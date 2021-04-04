using System;
using System.Collections.Generic;
using System.Text;

namespace WikiMedia.Core.Interfaces
{
    public interface IDateTimeService
    {
        public DateTime UtcNow { get; }
        public DateTime Now { get; }
    }
}
