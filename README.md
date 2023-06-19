# lab-azure-service-bus
Projeto simples de envio e consumo de mensagens, usando o Azure Service Bus.

Além do uso do Service Bus, implementei também uma lógica para quebrar a mensagem em pacotes.
Dessa forma, se torna possível fazer o envio de mensagens e o consumo de mensagens, 
que excedem os limites previstos a depender do pacote que você escolheu.