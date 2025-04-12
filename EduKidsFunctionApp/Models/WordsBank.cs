using System;
using System.Collections.Generic;

namespace EduKidsFunctionApp.Models;

public partial class WordsBank
{
    public int WordId { get; set; }

    public string Word { get; set; } = null!;

    public string? Meaning { get; set; }

    public string Grammar { get; set; } = null!;

    public string? ExampleUsage { get; set; }

    public int? DifficultLevel { get; set; }

    public DateTime? CreatedAt { get; set; }
}
