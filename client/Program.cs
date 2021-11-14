using System;
using Amazon.SQS;
using System.Threading.Tasks;
using Amazon.SQS.Model;

class Program
{
    static async Task Main(string[] args)
    {
        var sqsClient = new AmazonSQSClient();

        var outputQueue = await sqsClient.GetQueueUrlAsync("OutputQueue");
        var inputQueue = await sqsClient.GetQueueUrlAsync("InputQueue");
        long iStart = 1000000000000000001; // one quintillion one

        Console.WriteLine("Input number of large integers to factor, starting with one trillion one:");

        int count = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Sending {0} numbers sent to cluster for factoring.", count);
        Console.WriteLine("From {0} to {1}", iStart, iStart + count);

        for (long i = iStart; i < iStart + count; i++)
        {
            await sqsClient.SendMessageAsync(inputQueue.QueueUrl, i.ToString());
        }

        Console.WriteLine("Results:");

        int finishedCount = 0;

        while (finishedCount < count)
        {
            var msg = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = outputQueue.QueueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 3
            });

            if (msg.Messages.Count != 0)
            {
                Console.WriteLine(msg.Messages[0].Body);
                await sqsClient.DeleteMessageAsync(outputQueue.QueueUrl, msg.Messages[0].ReceiptHandle);
                finishedCount++;
            }

        }

        Console.WriteLine("Factoring complete, remember to terminate cluster when done.");
    }
}
