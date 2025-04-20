using System;
using System.Collections.Generic;

namespace EduKidsFunctionApp.Models;

public partial class CustomerContact
{
    public int ContactId { get; set; }

    public string? UserName { get; set; }

    public string? MotherName { get; set; }

    public string? FatherName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int RegistrationStep { get; set; }

    public bool Subscribed { get; set; }
}
