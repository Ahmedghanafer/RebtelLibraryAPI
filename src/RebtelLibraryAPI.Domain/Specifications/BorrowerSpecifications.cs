using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Domain.Specifications;

public class BorrowerMustBeActive : Specification<Borrower>
{
    public override string ErrorMessage => "Borrower must be active to borrow books";

    public override bool IsSatisfiedBy(Borrower borrower)
    {
        return borrower.MemberStatus == MemberStatus.Active;
    }
}

public class BorrowerMustHaveValidName : Specification<Borrower>
{
    public override string ErrorMessage => "Borrower must have valid first and last name (max 50 characters each)";

    public override bool IsSatisfiedBy(Borrower borrower)
    {
        return !string.IsNullOrWhiteSpace(borrower.FirstName) &&
               !string.IsNullOrWhiteSpace(borrower.LastName) &&
               borrower.FirstName.Length <= 50 &&
               borrower.LastName.Length <= 50;
    }
}

public class BorrowerMustHaveValidEmail : Specification<Borrower>
{
    public override string ErrorMessage => "Borrower must have a valid email address";

    public override bool IsSatisfiedBy(Borrower borrower)
    {
        return !string.IsNullOrWhiteSpace(borrower.Email);
    }
}