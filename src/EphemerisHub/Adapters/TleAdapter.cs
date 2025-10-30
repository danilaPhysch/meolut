using System.Text;
using EphemerisHub.Application;
using EphemerisHub.Infrastructure.Settings;
using EphemerisHub.Models;

namespace EphemerisHub.Adapters;

public class TleAdapter(HttpClient httpClient, TleSettings tleSettings, MeosarSatellitesSettings meosarSatellitesSettings, ILogger<TleAdapter> logger) : ITleAdapter
{
    public async Task<IReadOnlyList<ParsedTle>> GetTles()
    {
        var allTles = new List<ParsedTle>();

        foreach (var url in tleSettings.Uris)
        {
            try
            {
                logger.LogInformation("Downloading tle from {Url}", url);

                // Получаем поток байт
                await using var stream = await httpClient.GetStreamAsync(url);
                using var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                memory.Position = 0;

                // Читаем текст из памяти построчно
                using var reader = new StreamReader(memory, Encoding.UTF8, leaveOpen: false);

                static IEnumerable<string> StreamLines(StreamReader r)
                {
                    string? line;
                    while ((line = r.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!string.IsNullOrWhiteSpace(line))
                            yield return line;
                    }
                }

                var parsed = TleParser.ParseLines(StreamLines(reader).ToList()).ToList();
                logger.LogInformation("Fetched {ParsedCount} TLEs from {Url}", parsed.Count, url);
                allTles.AddRange(parsed);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error fetching {Url}: {ExMessage}", url, ex.Message);
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "IO error fetching {Url}: {ExMessage}", url, ex.Message);
            }
            catch (FormatException ex)
            {
                logger.LogError(ex, "Format error parsing TLE from {Url}: {ExMessage}", url, ex.Message);
            }
        }

        var meosarTles = allTles
            .Where(tle => meosarSatellitesSettings.AllNoradIds.TryGetValue(tle.NoradId, out _))
            .Select(tle =>
            {
                meosarSatellitesSettings.AllNoradIds.TryGetValue(tle.NoradId, out var csSatNum);
                tle.CsSatNum = csSatNum;
                return tle;
            })
            .ToList();

        return meosarTles;
    }
}