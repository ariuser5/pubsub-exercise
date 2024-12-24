using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using PubSubExercise.Authentication;

const string applicationName = "PubSubGmailPrimer";

const string projectIdEnvVar = "PUBSUB_GMAILPRIMER_PROJECTID";
const string topicIdEnvVar = "PUBSUB_GMAILPRIMER_TOPICID";

string projectId = Environment.GetEnvironmentVariable(projectIdEnvVar)
	?? throw new InvalidOperationException($"{projectIdEnvVar} environment variable is not set.");
	
string topicId = Environment.GetEnvironmentVariable(topicIdEnvVar)
	?? throw new InvalidOperationException($"{topicIdEnvVar} environment variable is not set.");

Console.WriteLine("Starting Gmail API Watch...");

// Load the service account credentials
UserCredential credential = await CredentialsProvider.GetUserCredentialsAsync();

// Create the Gmail service
var service = new GmailService(new BaseClientService.Initializer()
{
	HttpClientInitializer = credential,
	ApplicationName = applicationName,
});

// Schedule the task to run once a day
var timer = new Timer(async _ => await WatchGmail(service), null, TimeSpan.Zero, TimeSpan.FromDays(1));

// await StopGmailWatch(service);

// Keep the application running
Console.WriteLine("Press [Enter] to exit...");
Console.ReadLine();

async Task WatchGmail(GmailService service)
{
	try
	{
		var watchRequest = new WatchRequest
		{
			LabelFilterAction = "include",
			LabelIds = ["INBOX"], 
			TopicName = $"projects/{projectId}/topics/{topicId}"
		};

		var watchResponse = await service.Users.Watch(watchRequest, "me").ExecuteAsync();
		Console.WriteLine($"Watch request successful: {watchResponse.HistoryId}");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"An error occurred: {ex.Message}");
	}
}

static async Task StopGmailWatch(GmailService service)
{
    try
    {
        await service.Users.Stop("me").ExecuteAsync();
        Console.WriteLine("Stopped Gmail watch successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while stopping Gmail watch: {ex.Message}");
    }
}