using Mango.Services.RewardAPI.Message;
using Microsoft.EntityFrameworkCore;
using Mango.Services.RewardAPI.Data;
using System.Text;
using Mango.Services.RewardAPI.Models;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _dbOptions;


        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task UpdateReward(RewardMessage rewardMessage)
        {
            try
            {
                Reward reward = new()
                {
                    OrderId = rewardMessage.OrderId,
                    RewardsActivity = Convert.ToInt32(rewardMessage.RewardsActivity),
                    UserId = rewardMessage.UserId,
                    RewardsDate = DateTime.Now,
                };

                await using var _db = new AppDbContext(_dbOptions);
                await _db.Rewards.AddAsync(reward);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                
            }
        }


    }
}
