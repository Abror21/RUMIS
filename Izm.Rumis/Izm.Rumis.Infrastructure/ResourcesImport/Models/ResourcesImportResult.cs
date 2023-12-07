using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.ResourceImport.Models
{
    public class ResourcesImportResult
    {
        public int Imported { get; set; }

        public List<Error> Errors { get; } = new List<Error>();

        private int MaxErrorCount;  
        public ResourcesImportResult(int maxErrorCount)
        {
            MaxErrorCount = maxErrorCount;
        }

        public bool AddError(string message, int? row = null, string column = null)
        {
            Errors.Add(new Error(message, row, column));

            return Errors.Count > MaxErrorCount;
        }

        public class Error
        {
            public string Message { get; }
            public int? Row { get; }
            public string Column { get; }

            public Error(string message, int? row = null, string column = null)
            {
                Message = message;
                Row = row;
                Column = column;
            }
        }
    }
}
