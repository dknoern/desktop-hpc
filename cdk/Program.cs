using Amazon.CDK;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;

namespace HPC
{
    public class Program : Stack
    {
    static void Main(string[] args)
    {
            var app = new App();
            new Program(app, "HPCStack");
            app.Synth();
    }
        internal Program(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var inputQueue = new Queue(this, "InputQueue", new QueueProps
            {
                QueueName = "InputQueue"
            });
            var outputQueue = new Queue(this, "OutputQueue", new QueueProps
            {
                QueueName = "OutputQueue"
            });
            var cluster = new Cluster(this, "HPCCluster", new ClusterProps
            {
                ClusterName= "HPCCluster"
            });
            var service = new QueueProcessingFargateService(this, "HPCService",
                new QueueProcessingFargateServiceProps
                {
                    ServiceName = "HPCService",
                    Cluster = cluster,
                    MinScalingCapacity = 1,
                    MaxScalingCapacity = 5,
                    Image = ContainerImage.FromAsset("./worker"),
                    MemoryLimitMiB = 2048,
                    Queue = inputQueue
                }
            );
            outputQueue.GrantSendMessages(service.TaskDefinition.TaskRole);
        }
    }
}
