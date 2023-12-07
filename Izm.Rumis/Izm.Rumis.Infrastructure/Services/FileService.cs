using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Services
{
    public class FileServiceOptions
    {
        public string AccessDbProvider { get; set; }
        public string S3BucketName { get; set; }
    }

    public class FileService : IFileService
    {
        private readonly IAppDbContext db;
        private readonly IAmazonS3 s3;
        private readonly ILogger<FileService> logger;
        private readonly FileServiceOptions options;

        public FileService(IAppDbContext db, IAmazonS3 s3, FileServiceOptions options, ILogger<FileService> logger)
        {
            this.db = db;
            this.s3 = s3;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">Exception thrown when file is not found.</exception>
        public async Task<FileEntry> GetAsync(Guid id)
        {
            ValidateId(id);

            try
            {
                var model = await db.Files.Where(t => t.Id == id).Select(t => new FileEntry
                {
                    Content = t.Content,
                    ContentType = t.ContentType,
                    Extension = t.Extension,
                    Id = t.Id,
                    Length = t.Length,
                    Name = t.Name,
                    SourceType = t.SourceType,
                    BucketName = t.BucketName
                }).FirstOrDefaultAsync();

                if (model == null)
                    throw new EntityNotFoundException();

                if (model.SourceType == FileSourceType.S3)
                {
                    var response = await s3.GetObjectAsync(new GetObjectRequest
                    {
                        BucketName = model.BucketName,
                        Key = GetKeyObject(model.Id, model.Extension)
                    });

                    using var ms = new MemoryStream();

                    await response.ResponseStream.CopyToAsync(ms);

                    model.Content = ms.ToArray();
                }

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("file.notFound", ex);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Application.Exceptions.ValidationException"></exception>
        /// <exception cref="Exception">Exception thrown when the database save operation failed.</exception>
        public async Task AddOrUpdateAsync(Guid id, FileDto file)
        {
            ValidateId(id);

            if (file == null || file.Content == null || file.Content.Length == 0)
                throw new Application.Exceptions.ValidationException("file.emptyContent");

            if (string.IsNullOrEmpty(file.FileName))
                throw new Application.Exceptions.ValidationException("file.emptyFileName");

            try
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                var entity = await db.Files.FirstOrDefaultAsync(t => t.Id == id);

                if (entity == null)
                {
                    entity = new Domain.Entities.File
                    {
                        Id = id,
                        SourceType = file.SourceType,
                        BucketName = file.SourceType == FileSourceType.S3 ? options.S3BucketName : null
                    };

                    await db.Files.AddAsync(entity);
                }

                switch (entity.SourceType)
                {
                    case FileSourceType.S3:
                        await EnsureBucketExists(entity.BucketName);

                        using (var ms = new MemoryStream(file.Content))
                        {
                            await s3.PutObjectAsync(new PutObjectRequest
                            {
                                BucketName = entity.BucketName,
                                Key = GetKeyObject(entity.Id, ext),
                                InputStream = ms,
                                ContentType = file.ContentType
                            });
                        }

                        break;

                    case FileSourceType.Database:
                        entity.Content = file.Content;
                        break;

                    default:
                        break;
                }

                entity.ContentType = file.ContentType;
                entity.Extension = ext;
                entity.Length = file.Content.Length;
                entity.Name = file.FileName;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("file.addOrUpdateFailed", ex);
            }
        }

        private async Task EnsureBucketExists(string bucketName)
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucketName);

            if (!exists)
                await s3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">Exception thrown when the database save operation failed.</exception>
        public async Task DeleteAsync(Guid id)
        {
            ValidateId(id);

            try
            {
                var entity = await db.Files.FirstOrDefaultAsync(t => t.Id == id);

                if (entity == null)
                    throw new EntityNotFoundException();

                if (entity.SourceType == FileSourceType.S3)
                    await s3.DeleteObjectAsync(entity.BucketName, GetKeyObject(entity.Id, entity.Extension));

                db.Files.Remove(entity);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("file.deleteFailed", ex);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">Exception thrown when the database save operation failed.</exception>
        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await db.Files
                    .Where(t => ids.Any(i => i == t.Id))
                    .ToArrayAsync(cancellationToken);

                if (!entities.Any())
                    throw new EntityNotFoundException();

                var bucketNames = entities
                    .Where(t => t.SourceType == FileSourceType.S3)
                    .GroupBy(t => t.BucketName)
                    .ToArray();

                foreach (var bucketName in bucketNames)
                {
                    var deleteObjects = new DeleteObjectsRequest
                    {
                        BucketName = bucketName.Key
                    };

                    deleteObjects.Objects.AddRange(entities
                        .Where(t => t.BucketName == bucketName.Key && t.SourceType == FileSourceType.S3)
                        .Select(t => new KeyVersion
                        {
                            Key = GetKeyObject(t.Id, t.Extension)
                        }).ToList());

                    await s3.DeleteObjectsAsync(deleteObjects, cancellationToken);
                }

                db.Files.RemoveRange(entities);

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("file.deleteFailed", ex);
            }
        }

        /// <inheritdoc/>
        public byte[] ResizeImage(byte[] image, int width, int height, int quality, bool fit = false)
        {
            using (var img = Image.Load(image))
            {
                var dim = fit
                    ? new Dimensions { Height = height, Width = width }
                    : ResizeDimensions(img.Width, img.Height, width, height);

                var resizeOptions = new ResizeOptions
                {
                    Mode = fit ? (img.Width > dim.Width || img.Height > dim.Height) ? ResizeMode.Crop : ResizeMode.Stretch : ResizeMode.Max,
                    Size = new Size { Height = dim.Height, Width = dim.Width }
                };

                //var exifOrientation = img.Metadata?.ExifProfile?.GetValue(ExifTag.Orientation);

                //if (exifOrientation != null)
                //{
                //    //RotateMode rotateMode;
                //    //FlipMode flipMode;

                //    //SetRotateFlipMode(exifOrientation, out rotateMode, out flipMode);

                //    img.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.None));
                //    img.Metadata.ExifProfile.SetValue(ExifTag.Orientation, (ushort)1);
                //}

                img.Mutate(t => t.AutoOrient().Resize(resizeOptions));

                using (var destStream = new MemoryStream())
                {
                    img.Save(destStream, new JpegEncoder
                    {
                        Quality = quality
                    });

                    return destStream.ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public byte[] HtmlToPdf(string html)
        {
            const string WkHtmlToPdfExecPath = "/usr/bin/wkhtmltopdf";

            Process proc;
            var psi = new ProcessStartInfo();

            psi.FileName = WkHtmlToPdfExecPath;
            psi.WorkingDirectory = Path.GetDirectoryName(psi.FileName);

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            var argList = new string[] {
                "-q", // be quiet
                "-n", // no scripts
                "--outline-depth 0",
                "--encoding utf-8",
                "--enable-smart-shrinking",
                "--footer-right [page]",
                "- -" // send file to stdout
            };

            psi.Arguments = string.Join(" ", argList);

            proc = Process.Start(psi);

            try
            {
                using (StreamWriter stdin = proc.StandardInput)
                {
                    var utf8Bytes = new UTF8Encoding().GetBytes(html);

                    stdin.AutoFlush = true;
                    stdin.BaseStream.Write(utf8Bytes, 0, utf8Bytes.Length);
                }

                //read output
                byte[] buffer = new byte[32768];
                byte[] file;

                using (var ms = new MemoryStream())
                {
                    while (true)
                    {
                        int read = proc.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                        if (read <= 0)
                            break;

                        ms.Write(buffer, 0, read);
                    }

                    file = ms.ToArray();
                }

                proc.StandardOutput.Close();

                // wait or exit
                proc.WaitForExit(30000);

                var error = new StringBuilder();
                string errorLine = proc.StandardError.ReadLine();

                while (errorLine != null)
                {
                    error.AppendLine(errorLine);
                    errorLine = proc.StandardError.ReadLine();
                }

                // read the exit code, close process
                int returnCode = proc.ExitCode;

                proc.Close();

                return returnCode == 0 ? file : throw new Exception(error.ToString());
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> ReadFromCsv<T>(Stream stream) where T : class
        {
            var data = new List<T>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<T>();

                foreach (var record in records)
                {
                    var row = record;
                    data.Add(row);
                }
            }

            return data;
        }

        /// <inheritdoc/>
        public IEnumerable<T> ReadFromXlsx<T>(Stream stream, int startRowNumber) where T : class
        {
            var data = new List<T>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var ds = reader.AsDataSet();

                foreach (DataTable dt in ds.Tables)
                {
                    if (dt.Rows != null && dt.Rows.Count > startRowNumber)
                    {
                        var columnNames = dt.Rows[startRowNumber].ItemArray.Select(el => el.ToString()).ToList();

                        for (int i = startRowNumber + 1; i < dt.Rows.Count; i++)
                        {
                            var row = dt.Rows[i];
                            var obj = new ExpandoObject() as IDictionary<string, object>;

                            foreach (var colName in columnNames)
                            {
                                obj.Add(colName, row[columnNames.IndexOf(colName)]);
                            }

                            var resultObj = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

                            data.Add(resultObj);
                        }
                    }
                }
            }

            return data;
        }

        /// <inheritdoc/>
        public IEnumerable<IDictionary<string, object>> ReadFromXlsx(Stream stream, int startRowNumber)
        {
            var data = new List<Dictionary<string, object>>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var ds = reader.AsDataSet();

                foreach (DataTable dt in ds.Tables)
                {
                    if (dt.Rows != null && dt.Rows.Count >= startRowNumber)
                    {
                        var colNames = new Dictionary<int, string>();

                        for (int col = 0; col < dt.Columns.Count; col++)
                        {
                            string colName = GetExcelColumnName(col + 1);
                            colNames.Add(col, colName);
                        }

                        for (int row = startRowNumber; row < dt.Rows.Count; row++)
                        {
                            var rowData = new Dictionary<string, object>();

                            for (int col = 0; col < dt.Columns.Count; col++)
                            {
                                rowData.Add(colNames[col], dt.Rows[row][col]);
                            }

                            data.Add(rowData);
                        }
                    }

                    //    for (int col = 0; col < dt.Columns.Count; col++)
                    //{
                    //    var colValues = new List<object>();
                    //    for (int row = startRowNumber; row < dt.Rows.Count; row++)
                    //    {
                    //        colValues.Add(dt.Rows[row][col]);
                    //    }

                    //    string colCode = GetExcelColumnName(col + 1);
                    //    data.Add(colCode, colValues);
                    //}
                }
            }

            return data;
        }

        /// <inheritdoc/>
        public IEnumerable<IDictionary<string, object>> ReadFromAccess(Stream stream, string table)
        {
            var rows = new List<Dictionary<string, object>>();
            var colNames = new List<string>();

            UsingAccessDb(stream, connection =>
            {
                var cols = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new string[] { null, null, table });

                foreach (DataRow col in cols.Rows)
                    colNames.Add(col["COLUMN_NAME"].ToString());

                var command = new OleDbCommand($"select * from [{table}];", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var values = new Dictionary<string, object>();

                    foreach (var col in colNames)
                        values.Add(col, reader[col]);

                    rows.Add(values);
                }
            });

            return rows;
        }

        /// <inheritdoc/>
        public IEnumerable<string> ReadAccessColumnNames(Stream stream, string table)
        {
            var colNames = new List<string>();

            UsingAccessDb(stream, connection =>
            {
                var cols = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new string[] { null, null, table });

                foreach (DataRow col in cols.Rows)
                    colNames.Add(col["COLUMN_NAME"].ToString());
            });

            return colNames;
        }

        /// <inheritdoc/>
        public byte[] WriteToAccess(IEnumerable<Array> rows, IEnumerable<AccessColumn> columns, string table)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.Templates.blank.accdb";

            string filePath = null;

            try
            {
                filePath = Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".accdb";

                logger.LogDebug($"Prepare a blank database at {filePath}.");

                var colSql = string.Join(",", columns.Select(t => $"[{t.Name}] {t.Type}"));
                var tableSql = $"create table [{table}] ({colSql});";
                var insertSql = $"insert into [{table}] values({string.Join(",", columns.Select(t => "?"))});";

                // create a blank database copy
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var fs = File.Create(filePath))
                    stream.CopyTo(fs);

                var connectionString = BuildAccessDbConnectionString(filePath, "ReadWrite");

                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    logger.LogDebug($"Opened OLEDB connection on {filePath}.");

                    logger.LogDebug($"Create a table {table} in {filePath}.");

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = tableSql;
                        cmd.ExecuteNonQuery();
                    }

                    var cols = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new string[] { null, null, table });

                    logger.LogDebug($"Insert {rows.Count()} rows in {filePath}.");

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = insertSql;

                        var p = 0;

                        foreach (DataRow col in cols.Rows)
                        {
                            var colType = (OleDbType)col["DATA_TYPE"];
                            var para = new OleDbParameter();

                            para.ParameterName = $"p{p}";
                            para.OleDbType = colType == OleDbType.Numeric ? OleDbType.Decimal : colType;
                            para.IsNullable = true;
                            para.Size = -1;

                            if (!(col["NUMERIC_PRECISION"] is DBNull))
                                para.Precision = Convert.ToByte(col["NUMERIC_PRECISION"]);

                            if (!(col["NUMERIC_SCALE"] is DBNull))
                                para.Scale = Convert.ToByte(col["NUMERIC_SCALE"]);

                            cmd.Parameters.Add(para);

                            p++;
                        }

                        var i = 0;

                        foreach (var row in rows)
                        {
                            p = 0;

                            foreach (var val in row)
                            {
                                cmd.Parameters[p].Value = val;
                                p++;
                            }

                            if (i == 0)
                                cmd.Prepare();

                            cmd.ExecuteNonQuery();

                            i++;
                        }
                    }

                    logger.LogDebug($"Finished writing to the database in {filePath}.");
                }

                return File.ReadAllBytes(filePath);
            }
            finally
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        ///// <inheritdoc/>
        //public byte[] WriteToAccess(IEnumerable<Array> rows, IEnumerable<string> columns = null)
        //{
        //    var assembly = Assembly.GetExecutingAssembly();
        //    string resourceName = $"{assembly.GetName().Name}.Templates.blank.accdb";

        //    string filePath = null;

        //    try
        //    {
        //        filePath = Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".accdb";

        //        // create a blank database copy
        //        using (var stream = assembly.GetManifestResourceStream(resourceName))
        //        using (var fs = File.Create(filePath))
        //            stream.CopyTo(fs);

        //        using (var stream = File.OpenRead(filePath))
        //        {
        //            UsingAccessDb(stream, connection =>
        //            {
        //                var cmd = connection.CreateCommand();
        //                cmd.CommandText = "create table DataExport";
        //            });
        //        }

        //        //var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath}";

        //        //using (var connection = new OleDbConnection(connectionString))
        //        //{
        //        //    connection.Open();
        //        //    action(connection);
        //        //}
        //    }
        //    finally
        //    {
        //        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        //            File.Delete(filePath);
        //    }

        //    UsingAccessDb(stream, connection =>
        //    {
        //        var cols = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new string[] { null, null, table });

        //        foreach (DataRow col in cols.Rows)
        //            colNames.Add(col["COLUMN_NAME"].ToString());
        //    });
        //}

        /// <inheritdoc/>
        public byte[] WriteToCsv(IEnumerable<Array> rows, IEnumerable<string> header = null, CsvDelimiter delimiter = CsvDelimiter.Semicolon)
        {
            var delStr = delimiter switch
            {
                CsvDelimiter.Comma => ",",
                CsvDelimiter.Semicolon => ";",
                CsvDelimiter.Space => " ",
                CsvDelimiter.Tab => "\t",
                CsvDelimiter.Pipe => "|",
                _ => ";"
            };

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delStr
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, csvConfig))
                {
                    if (header != null)
                    {
                        foreach (var col in header)
                            csv.WriteField(col);

                        csv.NextRecord();
                    }

                    foreach (var row in rows)
                    {
                        foreach (var value in row)
                            csv.WriteField(value);

                        csv.NextRecord();
                    }
                }

                return stream.ToArray();
            }
        }

        /// <inheritdoc/>
        public byte[] WriteToXlsx(IEnumerable<IEnumerable<ExcelCell>> rows)
        {
            var typeMap = new Dictionary<ExcelCell.DataType, CellValues>();

            using (var stream = new MemoryStream())
            {
                using (var package = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = package.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var xlSheetData = new SheetData();

                    foreach (var row in rows)
                    {
                        var xlRow = new Row();

                        foreach (var cell in row)
                        {
                            Cell xlCell;

                            if (cell.Type == ExcelCell.DataType.String)
                            {
                                xlCell = new Cell(new InlineString(new Text(cell.Value?.ToString()))) { DataType = CellValues.InlineString };
                            }
                            else
                            {
                                xlCell = new Cell();

                                xlCell.DataType = cell.Type switch
                                {
                                    ExcelCell.DataType.Decimal => CellValues.Number,
                                    ExcelCell.DataType.Double => CellValues.Number,
                                    ExcelCell.DataType.Int => CellValues.Number,
                                    _ => CellValues.String
                                };

                                xlCell.CellValue = cell.Value == null
                                    ? new CellValue()
                                    : cell.Type switch
                                    {
                                        ExcelCell.DataType.Decimal => new CellValue((decimal)cell.Value),
                                        ExcelCell.DataType.Double => new CellValue((double)cell.Value),
                                        ExcelCell.DataType.Int => new CellValue((int)cell.Value),
                                        _ => new CellValue(cell.Value.ToString())
                                    };
                            }

                            xlRow.Append(xlCell);
                        }

                        xlSheetData.Append(xlRow);
                    }

                    worksheetPart.Worksheet = new Worksheet(xlSheetData);

                    var sheets = package.WorkbookPart.Workbook.AppendChild(new Sheets());
                    sheets.AppendChild(new Sheet
                    {
                        Id = package.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    });

                    workbookPart.Workbook.Save();
                    package.Close();
                }

                return stream.ToArray();
            }
        }

        /// <inheritdoc/>
        public byte[] WriteToXlsx(IEnumerable<Array> rows, IEnumerable<string> header = null)
        {
            using (var stream = new MemoryStream())
            {
                using (var package = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = package.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var xlSheetData = new SheetData();

                    if (header != null)
                    {
                        var xlRow = new Row();

                        foreach (var col in header)
                        {
                            var xlCell = new Cell(new InlineString(new Text(col))) { DataType = CellValues.InlineString };
                            xlRow.Append(xlCell);
                        }

                        xlSheetData.Append(xlRow);
                    }

                    foreach (var row in rows)
                    {
                        var xlRow = new Row();

                        foreach (var value in row)
                        {
                            var xlCell = new Cell(new InlineString(new Text(value?.ToString()))) { DataType = CellValues.InlineString };
                            xlRow.Append(xlCell);
                        }

                        xlSheetData.Append(xlRow);
                    }

                    worksheetPart.Worksheet = new Worksheet(xlSheetData);

                    var sheets = package.WorkbookPart.Workbook.AppendChild(new Sheets());
                    sheets.AppendChild(new Sheet
                    {
                        Id = package.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    });

                    workbookPart.Workbook.Save();
                    package.Close();
                }

                return stream.ToArray();
            }
        }

        private void ValidateId(Guid id)
        {
            if (id == Guid.Empty)
                throw new Application.Exceptions.ValidationException("file.emptyId");
        }

        private Dimensions ResizeDimensions(int width, int height, int maxWidth, int maxHeight)
        {
            var ratio = (double)width / height;
            var isVertical = height > width;

            int newHeight = height;
            int newWidth = width;

            if (isVertical)
            {
                if (newHeight > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = (int)(ratio * newHeight);
                }
            }
            else
            {
                if (newWidth > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = (int)(1 / ratio * newWidth);
                }
            }

            return new Dimensions
            {
                Height = newHeight,
                Width = newWidth
            };
        }

        private string GetExcelColumnName(int columnNumber)
        {
            var max = 'Z' - 'A' + 1;
            var letter = string.Empty;

            while (columnNumber > 0)
            {
                var mod = (columnNumber - 1) % max;

                letter = Convert.ToChar('A' + mod) + letter;

                columnNumber = (columnNumber - mod) / max;
            }

            return letter.ToString();
        }

        private static string GetKeyObject(Guid id, string extension)
        {
            return $"{id}{extension}";
        }

        private void UsingAccessDb(Stream stream, Action<OleDbConnection> action)
        {
            string filePath = null;

            try
            {
                filePath = Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".tmpdb";

                logger.LogDebug($"Create a temporary Access database at {filePath}.");

                using (var fs = File.Create(filePath))
                    stream.CopyTo(fs);

                var connectionString = BuildAccessDbConnectionString(filePath);

                using (var connection = new OleDbConnection(connectionString))
                {
                    logger.LogDebug($"Opened OLEDB connection on {filePath}.");

                    connection.Open();
                    action(connection);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        private string BuildAccessDbConnectionString(string filePath, string mode = "Read")
        {
            return $"Provider={options.AccessDbProvider};Mode={mode};Data Source={filePath};";
        }

        private struct Dimensions
        {
            public int Height;
            public int Width;
        }
    }
}
