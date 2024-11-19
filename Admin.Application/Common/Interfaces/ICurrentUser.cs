namespace Admin.Application.Common.Interfaces;
public interface ICurrentUser
{
    string? Id { get; }
    string? Name { get; }
    IEnumerable<string> Roles { get; }
}
