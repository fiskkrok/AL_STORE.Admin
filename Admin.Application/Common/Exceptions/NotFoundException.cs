namespace Admin.Application.Common.Exceptions;
public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base("Entity.NotFound", $"{entityName} with id {id} was not found")
    {
    }
}
