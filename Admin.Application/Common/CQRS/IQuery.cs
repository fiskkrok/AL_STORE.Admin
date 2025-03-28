using Admin.Application.Common.Models;
using MediatR;

namespace Admin.Application.Common.CQRS;

public interface IQuery<TResult> : IRequest<Result<TResult>> { }