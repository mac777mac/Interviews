namespace Boovatech.Eventing.Data;

internal static class RepositoryCache<T> where T : class
{
    private static readonly SemaphoreSlim loadLock = new(1, 1);
    private static volatile T? value;

    public static async Task<T> GetOrLoadAsync(Func<Task<T>> load)
    {
        var current = value;
        if (current is not null)
            return current;

        await loadLock.WaitAsync();
        try
        {
            return value ??= await load();
        }
        finally
        {
            loadLock.Release();
        }
    }
}
