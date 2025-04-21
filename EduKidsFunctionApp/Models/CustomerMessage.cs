using System;
using System.Collections.Generic;

namespace EduKidsFunctionApp.Models;

public partial class CustomerMessage
{
    public int MessageId { get; set; }

    public string? Phone { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }
}
