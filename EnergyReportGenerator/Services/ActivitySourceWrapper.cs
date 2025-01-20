namespace EnergyReportGenerator.Services
{
    public interface IActivitySource
    {
        Activity? StartActivity(string name);
    }

    public class ActivitySourceWrapper : IActivitySource
    {
        private readonly ActivitySource _activitySource;

        public ActivitySourceWrapper(string sourceName)
        {
            _activitySource = new ActivitySource(sourceName);
        }

        public Activity? StartActivity(string name)
        {
            return _activitySource.StartActivity(name);
        }
    }
}