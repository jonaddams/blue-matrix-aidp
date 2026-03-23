using XTractFlow.API;
using XTractFlow.API.Component;
using XTractFlow.API.Document;
using XTractFlow.API.Document.Fields;
using XTractFlow.API.Document.Fields.Validation;
using XTractFlow.API.LLM.Providers;
using XTractFlow.API.Presets;
using XTractFlow.API.Result;
using System.Text.Json;

// ── Load .env file ──────────────────────────────────────────────────
var envPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            continue;
        var sep = trimmed.IndexOf('=');
        if (sep <= 0)
            continue;
        var key = trimmed[..sep].Trim();
        var val = trimmed[(sep + 1)..].Trim();
        Environment.SetEnvironmentVariable(key, val);
    }
    Console.WriteLine($"Loaded .env from {Path.GetFullPath(envPath)}");
}

// ── Configuration ───────────────────────────────────────────────────
string gdPictureKey = Environment.GetEnvironmentVariable("GDPICTURE_KEY") ?? "";
string openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException(
        "Set OPENAI_API_KEY in the .env file or as an environment variable.");

string pdfPath = Path.Combine("..", "bluematrix-test.pdf");

if (!File.Exists(pdfPath))
{
    Console.Error.WriteLine($"PDF not found at: {Path.GetFullPath(pdfPath)}");
    return;
}

// ── SDK setup ───────────────────────────────────────────────────────
Configuration.RegisterGdPictureKey(gdPictureKey);
Configuration.RegisterLLMProvider(new OpenAIProvider(openAiKey));
Configuration.ResourcesFolder = Path.Combine(AppContext.BaseDirectory, "runtimes", "any", "native");

// ── Build a custom template for the BlueMatrix research report ──────
DocumentTemplate researchReportTemplate = new()
{
    Name = "BlueMatrix Research Report",
    Identifier = "BM-RESEARCH-REPORT-001",
    SemanticDescription = "A financial research report produced by BlueMatrix, "
        + "typically containing analyst ratings, target prices, company analysis, "
        + "and regulatory disclosures for a publicly traded company.",
    Fields = new List<TemplateField>
    {
        new()
        {
            Name = "Report Date",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The date the report was published",
            StandardValidationMethods = new List<StandardFieldValidationMethod>
            {
                new(StandardFieldValidation.DateIntegrity)
            }
        },
        new()
        {
            Name = "Company Name",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The name of the company being analyzed"
        },
        new()
        {
            Name = "Ticker Symbol",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The stock ticker symbol of the company"
        },
        new()
        {
            Name = "Stock Exchange",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The stock exchange where the company is listed (e.g. NYSE, NASDAQ)"
        },
        new()
        {
            Name = "Analyst Name",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The name of the analyst who authored the report"
        },
        new()
        {
            Name = "Analyst Email",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The email address of the analyst"
        },
        new()
        {
            Name = "Firm Name",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The name of the brokerage or securities firm that published the report"
        },
        new()
        {
            Name = "Current Rating",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The current analyst rating for the stock (e.g. Buy, Hold, Sell)"
        },
        new()
        {
            Name = "Previous Rating",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The previous analyst rating for the stock"
        },
        new()
        {
            Name = "Current Target Price",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The current target price for the stock"
        },
        new()
        {
            Name = "Previous Target Price",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The previous target price for the stock"
        },
        new()
        {
            Name = "Dividend Yield",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The dividend yield of the stock"
        },
        new()
        {
            Name = "Analyst Certification",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The analyst certification statement text"
        },
        new()
        {
            Name = "Analyst Owns Securities",
            Format = FieldDataFormat.Text,
            SemanticDescription = "Whether the primary analyst owns securities in the company (yes/no)"
        },
        new()
        {
            Name = "Required Disclosures",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The full text of the required disclosures section of the report"
        },
        new()
        {
            Name = "Ratings Definitions",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The ratings definitions section describing what each rating means"
        },
        new()
        {
            Name = "Important Disclosures",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The full text of the important disclosures section of the report"
        },
        new()
        {
            Name = "Other Disclosures",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The full text of the other disclosures section of the report"
        },
        new()
        {
            Name = "Firm Address",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The street address of the firm"
        },
        new()
        {
            Name = "Firm Phone",
            Format = FieldDataFormat.Text,
            SemanticDescription = "The phone number of the firm"
        }
    }
};

// ── Build the processor component ───────────────────────────────────
ProcessorComponent component = new()
{
    EnableClassifier = false,       // We know it's a research report
    EnableFieldsExtraction = true,
    Templates = new[] { researchReportTemplate }
};

// ── Process the document ────────────────────────────────────────────
Console.WriteLine($"Processing: {Path.GetFullPath(pdfPath)}");
Console.WriteLine(new string('─', 60));

ProcessorResult result = new DocumentProcessor().Process(pdfPath, component);

if (result.ExtractedFields != null)
{
    // Console output
    Console.WriteLine($"{"Field",-25} {"Value",-35} {"Validation"}");
    Console.WriteLine(new string('─', 80));

    foreach (var field in result.ExtractedFields)
    {
        Console.WriteLine($"{field.FieldName,-25} {field.Value,-35} ({field.ValidationState})");
    }

    // JSON output
    var jsonData = result.ExtractedFields.Select(f => new
    {
        field = f.FieldName,
        value = f.Value?.ToString(),
        validation = f.ValidationState.ToString()
    });

    string jsonPath = Path.Combine("..", "bluematrix-result.json");
    string json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(jsonPath, json);
    Console.WriteLine();
    Console.WriteLine($"JSON written to: {Path.GetFullPath(jsonPath)}");
}
else
{
    Console.WriteLine("No fields were extracted.");
}

Console.WriteLine();
Console.WriteLine("Done.");
