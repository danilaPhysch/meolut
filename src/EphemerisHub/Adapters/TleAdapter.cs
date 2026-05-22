using System.Text;
using EphemerisHub.Application;
using EphemerisHub.Infrastructure.Settings;
using EphemerisHub.Models;

namespace EphemerisHub.Adapters;

public class TleAdapter(HttpClient httpClient, TleSettings tleSettings, MeosarSatellitesSettings meosarSatellitesSettings, ILogger<TleAdapter> logger) : ITleAdapter
{
    private const int MaxParallelDownloads = 4;

    public async Task<IReadOnlyList<ParsedTle>> GetTles(CancellationToken cancellationToken = default)
    {
        var urls = tleSettings.Uris.ToList();
        var semaphore = new SemaphoreSlim(Math.Max(1, Math.Min(MaxParallelDownloads, urls.Count)));
        var downloadTasks = urls.Select(async url =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await DownloadAndParseTles(url, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });
        var allTles = (await Task.WhenAll(downloadTasks)).SelectMany(x => x).ToList();

        var meosarTles = allTles
            .Select(tle =>
            {
                if (meosarSatellitesSettings.AllNoradIds.TryGetValue(tle.NoradId, out var csSatNum))
                {
                    tle.CsSatNum = csSatNum;
                    return tle;
                }
                return null;
            })
            .OfType<ParsedTle>()
            .ToList();

        return meosarTles;
    }

    private async Task<IReadOnlyList<ParsedTle>> DownloadAndParseTles(Uri url, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started downloading TLE from {Url}", url);
        try
        {
            await using var stream = await httpClient.GetStreamAsync(url, cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: false);

            var parsedTles = new List<ParsedTle>();
            var malformedTles = 0;
            var skippedLines = 0;
            string? currentName = null;
            string? currentLine1 = null;

            while (await ReadNonEmptyLine(reader, cancellationToken) is { } line)
            {
                if (line.StartsWith("1 ", StringComparison.Ordinal))
                {
                    if (currentLine1 is not null)
                    {
                        skippedLines++;
                        logger.LogWarning("Incomplete TLE in {Url}: missing line 2 for line 1 [{Line1}]", url, currentLine1);
                    }

                    currentLine1 = line;
                    continue;
                }

                if (line.StartsWith("2 ", StringComparison.Ordinal))
                {
                    if (currentLine1 is null)
                    {
                        skippedLines++;
                        logger.LogWarning("Orphan line 2 in {Url}: {Line2}", url, line);
                        continue;
                    }

                    try
                    {
                        var name = currentName ?? "Unknown";
                        var tle = TleParser.ParseLines([name, currentLine1, line]).FirstOrDefault();

                        if (tle is null)
                        {
                            malformedTles++;
                            logger.LogWarning("Malformed TLE in {Url}: {Name}", url, name);
                        }
                        else
                        {
                            parsedTles.Add(tle);
                        }
                    }
                    catch (Exception ex) when (
                        ex is not OutOfMemoryException &&
                        ex is not StackOverflowException &&
                        ex is not AccessViolationException)
                    {
                        malformedTles++;
                        logger.LogWarning(ex, "Failed to parse TLE in {Url}.", url);
                    }

                    currentName = null;
                    currentLine1 = null;
                    continue;
                }

                if (currentLine1 is not null)
                {
                    skippedLines++;
                    logger.LogWarning("Discarding incomplete TLE in {Url}: missing line 2 for line 1 [{Line1}]", url, currentLine1);
                    currentLine1 = null;
                }

                currentName = line;
            }

            if (currentLine1 is not null)
            {
                skippedLines++;
                logger.LogWarning("Discarding incomplete trailing TLE in {Url}: missing line 2 for line 1 [{Line1}]", url, currentLine1);
            }

            logger.LogInformation(
                "Finished processing {Url}. Parsed: {ParsedCount}, Malformed: {MalformedCount}, Skipped: {SkippedCount}",
                url,
                parsedTles.Count,
                malformedTles,
                skippedLines);

            return parsedTles;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error fetching {Url}: {ExMessage}", url, ex.Message);
            return [];
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "IO error fetching {Url}: {ExMessage}", url, ex.Message);
            return [];
        }
    }

    private static async Task<string?> ReadNonEmptyLine(StreamReader reader, CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                return trimmed;
            }
        }

        return null;
    }
}