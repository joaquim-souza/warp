# WARP

**WARP — Workflow-Agnostic Record Processor**

Engine de transformação de dados baseada em templates declarativos.

O WARP recebe dados em um formato de entrada, converte-os para uma representação canônica e aplica um template para produzir um formato de saída.

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
```

## Objetivo

O WARP foi projetado para resolver integrações de dados sem transformar cada integração em código específico.

A regra de transformação fica em YAML.

Uma alteração de mapping normalmente não exige recompilar a engine.

Exemplo:

```yaml
Id: json-to-xml
Version: 1
SourceFormat: json
TargetFormat: xml
OutputRoot: Product

Mappings:
  - SourcePath: product.id
    TargetPath: id
    Required: true

  - SourcePath: product.name
    TargetPath: name
    Required: true

  - SourcePath: product.price
    TargetPath: price
    Required: false
    Transform: ToNumber

  - SourcePath: product.category
    TargetPath: category
    Required: false
```

---

# Arquitetura

```text
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
```

O pipeline central é deliberadamente independente dos formatos específicos:

```text
Input Format
     ↓
ICanonicalParser
     ↓
CanonicalDocument
     ↓
TemplateDefinition
     ↓
Transformer
     ↓
CanonicalDocument
     ↓
ICanonicalSerializer
     ↓
Output Format
```

Isso evita criar conversores diretos entre cada par de formatos.

Em vez de:

```text
CSV → JSON
CSV → XML
JSON → XML
JSON → XLSX
XML → JSON
XML → XLSX
...
```

o WARP utiliza uma representação canônica intermediária.

---

# Core

Localização:

```text
src/Core
```

O Core contém a engine independente da interface de usuário e dos formatos externos.

Principais componentes:

```text
Model
    Representação canônica

Parsing
    Parsers de entrada

Serialization
    Serializers de saída

Templates
    Definição, composição e validação de templates

Transform
    Aplicação dos mappings e transformações

Registry
    Registro e descoberta de parsers e serializers

Engine
    Orquestração da transformação

Time
    Abstração de relógio para operações temporais

Audit
    Contratos de auditoria

Observability
    Observabilidade das transformações
```

O Core não depende da CLI para executar uma transformação.

---

# Adapters

Localização:

```text
src/adapters
```

Contém implementações específicas de formatos externos.

Atualmente:

```text
Warp.Excel
```

O Core não precisa conhecer detalhes específicos do Excel.

---

# Formatos

## Entrada

Atualmente:

* CSV
* JSON
* XML

## Saída

Atualmente:

* XML
* XLSX

Novos formatos devem ser adicionados através das interfaces:

```csharp
ICanonicalParser
```

ou:

```csharp
ICanonicalSerializer
```

O pipeline central não precisa ser alterado para adicionar um novo formato.

---

# Canonical Document

O `CanonicalDocument` representa a estrutura intermediária utilizada pelo WARP.

Seu núcleo é formado por `CanonicalNode`.

Cada nó possui:

* nome;
* valor opcional;
* atributos;
* filhos ordenados.

A ordem dos filhos é preservada através de listas.

Isso é importante para o determinismo da engine.

Exemplo conceitual:

```text
Product
├── id
├── name
├── price
└── category
```

A mesma entrada deve produzir a mesma árvore canônica na mesma ordem.

---

# Templates

Templates ficam em:

```text
templates/
```

Exemplos:

```text
templates/
├── csv-to-xlsx.v1.yaml
├── json-to-xml.v1.yaml
└── xml-to-cxml.v1.yaml
```

Um template possui uma versão explícita:

```yaml
Id: json-to-xml
Version: 1
SourceFormat: json
TargetFormat: xml
```

A versão faz parte do contrato.

O WARP não escolhe automaticamente a versão mais recente.

Isso evita alterações implícitas de comportamento.

---

# Template Validation

Templates são validados antes de serem utilizados pelo Transformer.

A validação verifica, entre outras coisas:

* identificação do template;
* versão;
* formato de entrada;
* formato de saída;
* mappings;
* paths;
* `OutputRoot`;
* `TargetPath` duplicado;
* tipos de transformação válidos.

Exemplo de conflito:

```yaml
Mappings:
  - SourcePath: product.id
    TargetPath: id

  - SourcePath: product.code
    TargetPath: id
```

Esse template é inválido porque dois mappings tentam escrever no mesmo destino.

A validação impede que um mapping sobrescreva silenciosamente o resultado de outro.

---

# Segurança dos Templates

Templates são configuração declarativa.

Eles não executam código arbitrário.

O WARP utiliza o `YamlDotNet` para desserialização da definição do template sem habilitar execução arbitrária de tipos.

Além disso, `SourcePath` e `TargetPath` representam caminhos dentro da árvore canônica.

Eles não representam caminhos de filesystem.

Isso significa que:

```text
../../arquivo
```

não possui semântica de acesso ao sistema de arquivos dentro do Transformer.

Caso o WARP futuramente passe a utilizar valores de templates para operações de filesystem, essa fronteira deverá possuir validação específica contra path traversal.

---

# Mappings

Cada mapping pode possuir:

```yaml
SourcePath:
TargetPath:
Required:
DefaultValue:
Transform:
```

Exemplo:

```yaml
- SourcePath: product.price
  TargetPath: price
  Required: false
  Transform: ToNumber
