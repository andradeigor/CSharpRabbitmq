using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMQConfig
{
    private string exchangeName = "FilmExchange";
    private string routingKey = "film";
    private string queueName = "FilmQueue";

    private IModel _channel;

    private IConnection _conn;

    public RabbitMQConfig()
    {
        ConnectionFactory factory = new ConnectionFactory();
        //primeiro faz a conexão com o rabbit
        factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");
        factory.ClientProvidedName = "Rabbit Sender";

        this._conn = factory.CreateConnection();
        //depois começa a criar o model do canal e reunir as informações da fila exata que vai se conectar
        this._channel = _conn.CreateModel();
    }

    public void SendObject(Book message)
    {
        //declara o exchange que vamos usar e o tipo dele, usando direct as mensagens serão enviadas para filas com base na routingKey 
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        //declara a queue, tentei procurar o que essas declarações de durable false, etc fazem mas não achei no próprio guia 
        // do rabbitMQ https://www.rabbitmq.com/dotnet-api-guide.html#exchanges-and-queues
        _channel.QueueDeclare(queueName, false, false, false, null);
        //Aqui, faz a ligação da queue com o exchange, por meio da chave.
        _channel.QueueBind(queueName, exchangeName, routingKey, null);

        string messageText = JsonSerializer.Serialize(message);

        //transforma a string pra uma array de bytes pq é esse o padrão de envio.
        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(messageText);
        //Envia a mensagem pro Exchange declarado acima com a chave e a mensagem em bytes.
        _channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);

        Console.WriteLine("mensagem enviada!");
    }

}