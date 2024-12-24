using Google.Cloud.PubSub.V1;

namespace PubSubExercise.Publisher;
public class PublishMessagesAsyncSample
{
    public async Task<int> PublishMessagesAsync(
		string projectId,
		string topicId,
		IEnumerable<string> messageTexts)
    {
        TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);
        PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

        int publishedMessageCount = 0;
        var publishTasks = messageTexts.Select(async text =>
        {
            try
            {
                string message = await publisher.PublishAsync(text);
                Console.WriteLine($"Published message {message}");
                Interlocked.Increment(ref publishedMessageCount);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An error occurred when publishing message {text}: {exception.Message}");
            }
        });
        await Task.WhenAll(publishTasks);
        return publishedMessageCount;
    }
}