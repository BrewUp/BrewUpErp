# PDF Ingestion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ingest text-based PDF documents through the existing Knowledge file ingestion pipeline.

**Architecture:** Add a PdfPig-backed `IKnowledgeTextExtractor` in Infrastructure and register it with the existing extractors. The current file handler remains responsible for selecting the extractor and then reusing document creation, chunking, embedding, persistence, and vector indexing.

**Tech Stack:** .NET 10, xUnit, PdfPig

---

### Task 1: Specify PDF extraction and pipeline behavior

**Files:**
- Create: `src/BrewApi/Knowledge/BrewUp.Knowledge.Tests/PdfTextExtractorTests.cs`
- Modify: `src/BrewApi/Knowledge/BrewUp.Knowledge.Tests/KnowledgeIngestionTests.cs`

- [ ] Add failing tests for `.pdf` handling, empty PDF errors, unsupported extensions, and full pipeline ingestion.
- [ ] Run the focused tests and confirm they fail because `PdfTextExtractor` is missing and PDF is not registered.

### Task 2: Implement and register the extractor

**Files:**
- Create: `src/BrewApi/Knowledge/BrewUp.Knowledge.Infrastructure/PdfTextExtractor.cs`
- Modify: `src/BrewApi/Knowledge/BrewUp.Knowledge.Infrastructure/KnowledgeInfrastructureHelper.cs`
- Modify: `src/BrewApi/Knowledge/BrewUp.Knowledge.Infrastructure/BrewUp.Knowledge.Infrastructure.csproj`
- Modify: `src/BrewApi/Knowledge/BrewUp.Knowledge.SharedKernel/Exceptions/UnsupportedKnowledgeFileTypeException.cs`

- [ ] Add PdfPig as the lightweight text extraction dependency.
- [ ] Extract page text in content order and preserve page/paragraph separation.
- [ ] Report empty, scanned/image-only, encrypted, and malformed PDFs with clear messages.
- [ ] Register the extractor beside text and Markdown extractors.
- [ ] Update the unsupported-type message to include `.pdf`.

### Task 3: Verify behavior

**Files:**
- Test: `src/BrewApi/Knowledge/BrewUp.Knowledge.Tests/BrewUp.Knowledge.Tests.csproj`

- [ ] Run focused PDF tests.
- [ ] Run the full Knowledge test project.
- [ ] Build the affected Knowledge projects.
