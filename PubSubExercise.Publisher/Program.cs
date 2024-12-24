// See https://aka.ms/new-console-template for more information
using PubSubExercise.Publisher;

const string projectIdEnvVar = "PUBSUB_EXERCISE_PROJECTID";
const string topicIdEnvVar = "PUBSUB_EXERCISE_TOPICID";

Console.WriteLine("Starting PubSub Exercise > Publish...");

var publisher = new PublishMessagesAsyncSample();

var projectId = Environment.GetEnvironmentVariable(projectIdEnvVar)
	?? throw new InvalidOperationException($"{projectIdEnvVar} environment variable is not set.");

var topicId = Environment.GetEnvironmentVariable(topicIdEnvVar)
	?? throw new InvalidOperationException($"{topicIdEnvVar} environment variable is not set.");

var messageTexts = new List<string> { "Diane is cute!" };

var publishedMessageCount = await publisher.PublishMessagesAsync(projectId, topicId, messageTexts);

Console.WriteLine($"Published {publishedMessageCount} messages to {topicId}.");
