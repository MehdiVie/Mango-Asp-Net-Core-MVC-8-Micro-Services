using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;


        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }


        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine("<br/>Total : " + cartDto.CartHeader.CartTotal);
            message.AppendLine("<br/>");
            message.AppendLine("<ul>");

            foreach (var item in cartDto.CartDetails)
            {
                message.AppendLine("<li>");
                message.AppendLine("Name : " + item.Product.Name + " x " + item.Count);
                message.AppendLine("</li>");
            }
            message.AppendLine("</ul>");

            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);

        }

        public async Task<bool> LogAndEmail (string message , string email)
        {
            try
            {
                EmailLogger emailLogger = new()
                {
                    EmailSent=DateTime.Now,
                    Email = email,
                    Message = message
                };

                await using var _db = new AppDbContext(_dbOptions);
                await _db.EmailLoggers.AddAsync(emailLogger);
                _db.SaveChanges();

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task LogOrderPlaced(RewardMessage rewardMessage)
        {
            string message = "New Order was placed successfully! <br/> " + rewardMessage.OrderId;

            await LogAndEmail(message, "info@mango.com");
        }

        public async Task RegisterUserEmailAndLog(string email)
        {
            string message = "User Registeration was successful! <br/> "+ email;

            await LogAndEmail(message, "info@mango.com");
        }
    }
}
