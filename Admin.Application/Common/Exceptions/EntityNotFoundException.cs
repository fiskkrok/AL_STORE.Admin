using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Exceptions;

public class EntityNotFoundException : AppException
{
    public EntityNotFoundException(string entityName, object id)
        : base("Entity.NotFound",
            $"{entityName} with id {id} was not found",
            entityName, id)
    {
    }
}