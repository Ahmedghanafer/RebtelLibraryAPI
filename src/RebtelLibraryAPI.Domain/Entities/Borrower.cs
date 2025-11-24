using RebtelLibraryAPI.Domain.Events;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.Domain.Entities;

public class Borrower : Entity<Guid>
{
    private Borrower(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string? phone,
        DateTime registrationDate,
        MemberStatus memberStatus
    ) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        RegistrationDate = registrationDate;
        MemberStatus = memberStatus;
    }

    // Parameterless constructor for EF Core
    private Borrower() : base()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        RegistrationDate = DateTime.UtcNow;
        MemberStatus = MemberStatus.Active;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public MemberStatus MemberStatus { get; private set; }

    public static Borrower Create(
        string firstName,
        string lastName,
        string email,
        string? phone = null
    )
    {
        ValidateFirstName(firstName);
        ValidateLastName(lastName);
        ValidateEmail(email);
        ValidatePhone(phone);

        var borrowerId = Guid.NewGuid();

        var borrower = new Borrower(
            borrowerId,
            SanitizeTextInput(firstName),
            lastName != null ? SanitizeTextInput(lastName) : string.Empty,
            email.ToLowerInvariant().Trim(),
            NormalizePhoneNumber(phone),
            DateTime.UtcNow,
            MemberStatus.Active
        );

        borrower.AddDomainEvent(new BorrowerRegisteredEvent(borrowerId));

        return borrower;
    }

    /// <summary>
    /// Creates a new borrower from a full name (splits into first and last name)
    /// </summary>
    public static Borrower CreateFromFullName(
        string fullName,
        string email,
        string? phone = null
    )
    {
        var (firstName, lastName) = SplitFullName(fullName);
        return Create(firstName, lastName, email, phone);
    }

    public void UpdateContactInfo(string firstName, string lastName, string? phone = null)
    {
        ValidateFirstName(firstName);
        ValidateLastName(lastName);
        ValidatePhone(phone);

        var sanitizedFirstName = SanitizeTextInput(firstName);
        var sanitizedLastName = lastName != null ? SanitizeTextInput(lastName) : string.Empty;
        var hasChanges = false;

        if (FirstName != sanitizedFirstName)
        {
            FirstName = sanitizedFirstName;
            hasChanges = true;
        }

        if (LastName != sanitizedLastName)
        {
            LastName = sanitizedLastName;
            hasChanges = true;
        }

        var normalizedPhone = NormalizePhoneNumber(phone);
        if (Phone != normalizedPhone)
        {
            Phone = normalizedPhone;
            hasChanges = true;
        }

        if (hasChanges)
        {
            UpdateTimestamp();
            AddDomainEvent(new BorrowerUpdatedEvent(Id));
        }
    }

    public void UpdateEmail(string email)
    {
        ValidateEmail(email);
        var newEmail = email.ToLowerInvariant().Trim();

        if (Email == newEmail)
            return;

        Email = newEmail;
        UpdateTimestamp();
        AddDomainEvent(new BorrowerUpdatedEvent(Id));
    }

    public void Activate()
    {
        if (MemberStatus == MemberStatus.Active)
            return;

        MemberStatus = MemberStatus.Active;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (MemberStatus == MemberStatus.Inactive)
            return;

        MemberStatus = MemberStatus.Inactive;
        UpdateTimestamp();
    }

    public void Suspend()
    {
        if (MemberStatus == MemberStatus.Suspended)
            return;

        MemberStatus = MemberStatus.Suspended;
        UpdateTimestamp();
    }

    public string GetFullName()
    {
        return string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";
    }

    public bool CanBorrowBooks()
    {
        return MemberStatus == MemberStatus.Active;
    }


    private static void ValidateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BorrowerValidationException("First name is required");

        var sanitizedFirstName = SanitizeTextInput(firstName);

        if (sanitizedFirstName.Length > 50)
            throw new BorrowerValidationException("First name cannot exceed 50 characters");
    }

    private static void ValidateLastName(string lastName)
    {
        if (lastName != null)
        {
            var sanitizedLastName = SanitizeTextInput(lastName);
            if (sanitizedLastName.Length > 50)
                throw new BorrowerValidationException("Last name cannot exceed 50 characters");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BorrowerValidationException("Email is required");

        if (email.Length > 255)
            throw new BorrowerValidationException("Email cannot exceed 255 characters");

        // Email validation using regex pattern for proper format validation
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailRegex))
            throw new BorrowerValidationException("Invalid email format");
    }

    private static void ValidatePhone(string? phone)
    {
        if (!string.IsNullOrWhiteSpace(phone) && phone.Length > 20)
            throw new BorrowerValidationException("Phone number cannot exceed 20 characters");
    }

    /// <summary>
    /// Normalizes phone number by removing non-digit characters and validating length
    /// </summary>
    private static string? NormalizePhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Remove all non-digit characters
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");

        // Basic validation - check if it's a reasonable length
        if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            throw new BorrowerValidationException("Phone number must be between 10 and 15 digits");

        return digitsOnly;
    }

    /// <summary>
    /// Updates contact info from a full name (splits into first and last name)
    /// </summary>
    public void UpdateContactInfoFromFullName(string fullName, string? phone = null)
    {
        var (firstName, lastName) = SplitFullName(fullName);
        UpdateContactInfo(firstName, lastName, phone);
    }

    /// <summary>
    /// Sanitizes text input to prevent XSS and script injection
    /// </summary>
    private static string SanitizeTextInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove any potentially dangerous HTML/JS characters
        var sanitized = System.Text.RegularExpressions.Regex.Replace(input, @"[<>'""&\\\/()=]", "");

        // Remove common script patterns
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"javascript:", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"on\w+\s*=", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Normalize whitespace
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", " ").Trim();

        return sanitized;
    }

    /// <summary>
    /// Splits a full name into first and last name components
    /// </summary>
    private static (string FirstName, string LastName) SplitFullName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BorrowerValidationException("Name is required");

        var sanitizedName = SanitizeTextInput(name);
        var nameParts = sanitizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length == 1)
        {
            // If only one name provided, use it as first name and empty last name
            return (nameParts[0], string.Empty);
        }

        if (nameParts.Length == 2)
        {
            // First name and last name
            return (nameParts[0], nameParts[1]);
        }

        // Multiple name parts - first word is first name, rest is last name
        var firstName = nameParts[0];
        var lastName = string.Join(" ", nameParts.Skip(1));
        return (firstName, lastName);
    }
}

public enum MemberStatus
{
    Active,
    Inactive,
    Suspended
}
