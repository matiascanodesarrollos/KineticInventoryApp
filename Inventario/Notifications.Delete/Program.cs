using Infrastructure.ConfigurationProvider;
using Infrastructure.JobManager;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Delete;

IServiceProvider serviceProvider = new ServiceCollection()
    .AddInfrastructureServices()
    .AddTransient<INotificationsJobManager, DeleteJobManager>()
    .BuildServiceProvider();

var job = serviceProvider.GetRequiredService<INotificationsJobManager>();
await job.RunJob();