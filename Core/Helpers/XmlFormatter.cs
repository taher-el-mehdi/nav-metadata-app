using System.Xml;

namespace NAVMetadata.Helpers;

/// <summary>Pretty-prints XML for display in the metadata viewer.</summary>
public static class XmlFormatter
{
    /// <summary>Returns indented XML, or the original string when parsing fails.</summary>
    public static string TryFormat(string xml)
    {
        try
        {
            var document = new XmlDocument { PreserveWhitespace = true };
            document.LoadXml(xml);

            using var writer = new StringWriter();
            using var xmlWriter = new XmlTextWriter(writer) { Formatting = Formatting.Indented };
            document.WriteTo(xmlWriter);
            return writer.ToString();
        }
        catch
        {
            return xml;
        }
    }
}
