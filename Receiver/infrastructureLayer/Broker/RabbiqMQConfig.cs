using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQConfig
{
    private string exchangeName = "FilmExchange";
    private string routingKey = "film";
    private string queueName = "FilmQueue";

    private IModel _channel;

    private IConnection _conn;

    private EventingBasicConsumer _consumer;

    public RabbitMQConfig()
    {
        ConnectionFactory factory = new ConnectionFactory();
        //primeiro faz a conexão com o rabbit
        factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");
        factory.ClientProvidedName = "Rabbit Sender";

        _conn = factory.CreateConnection();
        //depois começa a criar o model do canal e reunir as informações da fila exata que vai se conectar
        _channel = _conn.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);
    }

    public void Recieve()
    {
        //declara o exchange que vamos usar e o tipo dele, usando direct as mensagens serão enviadas para filas com base na routingKey 
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        //declara a queue, tentei procurar o que essas declarações de durable false, etc fazem mas não achei no próprio guia 
        // do rabbitMQ https://www.rabbitmq.com/dotnet-api-guide.html#exchanges-and-queues
        _channel.QueueDeclare(queueName, false, false, false, null);
        //Aqui, faz a ligação da queue com o exchange, por meio da chave.
        _channel.QueueBind(queueName, exchangeName, routingKey, null);
        //O primeiro valor diz para pegar mensagens de qualquer tamanho, o segundo limita o número de mensagens recebidas para 1 por vez
        // A ultima mensagem faz com que essa configuração não seja usada globalmente na aplicação
        _channel.BasicQos(0, 1, false);

        _consumer.Received += (sender, args) =>
    {
        //pega os bytes da mensagem do body
        byte[] body = args.Body.ToArray();
        //le esses bytes e faz o cast para string
        string message = Encoding.UTF8.GetString(body);
        var objetoDeserializado = JsonConvert.DeserializeObject<BookView>(message);
        string messageSerialized = JsonConvert.SerializeObject(objetoDeserializado);
        Console.WriteLine($"Mensagem recebida: {messageSerialized}");
        //Avisa para o RabbitMQ que essa mensagem foi propriamente tratada
        _channel.BasicAck(args.DeliveryTag, false);
    };

        //após isso, fechar a conexão do consumer/receiver
        string consumerTag = _channel.BasicConsume(queueName, false, _consumer);

        Console.ReadLine();

        _channel.BasicCancel(consumerTag);
        _channel.Close();
        _conn.Close();
    }

}