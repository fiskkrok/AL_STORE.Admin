using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Interfaces;

public interface IIdempotencyService
{
    Task<bool> IsDuplicateRequestAsync(Guid commandRequestId);
    Task<T> GetExistingResultAsync<T>(T commandRequestId);
}
