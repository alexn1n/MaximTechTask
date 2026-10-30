namespace MaximTech.Api;

public sealed class RandomNumberClient(HttpClient httpClient, ILogger<RandomNumberClient> logger)
{
    public async Task<int> NextAsync(int min, int max, CancellationToken cancellationToken)
    {
        try
        {
            var numbers = await httpClient.GetFromJsonAsync<int[]>(
                $"api/v1.0/random?min={min}&max={max + 1}&count=1", cancellationToken);

            if (numbers is [var number] && number >= min && number <= max)
                return number;

            logger.LogWarning("Удалённое API вернуло неожиданный ответ: {Numbers}", numbers);
        }
        catch (Exception e) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(e, "Удалённое API случайных чисел недоступно");
        }

        return Random.Shared.Next(min, max + 1);
    }
}
