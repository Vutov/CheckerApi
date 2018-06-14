using System.IO;
using System.IO.Compression;
using System.Text;
using CheckerApi.Services.Interfaces;

namespace CheckerApi.Services
{
    public class CompressService : ICompressService
    {
        public byte[] Zip(string data, string innerZipFile)
        {
            byte[] compressedBytes;
            using (var outStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                {
                    var fileInArchive = archive.CreateEntry(innerZipFile, CompressionLevel.Optimal);
                    using (var entryStream = fileInArchive.Open())
                    using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(data ?? string.Empty)))
                    {
                        fileToCompressStream.CopyTo(entryStream);
                    }
                }

                compressedBytes = outStream.ToArray();
            }

            return compressedBytes;
        }
    }
}
