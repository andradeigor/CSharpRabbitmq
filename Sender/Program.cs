// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text.Json.Nodes;
using RabbitMQ.Client;


RabbitMQConfig Sender = new();
Book message = new("Trono de vidro (Vol. 1)", "A magia há muito abandonou Adarlan.");
Sender.SendObject(message);