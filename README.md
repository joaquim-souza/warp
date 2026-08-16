# WARP

**WARP — Workflow-Agnostic Record Processor**

Engine de transformação de dados baseada em templates declarativos.

O WARP recebe dados em um formato de entrada, converte-os para uma representação
canônica e aplica um template para produzir um formato de saída.

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

Objetivo

O WARP foi projetado para resolver integrações de dados sem transformar cada
integração em código específico.

A regra de transformação fica em YAML.

Exemplo:

Id: json-to-xml
Version: 1
SourceFormat: json
TargetFormat: xml


Mappings:
  - SourcePath: product.id
    TargetPath: product.id
    Required: true


  - SourcePath: product.name
    TargetPath: product.name
    Required: true


  - SourcePath: product.price
    TargetPath: product.price
    Transform: ToNumber

Assim, uma alteração de mapeamento normalmente não exige recompilar a engine.

Arquitetura
                    ┌─────────────────┐
                    │       CLI       │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │   WarpEngine    │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
         ParserRegistry  Transformer  SerializerRegistry
              │              │              │
              ▼              ▼              ▼
          Canonical      Template       Canonical
          Document       Definition     Document
Core

src/Core

Contém a engine independente de interface de usuário.

Principais componentes:

Model — representação canônica
Parsing — parsers de entrada
Serialization — serializers
Templates — definição e validação dos templates
Transform — aplicação dos mappings
Registry — descoberta dos parsers e serializers
Engine — orquestração
Time — abstração de relógio
Adapters

src/adapters

Implementações específicas de formatos externos.

Atualmente:

Warp.Excel

O Core não precisa conhecer detalhes de Excel.

Formatos
Entrada

Atualmente:

CSV
JSON
XML
Saída

Atualmente:

XML
XLSX

Novos formatos devem ser adicionados através de implementações de:

ICanonicalParser

ou:

ICanonicalSerializer
Templates

Templates ficam em:

templates/

Exemplos:

templates/
├── csv-to-xlsx.v1.yaml
├── json-to-xml.v1.yaml
└── xml-to-cxml.v1.yaml

A versão faz parte explicitamente do template.

Exemplo:

csv-to-xlsx.v1.yaml
csv-to-xlsx.v2.yaml

O WARP não escolhe automaticamente a versão mais recente.

Isso evita mudanças implícitas de comportamento.

Mappings

Cada mapping possui:

SourcePath:
TargetPath:
Required:
DefaultValue:
Transform:

Exemplo:

- SourcePath: price
  TargetPath: product.price
  Required: false
  Transform: ToNumber
Transformações disponíveis
None
Trim
Upper
Lower
ToNumber
ToDateIso8601

O conjunto é deliberadamente fechado.

Templates não executam código arbitrário.

Campos obrigatórios

Um campo pode ser marcado como:

Required: true

Se o campo não existir e não houver DefaultValue, a transformação falha
com erro de validação.

Exemplo:

[documento.PurchaseOrder.OrderID]
Campo obrigatório ausente: 'PurchaseOrder.OrderID'

Essa rigidez é intencional.

O template define o contrato esperado para aquela transformação.

Registros repetidos

Templates podem definir:

RecordsPath: row

Nesse caso, cada row é tratado como um registro independente.

Exemplo:

CSV
 │
 ├── row
 ├── row
 └── row

O Transformer processa cada registro separadamente.

Sem RecordsPath, o documento inteiro é tratado como um único registro.

Determinismo

Uma regra central do WARP é:

mesmo input + mesmo template = mesmo output

O Transformer não mantém estado entre chamadas e não gera valores aleatórios.

Quando um formato exige valores temporais ou identificadores únicos, a
dependência temporal deve ser explicitamente injetada através de:

IClock

Testes podem utilizar:

FixedClock

para manter o resultado reproduzível.

CLI

Exemplo:

dotnet run --project src/Cli -- `
  --template templates/json-to-xml.v1.yaml `
  --input samples/json/product.json `
  --output samples/output.xml

O nome do arquivo de saída é definido pelo usuário.

Isso permite que uma mesma transformação seja utilizada em diferentes
pipelines de organização de arquivos.

Exemplos
JSON → XML

Entrada:

samples/json/product.json

Template:

templates/json-to-xml.v1.yaml

Saída:

samples/output.xml
CSV → XLSX

Entrada:

samples/csv/products.csv

Template:

templates/csv-to-xlsx.v1.yaml

Saída:

samples/output.xlsx
XML → cXML

Entrada:

samples/xml/purchase-order.xml

Template:

templates/xml-to-cxml.v1.yaml
Testes

Os testes ficam em:

tests/

Categorias:

tests/
├── Model/
├── Parsing/
├── Serialization/
├── Template/
├── Transform/
└── Integration/

Executar todos:

dotnet test

Build:

dotnet build
Princípios
Explicit over implicit

O template declara explicitamente como a transformação deve funcionar.

Fail closed

Campos obrigatórios ausentes não são silenciosamente ignorados.

Determinismo

O mesmo input e template devem produzir o mesmo resultado.

Core independente

O Core não depende de CLI, filesystem específico ou implementação de
formato externo.

Sem execução arbitrária

Templates são configuração declarativa, não scripts.

Extensibilidade por contrato

Novos formatos são adicionados implementando os contratos existentes, sem
alterar o pipeline central.

Estrutura
Warp/
├── src/
│   ├── Core/
│   │   ├── Engine/
│   │   ├── Model/
│   │   ├── Parsing/
│   │   ├── Registry/
│   │   ├── Serialization/
│   │   ├── Templates/
│   │   ├── Time/
│   │   └── Transform/
│   │
│   ├── Cli/
│   │
│   └── adapters/
│       └── Warp.Excel/
│
├── templates/
│
├── samples/
│
├── tests/
│
└── Warp.slnx
Status

WARP atualmente possui:

 Canonical Document Model
 CSV Parser
 JSON Parser
 XML Parser
 XML Serializer
 Excel Serializer
 Parser Registry
 Serializer Registry
 Declarative Templates
 Template Validation
 Field Mapping
 Default Values
 Transformations
 Required Fields
 Multiple Records
 Deterministic Transformer
 Clock abstraction
 CLI
 Integration Tests