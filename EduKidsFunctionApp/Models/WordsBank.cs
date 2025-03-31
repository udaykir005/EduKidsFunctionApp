using System;
using System.Collections.Generic;

namespace EduKidsFunctionApp.Models;

public partial class WordsBank
{
    public int WordId { get; set; }

    public string Word { get; set; } = null!;

    public string Meaning { get; set; } = null!;

    public string Grammar { get; set; } = null!;

    public string ExampleUsage { get; set; } = null!;

    public int? DifficultLevel { get; set; }

    public DateTime? CreatedAt { get; set; }
}
