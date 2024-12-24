// See https://aka.ms/new-console-template for more information

using PubSubExercise.PubSubSubscriber;

const string projectIdEnvVar = "PUBSUB_EXERCISE_PROJECTID";
const string subscriptionIdEnvVar = "PUBSUB_EXERCISE_SUBSCRIPTIONID";

Console.WriteLine("Starting PubSub Exercise > Subscribe...");

var subscriber = new PullMessagesAsyncSample();

var projectId = Environment.GetEnvironmentVariable(projectIdEnvVar)
	?? throw new InvalidOperationException($"{projectIdEnvVar} environment variable is not set.");

var subscriptionId = Environment.GetEnvironmentVariable(subscriptionIdEnvVar)
	?? throw new InvalidOperationException($"{subscriptionIdEnvVar} environment variable is not set.");

var messageCount = await subscriber.PullMessagesAsync(projectId, subscriptionId, acknowledge: false);

Console.WriteLine($"Pulled {messageCount} messages from {subscriptionId}.");