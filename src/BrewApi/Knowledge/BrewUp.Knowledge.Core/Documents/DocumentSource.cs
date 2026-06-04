using BrewUp.Shared.Helpers;

namespace BrewUp.Knowledge.Core.Documents;

public sealed class DocumentSource(int id, string name) : Enumeration(id, name)
{
    public static DocumentSource Pdf = new (0, nameof(Pdf).ToLowerInvariant());
    public static DocumentSource Markdown = new (1, nameof(Markdown).ToLowerInvariant());
    public static DocumentSource Word = new (2, nameof(Word).ToLowerInvariant());
    public static DocumentSource WebPage = new (3, nameof(WebPage).ToLowerInvariant());
    
    public static IEnumerable<DocumentSource> List() => [Pdf, Markdown, Word, WebPage];
    
    public static DocumentSource FromName(string name)
    {
        var documentSource = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return documentSource ??
               throw new Exception($"Possible values for DocumentSource: {string.Join(",", List().Select(s => s.Name))}");
    }

    public static DocumentSource From(int id)
    {
        var documentSource = List().SingleOrDefault(s => s.Id == id);

        return documentSource ??
               throw new Exception($"Possible values for DocumentSource: {string.Join(",", List().Select(s => s.Name))}");
    }
}