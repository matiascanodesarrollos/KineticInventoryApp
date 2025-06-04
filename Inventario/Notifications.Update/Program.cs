using Infrastructure.ConfigurationProvider;
using Infrastructure.JobManager;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Update;

IServiceProvider serviceProvider = new ServiceCollection()
    .AddInfrastructureServices()
    .AddTransient<INotificationsJobManager, UpdateJobManager>()
    .BuildServiceProvider();

var job = serviceProvider.GetRequiredService<INotificationsJobManager>();
await job.RunJob();