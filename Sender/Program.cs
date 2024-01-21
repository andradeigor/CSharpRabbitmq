// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text.Json.Nodes;
using RabbitMQ.Client;


ConnectionFactory factory = new ConnectionFactory();
//primeiro faz a conexão com o rabbit
factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");
factory.ClientProvidedName = "Rabbit Sender";

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

Book testBook = new Book("Percy Jackson", "um livro sobre deuses");

string teste = JsonSerializer.Serialize(testBook);

//transforma a string pra uma array de bytes pq é esse o padrão de envio.
byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(teste);
//Envia a mensagem pro Exchange declarado acima com a chave e a mensagem em bytes.
channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);

channel.Close();
conn.Close();

Console.WriteLine("mensagem enviada!");