using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beltzac.AIPlay.App.Api.Helpers
{
    public interface IRabbitMQHelper
    {
        ConnectionFactory GetConnectionFactory();

        string RetrieveSingleMessage(string queueName, IConnection connection);

        uint RetrieveMessageCount(string queueName, IConnection connection);

        IConnection CreateConnection(ConnectionFactory connectionFactory);

        QueueDeclareOk CreateQueue(string queueName, IConnection connection);

        bool WriteMessageOnQueue(string message, string queueName, IConnection connection);
    }
}
