using Infrastructure.ConfigurationProvider;
using Infrastructure.JobManager;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Insert;

IServiceProvider serviceProvider = new ServiceCollection()
    .AddInfrastructureServices()
    .AddTransient<INotificationsJobManager, InsertJobManager>()
    .BuildServiceProvider();

var job = serviceProvider.GetRequiredService<INotificationsJobManager>();
await job.RunJob();