```

---

# Transformações

As transformações disponíveis são deliberadamente fechadas:

```text
None
Trim
Upper
Lower
ToNumber
ToDateIso8601
```

Templates não executam código arbitrário.

Uma transformação desconhecida não é silenciosamente ignorada.

---

# Campos obrigatórios

Um campo pode ser marcado como:

```yaml
Required: true
```

Se o campo não existir e não houver `DefaultValue`, a transformação falha com erro de validação.

Exemplo:

```text
[Mappings[0].SourcePath]
Campo obrigatório ausente: 'product.id'
```

Essa rigidez é intencional.

O template define o contrato esperado para aquela transformação.

---

# Valores padrão

Mappings podem definir um valor padrão:

```yaml
- SourcePath: product.category
  TargetPath: category
  Required: false
  DefaultValue: unknown
```

Isso permite que determinados campos sejam opcionais sem produzir ausência silenciosa de dados.

---

# Registros repetidos

Templates podem definir:

```yaml
RecordsPath: row
```

Nesse caso, cada `row` é tratado como um registro independente.

Exemplo:

```text
CSV
 │
 ├── row
 ├── row
 └── row
```

O Transformer processa cada registro separadamente.

---

# Collection Mapping

O WARP também suporta mappings de coleções.

Uma collection define uma origem repetível e um destino correspondente:

```yaml
Collections:
  - SourcePath: products
    TargetPath: product
    Mappings:
      - SourcePath: id
        TargetPath: id

      - SourcePath: name
        TargetPath: name
```

Os mappings internos são aplicados ao mesmo elemento de destino, preservando a associação entre os campos daquele item.

Isso permite transformar estruturas repetidas sem perder a relação entre os valores de cada registro.

---

# Herança de Templates

Templates podem estender outros templates:

```yaml
Extends: base-template.v1.yaml
```

O `TemplateLoader` resolve o template base e compõe a definição antes da validação final.

A engine também detecta ciclos de herança.

Exemplo inválido:

```text
A → B → C → A
```

O carregamento falha em vez de entrar em recursão infinita.

---

# CLI

O WARP possui uma CLI para execução e gerenciamento de templates.

## Executar uma transformação

Exemplo:

```powershell
dotnet run --project src/Cli -- `
  --template templates/json-to-xml.v1.yaml `
  --input samples/json/product.json `
  --output samples/output.xml
```

O nome do arquivo de saída é definido pelo usuário.

Isso permite utilizar a mesma transformação em diferentes pipelines de organização de arquivos.

---

# Gerenciamento de templates

## Criar template

```powershell
dotnet run --project src/Cli -- `
  template create `
  --id products `
  --version 1 `
  --source csv `
  --target xml `
  --output templates/products.v1.yaml
```

O comando cria uma definição inicial de template.

---

## Validar template

```powershell
dotnet run --project src/Cli -- `
  template validate templates/products.v1.yaml
```

Exemplo:

```text
Template válido.
ID: products
Version: 1
Source: csv
Target: xml
Mappings: 1
Collections: 0
```

---

## Adicionar mapping

```powershell
dotnet run --project src/Cli -- `
  template add-mapping `
  --template templates/products.v1.yaml `
  --source product.id `
  --target id `
  --required
```

---

## Adicionar collection

```powershell
dotnet run --project src/Cli -- `
  template add-collection `
  --template templates/products.v1.yaml `
  --source products `
  --target product
```

---

# Tratamento de entradas malformadas

O WARP não assume que toda entrada é válida.

O parser CSV, por exemplo, valida a estrutura das linhas.

Se uma linha possuir mais colunas do que o cabeçalho:

```text
CSV inválido: linha possui X colunas, mas o cabeçalho possui Y.
```

Campos ausentes podem ser representados como valores vazios quando a estrutura permite.

O parser também trata campos CSV entre aspas, incluindo campos que contêm separadores e quebras de linha.

Entradas com aspas não fechadas são rejeitadas.

---

# Encoding

A leitura de CSV é realizada com tratamento explícito de encoding e detecção de BOM.

O suporte de encoding deve ser considerado parte do contrato do parser.

Arquivos provenientes de sistemas legados ou exportações antigas de planilhas devem ser tratados de acordo com o encoding efetivamente utilizado pelo arquivo.

---

# Limitações conhecidas

JSON e XML atualmente são carregados em memória durante o parsing.

Isso significa que arquivos extremamente grandes podem consumir uma quantidade significativa de memória.

O WARP não implementa atualmente parsing streaming para documentos JSON/XML de centenas de megabytes.

Essa é uma limitação conhecida da versão 1.0 e não deve ser confundida com comportamento de processamento ilimitado.

Uma futura implementação de streaming poderá ser adicionada sem alterar necessariamente o contrato dos templates.

---

# Determinismo

Uma regra central do WARP é:

```text
mesmo input + mesmo template = mesmo output
```

