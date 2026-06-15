using System.Text;

namespace BrewUp.Knowledge.Tests;

internal static class PdfTestDocument
{
    public static MemoryStream Create(string? text)
    {
        var pageContent = string.IsNullOrEmpty(text)
            ? string.Empty
            : $"BT /F1 12 Tf 72 720 Td ({Escape(text)}) Tj ET";

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] " +
            "/Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(pageContent)} >>\n" +
            $"stream\n{pageContent}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };

        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1)
                .Append(" 0 obj\n")
                .Append(objects[index])
                .Append("\nendobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n0 ")
            .Append(objects.Length + 1)
            .Append("\n0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
            builder.Append(offset.ToString("D10")).Append(" 00000 n \n");

        builder.Append("trailer\n<< /Size ")
            .Append(objects.Length + 1)
            .Append(" /Root 1 0 R >>\nstartxref\n")
            .Append(xrefOffset)
            .Append("\n%%EOF");

        return new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
    }

    private static string Escape(string text)
        => text.Replace(@"\", @"\\").Replace("(", @"\(").Replace(")", @"\)");
}
