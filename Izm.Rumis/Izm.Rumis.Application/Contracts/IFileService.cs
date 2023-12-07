using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IFileService
    {
        /// <summary>
        /// Add or update a file.
        /// </summary>
        /// <param name="id">File ID</param>
        /// <param name="file">File data</param>
        /// <returns></returns>
        Task AddOrUpdateAsync(Guid id, FileDto file);
        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="id">File ID</param>
        /// <returns></returns>
        Task DeleteAsync(Guid id);
        /// <summary>
        /// Delete a files range.
        /// </summary>
        /// <param name="ids">File IDs</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get a file.
        /// </summary>
        /// <param name="id">File ID</param>
        /// <returns></returns>
        Task<FileEntry> GetAsync(Guid id);
        /// <summary>
        /// Resize an image.
        /// </summary>
        /// <param name="content">Image file content</param>
        /// <param name="width">Desired width (px)</param>
        /// <param name="height">Desired height (px)</param>
        /// <param name="quality">Image quality (from 0 to 100)</param>
        /// <param name="fit">Fit an image to the provided frame by height and width</param>
        /// <returns>Resized image content</returns>
        byte[] ResizeImage(byte[] content, int width, int height, int quality, bool fit = false);
        /// <summary>
        /// Convert HTML to PDF.
        /// </summary>
        /// <param name="html">HTML content</param>
        /// <returns>PDF file content</returns>
        byte[] HtmlToPdf(string html);
        /// <summary>
        /// Get data rows from CSV file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        IEnumerable<T> ReadFromCsv<T>(Stream stream) where T : class;
        /// <summary>
        /// Get data rows from XLSX file.
        /// A data header row must be present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="startRowNumber">Header row number</param>
        /// <returns></returns>
        IEnumerable<T> ReadFromXlsx<T>(Stream stream, int startRowNumber) where T : class;
        /// <summary>
        /// Get data rows from XLSX file.
        /// A data header row must be present.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="startRowNumber">Header row number</param>
        /// <returns>List of dictionaries, where key is Excel column name</returns>
        IEnumerable<IDictionary<string, object>> ReadFromXlsx(Stream stream, int startRowNumber);
        /// <summary>
        /// Get data rows from Access database file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        IEnumerable<IDictionary<string, object>> ReadFromAccess(Stream stream, string table);
        /// <summary>
        /// Create an Access database file.
        /// </summary>
        /// <param name="rows">Data rows</param>
        /// <param name="columns">Column definitions</param>
        /// <param name="table">Table name</param>
        /// <returns></returns>
        byte[] WriteToAccess(IEnumerable<Array> rows, IEnumerable<AccessColumn> columns, string table);
        /// <summary>
        /// Get table column names from Access database file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        IEnumerable<string> ReadAccessColumnNames(Stream stream, string table);
        /// <summary>
        /// Create a CSV file.
        /// </summary>
        /// <param name="rows">Data rows</param>
        /// <param name="header">Header labels</param>
        /// <param name="delimiter">Column delimiter</param>
        /// <returns>CSV file content</returns>
        byte[] WriteToCsv(IEnumerable<Array> rows, IEnumerable<string> header = null, CsvDelimiter delimiter = CsvDelimiter.Semicolon);
        /// <summary>
        /// Create a XLSX file.
        /// </summary>
        /// <param name="rows">Cell rows</param>
        /// <returns>XLSX file content</returns>
        byte[] WriteToXlsx(IEnumerable<IEnumerable<ExcelCell>> rows);
        /// <summary>
        /// Create a XLSX file.
        /// </summary>
        /// <param name="rows">Data rows</param>
        /// <param name="header">Header labels</param>
        /// <returns>XLSX file content</returns>
        byte[] WriteToXlsx(IEnumerable<Array> rows, IEnumerable<string> header = null);
    }
}
