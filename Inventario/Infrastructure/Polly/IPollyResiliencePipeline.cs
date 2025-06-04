namespace Infrastructure.Polly
{
    public interface IPollyResiliencePipeline
    {
        Task ExecuteAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default);
        Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default);
    }
}