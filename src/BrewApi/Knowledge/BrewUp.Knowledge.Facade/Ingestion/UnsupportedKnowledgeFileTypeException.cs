namespace BrewUp.Knowledge.Facade.Ingestion;

public sealed class UnsupportedKnowledgeFileTypeException(string extension)
    : NotSupportedException(
        $"Knowledge file type '{extension}' is not supported. Supported file types are .txt and .md.");
