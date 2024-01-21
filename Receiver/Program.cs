using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

RabbitMQConfig Reciever = new();
Reciever.Recieve();
