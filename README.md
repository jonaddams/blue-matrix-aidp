# BlueMatrix AI Document Processing Sample

This sample shows how to use the [Nutrient AI Document Processing SDK](https://www.nutrient.io/guides/ai-document-processing/) (formerly XtractFlow) to automatically extract structured data from a financial research report PDF.

The included sample PDF (`bluematrix-test.pdf`) is a BlueMatrix-generated equity research report. The program reads the PDF, sends it through the SDK, and outputs the extracted fields as both a console table and a JSON file.

## What Gets Extracted

The program extracts 20 fields from the research report, including:

- **Report metadata** — date, company name, ticker symbol, exchange
- **Analyst info** — name, email, firm, certification statement
- **Ratings** — current/previous rating, current/previous target price, dividend yield
- **Disclosures** — required, important, ratings definitions, other disclosures
- **Firm details** — address, phone number

## Prerequisites

You need two things before running this sample:

### 1. .NET 8 SDK

**macOS (Homebrew):**
```bash
brew install dotnet@8
```

After installing, add this to your shell profile (`~/.zprofile` or `~/.bash_profile`):
```bash
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"
export PATH="$DOTNET_ROOT:$PATH"
```

Then restart your terminal or run `source ~/.zprofile`.

**Windows:**
Download from https://dotnet.microsoft.com/download/dotnet/8.0

**Verify it works:**
```bash
dotnet --version
```

You should see `8.0.x`.

### 2. An OpenAI API Key

The SDK uses OpenAI for its LLM-powered extraction. You need an API key with available credits.

1. Go to https://platform.openai.com/signup and create an account (or sign in)
2. Go to https://platform.openai.com/api-keys and create a new secret key
3. Make sure you have credits — go to https://platform.openai.com/settings/organization/billing

## Setup

### Step 1: Clone this repository

```bash
git clone <this-repo-url>
cd blue-matrix-aidp
```

### Step 2: Create your `.env` file

```bash
cd BlueMatrixSample
cp .env.example .env
```

Open `.env` in any text editor and paste in your keys:

```
GDPICTURE_KEY=your-gdpicture-key-here
OPENAI_API_KEY=sk-your-openai-key-here
```

> **Note:** The `GDPICTURE_KEY` can be left empty to run in demo/trial mode. The `OPENAI_API_KEY` is required.

### Step 3: Restore packages

```bash
dotnet restore
```

This downloads the Nutrient AI Document Processing SDK and its dependencies (OCR models, language packs, etc.). It may take a minute the first time.

## Running the Sample

From the `BlueMatrixSample` folder:

```bash
dotnet run
```

Or from the repo root:

```bash
dotnet run --project BlueMatrixSample
```

### Expected Output

You will see a table in the console:

```
Processing: /path/to/bluematrix-test.pdf
────────────────────────────────────────────────────────────
Field                     Value                               Validation
────────────────────────────────────────────────────────────────────────────────
Report Date               August 18, 2022                     (Valid)
Company Name              A- Company                          (Valid)
Ticker Symbol             AC1                                 (Valid)
...
```

A JSON file (`bluematrix-result.json`) is also written to the repo root with the same data:

```json
[
  {
    "field": "Report Date",
    "value": "August 18, 2022",
    "validation": "Valid"
  },
  ...
]
```

## Project Structure

```
blue-matrix-aidp/
├── bluematrix-test.pdf          # Sample BlueMatrix research report
├── BlueMatrixSample/
│   ├── Program.cs               # Main program (all the code is here)
│   ├── BlueMatrixSample.csproj  # Project file with NuGet dependencies
│   ├── .env.example             # Template for your API keys
│   └── .gitignore               # Keeps .env and build output out of git
├── .gitignore
└── README.md
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `dotnet: command not found` | Install .NET 8 SDK (see Prerequisites above) |
| `Set OPENAI_API_KEY in the .env file` | Create a `.env` file with your OpenAI key (see Setup) |
| `HTTP 429 (insufficient_quota)` | Your OpenAI account needs credits — add billing at https://platform.openai.com/settings/organization/billing |
| `Unable to locate the latin dictionary` | Run `dotnet restore` to download the resource packages |
| `Couldn't find a project to run` | Make sure you're in the `BlueMatrixSample` folder, or use `dotnet run --project BlueMatrixSample` |

## Learn More

- [Nutrient AI Document Processing Guides](https://www.nutrient.io/guides/ai-document-processing/)
- [API Reference](https://www.nutrient.io/api/xtractflow/)
- [Getting Started Guide](https://www.nutrient.io/sdk/ai-document-processing/getting-started/)
- [Custom Template Guide](https://www.nutrient.io/guides/ai-document-processing/custom-templates/)
