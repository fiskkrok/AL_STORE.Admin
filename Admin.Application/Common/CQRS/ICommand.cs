using Admin.Application.Common.Models;

using MediatR;

namespace Admin.Application.Common.CQRS;
public interface ICommand<TResult> : IRequest<Result<TResult>> { }
