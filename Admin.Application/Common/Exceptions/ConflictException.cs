using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Exceptions;
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base("Conflict", message)
    {
    }
}