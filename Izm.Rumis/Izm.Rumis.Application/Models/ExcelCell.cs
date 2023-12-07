namespace Izm.Rumis.Application.Models
{
    public class ExcelCell
    {
        public object Value { get; }
        public DataType Type { get; }

        /// <summary>
        /// Initialize a string Excel cell.
        /// </summary>
        /// <param name="value"></param>
        public ExcelCell(object value)
        {
            Value = value;
            Type = DataType.String;
        }

        /// <summary>
        /// Initialize explicitly type Excel cell.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        public ExcelCell(object value, DataType type)
        {
            Value = value;
            Type = type;
        }

        public enum DataType
        {
            String,
            Decimal,
            Double,
            Int
        }
    }
}
