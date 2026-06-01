using BrewUp.Shared.Helpers;

namespace BrewUp.Mother.Contracts;

public sealed class MotherAnalysisStatus(int id, string name) : Enumeration(id, name)
{
    public static MotherAnalysisStatus Completed = new (0, nameof(Completed).ToLowerInvariant());
    public static MotherAnalysisStatus NeedsClarification = new (1, nameof(NeedsClarification).ToLowerInvariant());
    public static MotherAnalysisStatus NotEnoughData = new (2, nameof(NotEnoughData).ToLowerInvariant());
    public static MotherAnalysisStatus Failed = new (3, nameof(Failed).ToLowerInvariant());
    
    public static IEnumerable<MotherAnalysisStatus> List() => new[] { Completed, NeedsClarification, NotEnoughData, Failed };

    public static MotherAnalysisStatus FromName(string name)
    {
        var motherAnalysisStatus = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return motherAnalysisStatus ??
               throw new Exception($"Possible values for MotherAnalysisStatus: {string.Join(",", List().Select(s => s.Name))}");
    }

    public static MotherAnalysisStatus From(int id)
    {
        var motherAnalysisStatus = List().SingleOrDefault(s => s.Id == id);

        return motherAnalysisStatus ??
               throw new Exception($"Possible values for MotherAnalysisStatus: {string.Join(",", List().Select(s => s.Name))}");
    }
}