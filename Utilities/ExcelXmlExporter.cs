using System.Text;
using System.Xml;

namespace ERecruitment.Web.Utilities;

public static class ExcelXmlExporter
{
    public static byte[] BuildMasterListWorkbook(string worksheetName, IReadOnlyCollection<ExcelMasterListRow> rows)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            NewLineOnAttributes = false
        };

        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        {
            writer.WriteStartDocument();
            writer.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
            writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
            writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
            writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
            writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
            writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");

            writer.WriteStartElement("Worksheet");
            writer.WriteAttributeString("ss", "Name", null, SanitizeWorksheetName(worksheetName));

            writer.WriteStartElement("Table");

            WriteHeaderRow(writer);

            foreach (var row in rows)
            {
                WriteDataRow(writer, row);
            }

            writer.WriteEndElement(); // Table
            writer.WriteEndElement(); // Worksheet
            writer.WriteEndElement(); // Workbook
            writer.WriteEndDocument();
        }

        return ms.ToArray();
    }

    private static void WriteHeaderRow(XmlWriter writer)
    {
        writer.WriteStartElement("Row");
        WriteHeaderCell(writer, "No");
        WriteHeaderCell(writer, "Name");
        WriteHeaderCell(writer, "DLR");
        WriteHeaderCell(writer, "Age");
        WriteHeaderCell(writer, "Gender");
        WriteHeaderCell(writer, "Disability");
        WriteHeaderCell(writer, "Qualifications");
        WriteHeaderCell(writer, "Experience");
        WriteHeaderCell(writer, "Comments");
        WriteHeaderCell(writer, "Status");
        WriteHeaderCell(writer, "Submitted");
        WriteHeaderCell(writer, "Email");
        writer.WriteEndElement();
    }

    private static void WriteDataRow(XmlWriter writer, ExcelMasterListRow row)
    {
        writer.WriteStartElement("Row");
        WriteNumberCell(writer, row.Index);
        WriteTextCell(writer, row.Name);
        WriteTextCell(writer, row.Dlr);
        WriteTextCell(writer, row.Age);
        WriteTextCell(writer, row.Gender);
        var disabilityCell = string.IsNullOrWhiteSpace(row.DisabilityNarrative)
            ? row.DisabilityFlag
            : $"{row.DisabilityFlag} - {row.DisabilityNarrative}";
        WriteTextCell(writer, disabilityCell);
        WriteTextCell(writer, row.Qualifications);
        WriteTextCell(writer, row.Experience);
        WriteTextCell(writer, row.Comments);
        WriteTextCell(writer, row.Status);
        WriteTextCell(writer, row.SubmittedAt);
        WriteTextCell(writer, row.Email);
        writer.WriteEndElement();
    }

    private static void WriteHeaderCell(XmlWriter writer, string value)
    {
        writer.WriteStartElement("Cell");
        writer.WriteStartElement("Data");
        writer.WriteAttributeString("ss", "Type", null, "String");
        writer.WriteString(value);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteTextCell(XmlWriter writer, string value)
    {
        writer.WriteStartElement("Cell");
        writer.WriteStartElement("Data");
        writer.WriteAttributeString("ss", "Type", null, "String");
        writer.WriteString(value ?? string.Empty);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteNumberCell(XmlWriter writer, int value)
    {
        writer.WriteStartElement("Cell");
        writer.WriteStartElement("Data");
        writer.WriteAttributeString("ss", "Type", null, "Number");
        writer.WriteString(value.ToString());
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string SanitizeWorksheetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "MasterList";
        }

        var invalidChars = new[] { '\\', '/', '?', '*', '[', ']' };
        var sanitized = new string(name.Where(ch => !invalidChars.Contains(ch)).ToArray());
        sanitized = sanitized.Trim();
        if (sanitized.Length > 31)
        {
            sanitized = sanitized[..31];
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "MasterList" : sanitized;
    }
}

public record ExcelMasterListRow(
    int Index,
    string Name,
    string Email,
    string Dlr,
    string Age,
    string Gender,
    string DisabilityFlag,
    string DisabilityNarrative,
    string Qualifications,
    string Experience,
    string Comments,
    string Status,
    string SubmittedAt);