O Transformer não mantém estado entre chamadas e não utiliza aleatoriedade para produzir resultados.

Quando um formato exige valores temporais ou identificadores únicos, a dependência temporal deve ser explicitamente injetada através de:

```csharp
IClock
```

Testes podem utilizar:

```csharp
FixedClock
```

para manter o resultado reproduzível.

---

# Golden Files

O determinismo também é verificado através de golden files.

Um input e um template conhecidos produzem uma saída armazenada no repositório.

O teste compara a saída atual com o arquivo esperado **byte a byte**.

Exemplo:

```text
Input
  +
Template
  ↓
WARP
  ↓
Output
  ↓
Golden File
  ↓
byte-for-byte comparison
```

Isso detecta regressões envolvendo:

* ordem dos elementos;
* ordem dos atributos;
* serialização;
* formatação;
* valores;
* alterações no Transformer;
* alterações nos serializers;
* comportamento não determinístico.

O golden file funciona como um contrato de saída versionado.

---

# Observabilidade e Auditoria

Transformações podem produzir eventos de auditoria através de:

```csharp
IAuditSink
```

O evento de auditoria contém informações como:

```text
Template ID
Template Version
Source Format
Target Format
Success
Validation Error Count
Duration
```

O Core não depende de uma implementação específica de armazenamento.

Um sink pode futuramente direcionar eventos para:

* arquivo;
* banco;
* mensageria;
* observabilidade;
* OpenTelemetry;
* outro sistema de auditoria.

A falha de um `IAuditSink` não deve derrubar uma transformação que foi concluída com sucesso.

Isso mantém observabilidade desacoplada do processamento principal.

---

# Exemplos

## JSON → XML

Entrada:

```text
samples/json/product.json
```

Template:

```text
templates/json-to-xml.v1.yaml
```

Saída:

```text
samples/output.xml
```

Fluxo:

```text
JSON
 ↓
Canonical Document
 ↓
json-to-xml.v1.yaml
 ↓
XML
```

---

## CSV → XLSX

Entrada:

```text
samples/csv/products.csv
```

Template:

```text
templates/csv-to-xlsx.v1.yaml
```

Saída:

```text
samples/output.xlsx
```

---

## XML → cXML

Entrada:

```text
samples/xml/purchase-order.xml
```

Template:

```text
templates/xml-to-cxml.v1.yaml
```

---

# Testes

Os testes ficam em:

```text
tests/
```

Categorias principais:

```text
tests/
├── Model/
├── Parsing/
├── Serialization/
├── Template/
├── Transform/
├── Integration/
├── Golden/
└── Audit/
```

Executar toda a suíte:

```powershell
dotnet test
```

Build:

```powershell
dotnet build
```

O projeto utiliza testes para validar tanto contratos individuais quanto o comportamento integrado da engine.

A suíte inclui testes para:

* parsers;
* serializers;
* templates;
* mappings;
* collections;
* validação;
* transformação;
* auditoria;
* determinismo;
* golden files;
* integração.

---

# Princípios

## Explicit over implicit

O template declara explicitamente como a transformação deve funcionar.

## Fail closed

Campos obrigatórios ausentes e configurações inválidas não são silenciosamente ignorados.

## Determinismo

O mesmo input e template devem produzir o mesmo resultado.

## Core independente

O Core não depende da CLI ou de implementações específicas de formatos externos.

## Sem execução arbitrária

Templates são configuração declarativa, não scripts.

## Extensibilidade por contrato

Novos formatos são adicionados implementando os contratos existentes, sem alterar o pipeline central.

## Configuração versionada

Templates possuem versão explícita.

Mudanças de contrato podem coexistir através de versões diferentes do template.

## Observabilidade desacoplada

Auditoria e observabilidade não devem controlar o funcionamento do processamento principal.

---

# Estrutura

```text
Warp/
├── src/
│   ├── Core/
│   │   ├── Audit/
│   │   ├── Engine/
│   │   ├── Model/
│   │   ├── Observability/
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
│   ├── Audit/
│   ├── Golden/
│   ├── Integration/
│   ├── Model/
│   ├── Parsing/
│   ├── Serialization/
│   ├── Template/
│   └── Transform/
│
└── Warp.slnx
```

---

# Status

## WARP 1.0.0

A versão 1.0 possui:

* Canonical Document Model
* CSV Parser
* JSON Parser
* XML Parser
* XML Serializer
* Excel/XLSX Serializer
* Parser Registry
* Serializer Registry
* Declarative Templates
* Template Validation
* Template Inheritance
* Field Mapping
* Collection Mapping
* Default Values
* Required Fields
* Multiple Records
* Closed Transform Set
* Deterministic Transformer
* Clock abstraction
* CLI
* Template creation
* Template validation
* CLI mapping management
* CLI collection management
* CSV malformed-input validation
* Explicit encoding handling
* Audit contracts
* Transformation observability
* Golden-file determinism tests
* Integration Tests
* Security-oriented template validation

O WARP 1.0 prioriza **determinismo, contratos explícitos, extensibilidade e previsibilidade de transformação**.

---

# License

See [LICENSE](LICENSE) for the project license.
