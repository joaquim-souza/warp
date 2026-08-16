# WARP

**WARP — Workflow and Artifact Representation Pipeline**

A deterministic M2M/B2B transformation engine built with .NET.

WARP converts data between different representations through a canonical
intermediate model instead of implementing direct format-to-format converters.

```text
Input
  │
  ▼
Parser
  │
  ▼
Canonical Document
  │
  ▼
Template
  │
  ▼
Transformer
  │
  ▼
Canonical Document
  │
  ▼
Serializer
  │
  ▼
Output

Why WARP?

Traditional format conversion quickly becomes an N×N problem:

CSV  ─────► JSON
 │  ╲       │
 │   ╲      │
 ▼    ╲     ▼
XML ◄─── XLSX

WARP uses a canonical representation:

             ┌── CSV
             │
             ├── JSON
Input ───────┼── XML
             │
             └── ...
                    │
                    ▼
             Canonical Model
                    │
                    ├── XLSX
                    ├── XML
                    ├── JSON
                    └── ...

Adding a new input format requires a parser.

Adding a new output format requires a serializer.

The transformation rules remain in templates.

Current capabilities

WARP currently supports:

Input	Output	Status
CSV	XLSX	✅
JSON	XML	✅
XML	cXML	✅

The project intentionally starts small.

More formats and transformations will be added incrementally.

Design principles
Deterministic

The same input and the same template must produce the same result.

same input
+
same template
+
same engine
=
same output

Non-deterministic requirements are isolated behind explicit abstractions such as IClock.

Template-driven

Transformation rules are data, not application code.

Example:

Mappings:
  - SourcePath: price
    TargetPath: price
    Transform: ToNumber

Changing a mapping does not require recompiling the engine.

Fail-closed

Required fields are explicitly declared.

If a required field is missing, WARP refuses to generate the output.

- SourcePath: SKU
  TargetPath: ItemOut.SKU
  Required: true
No arbitrary scripting

Templates use a deliberately closed set of transformations.

Current transformations:

None
Trim
Upper
Lower
ToNumber
ToDateIso8601

The template system does not execute arbitrary code.

CLI
CSV → Excel
dotnet run --project src/Cli -- convert `
  --input samples/csv/products.csv `
  --template templates/csv-to-xlsx.v1.yaml `
  --output output/products.xlsx
JSON → XML
dotnet run --project src/Cli -- convert `
  --input samples/json/product.json `
  --template templates/json-to-xml.v1.yaml `
  --output output/product.xml
XML → cXML
dotnet run --project src/Cli -- convert `
  --input samples/xml/purchase-order.xml `
  --template templates/xml-to-cxml.v1.yaml `
  --output output/order.xml

The input and output filenames are independent.

This allows integration workflows to rename generated artifacts according to
the conventions required by the destination system.

Templates

Templates define the transformation contract.

Example:

Id: csv-to-xlsx
Version: 1
SourceFormat: csv
TargetFormat: xlsx
RecordsPath: row


Mappings:
  - SourcePath: sku
    TargetPath: sku
    Required: true


  - SourcePath: name
    TargetPath: name
    Required: true


  - SourcePath: price
    TargetPath: price
    Transform: ToNumber

Templates are explicitly versioned.

csv-to-xlsx.v1.yaml
csv-to-xlsx.v2.yaml

Consumers select the version they want.

There is no implicit "latest version".

Architecture
Warp.Core
│
├── Model
│   ├── CanonicalNode
│   └── CanonicalDocument
│
├── Parsing
│   ├── ICanonicalParser
│   ├── CsvParser
│   ├── JsonParser
│   └── XmlParser
│
├── Serialization
│   ├── ICanonicalSerializer
│   └── XmlCanonicalSerializer
│
├── Templates
│   ├── TemplateDefinition
│   ├── FieldMapping
│   ├── TemplateLoader
│   └── TemplateValidator
│
├── Transform
│   ├── Transformer
│   ├── ValidationResult
│   └── ValidationError
│
├── Registry
│   ├── ParserRegistry
│   └── SerializerRegistry
│
├── Engine
│   └── WarpEngine
│
└── Time
    ├── IClock
    ├── SystemClock
    └── FixedClock

Adapters remain outside the Core.

src/adapters/
└── Warp.Excel/
    └── ExcelSerializer

This keeps the engine independent from specific output technologies.

Testing

Run:

dotnet test

The test suite covers parsing, transformation, validation,
serialization and deterministic behavior.

Roadmap

WARP intentionally grows incrementally.

Current focus:

CSV → XLSX
JSON → XML
XML → cXML
deterministic transformation
template validation
CLI execution

Future possibilities include:

additional enterprise formats
richer template validation
additional serializers
additional parsers
integration adapters
stronger B2B envelope support
operational observability

The goal is to prove the M2M/B2B transformation model first and scale the
feature set afterwards.