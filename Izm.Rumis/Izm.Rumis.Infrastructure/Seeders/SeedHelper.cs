using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using Izm.Rumis.Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Izm.Rumis.Infrastructure.Seeders
{
    internal class SeedHelper
    {
        public SeedHelper(Guid userId)
        {
            User = userId;
        }

        public readonly Guid User;

        public T Audit<T>(T item) where T : IAuditable
        {
            if (item.CreatedById == Guid.Empty)
            {
                item.CreatedById = User;
                item.Created = DateTime.UtcNow;
            }

            item.Modified = DateTime.UtcNow;
            item.ModifiedById = User;

            return item;
        }

        public IList<T> ReadFromCsv<T>(string fileName) where T : class
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.Seeders.Data.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                var csvItems = csvReader.GetRecords<T>();

                return csvItems.ToList();
            }
        }

        public IList<T> ReadFromXlsx<T>(string fileName) where T : class
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.Seeders.Data.{fileName}";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var xlsx = ExcelReaderFactory.CreateReader(stream))
            {
                var items = new List<T>();

                var ds = xlsx.AsDataSet();
                var dt = ds.Tables[0];

                if (dt.Rows != null)
                {
                    var columnNames = dt.Rows[0].ItemArray.Select(t => t.ToString()).ToList();

                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];
                        var obj = new ExpandoObject() as IDictionary<string, object>;

                        foreach (var colName in columnNames)
                        {
                            var cellValue = row[columnNames.IndexOf(colName)];

                            if (cellValue is string && DateTime.TryParseExact((string)cellValue, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                obj.Add(colName, date);
                            }
                            else
                            {
                                obj.Add(colName, row[columnNames.IndexOf(colName)]);
                            }
                        }

                        var resultObj = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

                        items.Add(resultObj);
                    }
                }

                return items;
            }
        }
    }
}
