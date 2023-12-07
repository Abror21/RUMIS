using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class FileServiceFake : IFileService
    {
        public FileEntry Entity { get; set; } = new FileEntry { };
        public byte[] Content { get; set; } = new byte[] { };

        public IEnumerable<object> ReadFromCsvTResult { get; set; }
        public IEnumerable<IDictionary<string, object>> ReadFromXlsxResult { get; set; }
        public IEnumerable<object> ReadFromXlsxTResult { get; set; }
        public IEnumerable<IDictionary<string, object>> ReadFromAccessResult { get; set; }

        public Task AddOrUpdateAsync(Guid id, FileDto file)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public byte[] WriteToCsv(IEnumerable<Array> rows, IEnumerable<string> columns = null, CsvDelimiter delimiter = CsvDelimiter.Semicolon)
        {
            throw new NotImplementedException();
        }

        public Task<FileEntry> GetAsync(Guid id)
        {
            return Task.FromResult(Entity);
        }

        public byte[] HtmlToPdf(string html)
        {
            return Content;
        }

        public byte[] ResizeImage(byte[] content, int width, int height, int quality, bool fit = false)
        {
            return Content;
        }

        public IEnumerable<T> ReadFromCsv<T>(Stream stream) where T : class
        {
            return ReadFromCsvTResult as IEnumerable<T>;
        }

        public IEnumerable<T> ReadFromXlsx<T>(Stream stream, int startRowNumber) where T : class
        {
            return ReadFromXlsxTResult as IEnumerable<T>;
        }

        public IEnumerable<IDictionary<string, object>> ReadFromXlsx(Stream stream, int startRowNumber)
        {
            return ReadFromXlsxResult;
        }

        public IEnumerable<IDictionary<string, object>> ReadFromAccess(Stream stream, string table)
        {
            return ReadFromAccessResult;
        }

        public IEnumerable<string> ReadAccessColumnNames(Stream stream, string table)
        {
            return new string[] { };
        }

        public byte[] WriteToAccess(IEnumerable<Array> rows, IEnumerable<AccessColumn> columns, string table)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteToXlsx(IEnumerable<IEnumerable<ExcelCell>> rows)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteToXlsx(IEnumerable<Array> rows, IEnumerable<string> header = null)
        {
            throw new NotImplementedException();
        }
    }
}
