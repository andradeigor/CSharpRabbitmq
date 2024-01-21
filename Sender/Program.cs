RabbitMQConfig Sender = new();
Book message = new("Trono de vidro (Vol. 1)", "A magia há muito abandonou Adarlan.");
Sender.SendObject(message);