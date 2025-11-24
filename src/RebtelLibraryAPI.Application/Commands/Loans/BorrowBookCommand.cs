using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Commands.Loans;

public record BorrowBookCommand(
    Guid BookId,
    Guid BorrowerId
) : IRequest<LoanDto>;