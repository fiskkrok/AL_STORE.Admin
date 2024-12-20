using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Admin/Application/Common/Exceptions/AppException.cs
namespace Admin.Application.Common.Exceptions;

public class AppException : Exception
{
    public string Code { get; }
    public object[] Args { get; }

    public AppException(string code, string message, params object[] args)
        : base(message)
    {
        Code = code;
        Args = args;
    }

    public AppException(string code, string message, Exception innerException, params object[] args)
        : base(message, innerException)
    {
        Code = code;
        Args = args;
    }
}