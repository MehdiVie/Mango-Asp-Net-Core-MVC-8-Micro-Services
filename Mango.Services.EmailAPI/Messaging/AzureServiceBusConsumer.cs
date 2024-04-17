using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _serviceBudConnectionString;
        private readonly string _emailCartQueue;
        private readonly string _emailRegedUserQueue;

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        private readonly string _orderCreatedTopic;
        private readonly string _orderCreatedRewardSubscription;
        private ServiceBusProcessor _emailOrderPlacedProcessor;

        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _emailRegedUserProcessor;

        

        public AzureServiceBusConsumer(IConfiguration configuration,EmailService emailService)
        {
            _emailService=emailService;

            _configuration=configuration;
            
            _serviceBudConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            _emailRegedUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
            _orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            _orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Email_Subscription");

            var client = new ServiceBusClient(_serviceBudConnectionString);

            _emailCartProcessor=client.CreateProcessor(_emailCartQueue);
            _emailRegedUserProcessor = client.CreateProcessor(_emailRegedUserQueue);
            _emailOrderPlacedProcessor= client.CreateProcessor(_orderCreatedTopic, _orderCreatedRewardSubscription);
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();

            _emailRegedUserProcessor.ProcessMessageAsync += OnEmailRegedUserRequestReceived;
            _emailRegedUserProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailRegedUserProcessor.StartProcessingAsync();

            _emailOrderPlacedProcessor.ProcessMessageAsync += OnOrderPlacedRequestReceived;
            _emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailOrderPlacedProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            await _emailRegedUserProcessor.StopProcessingAsync();
            await _emailRegedUserProcessor.DisposeAsync();

            await _emailOrderPlacedProcessor.StopProcessingAsync();
            await _emailOrderPlacedProcessor.DisposeAsync();
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            //this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body);

            try
            {
                //TODO - try to log email
                await _emailService.EmailCartAndLog(objMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task OnEmailRegedUserRequestReceived(ProcessMessageEventArgs args)
        {
            //this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            string objMessage = JsonConvert.DeserializeObject<string>(body);

            try
            {
                //TODO - try to log email
                await _emailService.RegisterUserEmailAndLog(objMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task OnOrderPlacedRequestReceived(ProcessMessageEventArgs args)
        {
            //this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RewardMessage objMessage = JsonConvert.DeserializeObject<RewardMessage>(body);

            try
            {
                //TODO - try to log email
                await _emailService.LogOrderPlaced(objMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }


    }
}
