using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MoviesDatabaseChat;

class CsvSplitter
{
    public static async Task SplitBySizeAsync(string sourceCsv, long maxBytesPerFile = 90L * 1024 * 1024)
    {
        if (!File.Exists(sourceCsv))
            throw new FileNotFoundException("Source file not found", sourceCsv);

        var encoding = new UTF8Encoding(false);
        string? header;
        string newLine = Environment.NewLine;

        using var reader = new StreamReader(sourceCsv, encoding);
        header = await reader.ReadLineAsync();
        if (header == null)
            return; // empty file

        var headerLine = header + newLine;
        int headerBytes = encoding.GetByteCount(headerLine);

        int fileIndex = 1;
        StreamWriter? writer = null;
        long currentBytes = 0;

        async Task StartNewFile()
        {
            if (writer != null)
            {
                await writer.FlushAsync();
                writer.Dispose();
            }

            string outFile = Path.Combine(
                Path.GetDirectoryName(sourceCsv)!,
                $"{Path.GetFileNameWithoutExtension(sourceCsv)}{fileIndex}.csv");

            writer = new StreamWriter(outFile, false, encoding);
            await writer.WriteAsync(headerLine);
            currentBytes = headerBytes;
            fileIndex++;
        }

        await StartNewFile();

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var lineWithNewline = line + newLine;
            int lineBytes = encoding.GetByteCount(lineWithNewline);

            if (currentBytes + lineBytes > maxBytesPerFile && currentBytes > headerBytes)
            {
                await StartNewFile();
            }

            await writer!.WriteAsync(lineWithNewline);
            currentBytes += lineBytes;
        }

        if (writer != null)
        {
            await writer.FlushAsync();
            writer.Dispose();
        }
    }
}