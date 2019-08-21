// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using StrongForce.Core;
using StrongForce.Core.Kits;
using StrongForce.Core.Permissions;
using TokenSystem.TokenManagerBase;
using TokenSystem.TokenManagerBase.Actions;

namespace Wetonomy.Achievements
{
	public class AchievementsKit : Kit
	{
		public AchievementsKit(int exchangeRateNumerator, int exchangeRateDenominator)
		{
			this.ExchangeRateNumerator = exchangeRateNumerator;
			this.ExchangeRateDenominator = exchangeRateDenominator;
		}

		public int ExchangeRateNumerator { get; }

		public int ExchangeRateDenominator { get; }

		public override Address Instantiate(Address initialManager)
		{
			var burnTokenManager = this.CreateAddress();
			var mintTokenManager = this.CreateAddress();
			var achievementFactory = this.CreateAddress();

			this.CreateContract<TokenManager>(burnTokenManager, new Dictionary<string, object>()
			{
				["Acl"] = new AccessControlList(new Permission[]
				{
					new Permission(BurnAction.Type, AccessControlList.AnyAddress, burnTokenManager),
					new Permission(BurnOtherAction.Type, achievementFactory, burnTokenManager),
				}).GetState(),
			});

			this.CreateContract<TokenManager>(mintTokenManager, new Dictionary<string, object>()
			{
				["Acl"] = new AccessControlList(new Permission[]
				{
					new Permission(BurnAction.Type, AccessControlList.AnyAddress, mintTokenManager),
					new Permission(TransferAction.Type, AccessControlList.AnyAddress, mintTokenManager),
					new Permission(MintAction.Type, achievementFactory, mintTokenManager),
				}).GetState(),
			});

			this.CreateContract<AchievementFactory>(achievementFactory, new Dictionary<string, object>()
			{
				["BurnTokenManager"] = burnTokenManager.ToBase64String(),
				["MintTokenManager"] = mintTokenManager.ToBase64String(),
				["ExchangeRateNumerator"] = this.ExchangeRateNumerator,
				["ExchangeRateDenominator"] = this.ExchangeRateDenominator,

				["Admin"] = initialManager,
				["User"] = AccessControlList.AnyAddress,
			});

			return achievementFactory;
		}
	}
}