namespace Izm.Rumis.Application.Models
{
    public class ExcelColumn
    {
        public string Name { get; }
        public string Type { get; }

        /// <summary>
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type - see DocumentFormat.OpenXml.Spreadsheet.CellValues for available types</param>
        public ExcelColumn(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
