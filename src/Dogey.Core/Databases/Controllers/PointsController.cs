﻿using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dogey
{
    public class PointsController : DbController<PointsDatabase>
    {
        public PointsController(PointsDatabase db) : base(db) { }

        public Task<Wallet> GetWalletAsync(IUser user)
            => _db.Wallets.SingleOrDefaultAsync(x => x.Id == user.Id);
        public Task<PointLog> GetLogAsync(ulong id)
            => _db.Logs.SingleOrDefaultAsync(x => x.SenderId == id);
        public Task<Price> GetPriceAsync(IGuild guild, string name)
            => _db.Prices.SingleOrDefaultAsync(x => x.GuildId == guild.Id && x.Name.ToLower() == name.ToLower());

        public Task<List<Price>> GetPricesAsync(IGuild guild)
            => _db.Prices.Where(x => x.GuildId == guild.Id).ToListAsync();

        public async Task<PointLog> TradePointsAsync(IUser sender, IUser receiver, int amount)
        {
            var senderWallet = await GetOrCreateWalletAsync(sender);
            if (senderWallet.Balance < amount) return null;

            var receiverWallet = await GetOrCreateWalletAsync(receiver);

            var log = await CreateAsync(new PointLog
            {
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                UserId = receiver.Id,
                EarningType = EarningType.Trade,
                Amount = amount
            });

            senderWallet.Balance -= amount;
            receiverWallet.Balance += amount;
            await _db.SaveChangesAsync();
            return log;
        }

        public async Task<Wallet> GetOrCreateWalletAsync(IUser user)
        {
            var wallet = await GetWalletAsync(user);
            if (wallet != null) return wallet;
            return await CreateAsync(new Wallet { Id = user.Id });
        }

        public async Task<Wallet> CreateAsync(Wallet wallet)
        {
            await _db.Wallets.AddAsync(wallet);
            await _db.SaveChangesAsync();
            return wallet;
        }
        public async Task<PointLog> CreateAsync(PointLog log)
        {
            await _db.Logs.AddAsync(log);
            await _db.SaveChangesAsync();
            return log;
        }

        public async Task<Wallet> ModifyAsync(Wallet wallet)
        {
            if (wallet.Balance < 0) wallet.Balance = 0;
            await _db.SaveChangesAsync();
            return wallet;
        }
        
        public Task DeleteAsync(PointLog log)
        {
            _db.Logs.Remove(log);
            return Task.CompletedTask;
        }
    }
}
