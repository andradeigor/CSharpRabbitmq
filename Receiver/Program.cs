using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory factory = new ConnectionFactory();
//primeiro faz a conexão com o rabbit
factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");
factory.ClientProvidedName = "Rabbit Receiver";

IConnection conn = factory.CreateConnection();
//depois começa a criar o model do canal e reunir as informações da fila exata que vai se conectar
IModel channel = conn.CreateModel();

string exchangeName = "FilmExchange";
string routingKey = "film";
string queueName = "FilmQueue";

//declara o exchange que vamos usar e o tipo dele, usando direct as mensagens serão enviadas para filas com base na routingKey 
channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
//declara a queue, tentei procurar o que essas declarações de durable false, etc fazem mas não achei no próprio guia 
// do rabbitMQ https://www.rabbitmq.com/dotnet-api-guide.html#exchanges-and-queues
channel.QueueDeclare(queueName,false,false,false,null);
//Aqui, faz a ligação da queue com o exchange, por meio da chave.
channel.QueueBind(queueName,exchangeName,routingKey,null);
//O primeiro valor diz para pegar mensagens de qualquer tamanho, o segundo limita o número de mensagens recebidas para 1 por vez
// A ultima mensagem faz com que essa configuração não seja usada globalmente na aplicação
channel.BasicQos(0,1,false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args)=>{
    //pega os bytes da mensagem do body
    byte[] body = args.Body.ToArray();
    //le esses bytes e faz o cast para string
    string message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Mensagem recebida: {message}");
    //Avisa para o RabbitMQ que essa mensagem foi propriamente tratada
    channel.BasicAck(args.DeliveryTag, false);
};

//após isso, fechar a conexão do consumer/receiver
string consumerTag = channel.BasicConsume(queueName,false, consumer);

Console.ReadLine();

channel.BasicCancel(consumerTag);
channel.Close();
conn.Close();
