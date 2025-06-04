using Polly;

namespace Infrastructure.Polly
{
    public class PollyResiliencePipeline : IPollyResiliencePipeline
    {
        private readonly ResiliencePipeline _pipeline;

        public PollyResiliencePipeline(ResiliencePipelineBuilder pipelineBuilder)
        {
            _pipeline = pipelineBuilder.Build();
        }

        public async Task ExecuteAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default)
        {
            await _pipeline.ExecuteAsync(callback, cancellationToken);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default)
        {
            return await _pipeline.ExecuteAsync(callback, cancellationToken);
        }
    }
}
