using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal class FileServiceFake : IFileService
    {
        private readonly Dictionary<Guid, FileEntry> files = new Dictionary<Guid, FileEntry>();
        public IEnumerable<object> ReadFromCsvTResult { get; set; }
        public FileDto File { get; set; }

        public Task AddOrUpdateAsync(Guid id, FileDto file)
        {
            var entry = new FileEntry
            {
                Content = file.Content,
                ContentType = file.ContentType,
                Name = file.FileName
            };

            if (files.ContainsKey(id))
                files[id] = entry;
            else
                files.Add(id, entry);

            File = file;

            return Task.FromResult(true);
        }

        public Task DeleteAsync(Guid id)
        {
            if (files.ContainsKey(id))
                files.Remove(id);

            return Task.FromResult(true);
        }

        public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            foreach (var id in ids)
                if (files.ContainsKey(id))
                    files.Remove(id);

            return Task.FromResult(true);
        }

        public Task<FileEntry> GetAsync(Guid id)
        {
            var file = files.ContainsKey(id) ? files[id] : null;
            return Task.FromResult(file);
        }

        public byte[] HtmlToPdf(string html)
        {
            var bytes = Encoding.UTF8.GetBytes("htmltopdf");
            return bytes;
        }

        public byte[] ResizeImage(byte[] content, int width, int height, int quality, bool fit = false)
        {
            var bytes = Encoding.UTF8.GetBytes("resized");
            return bytes;
        }

        public IEnumerable<T> ReadFromCsv<T>(Stream stream) where T : class
        {
            return ReadFromCsvTResult as IEnumerable<T>;
        }

        public IEnumerable<T> ReadFromXlsx<T>(Stream stream, int startRowNumber) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> ReadFromXlsx(Stream stream, int startRowNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> ReadFromAccess(Stream stream, string table)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ReadAccessColumnNames(Stream stream, string table)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteToAccess(IEnumerable<Array> rows, IEnumerable<AccessColumn> columns, string table)
        {
            return new byte[] { };
        }

        public byte[] WriteToCsv(IEnumerable<Array> rows, IEnumerable<string> header = null, CsvDelimiter delimiter = CsvDelimiter.Semicolon)
        {
            return new byte[] { };
        }

        public byte[] WriteToXlsx(IEnumerable<IEnumerable<ExcelCell>> rows)
        {
            return new byte[] { };
        }

        public byte[] WriteToXlsx(IEnumerable<Array> rows, IEnumerable<string> header = null)
        {
            return new byte[] { };
        }
    }
}
