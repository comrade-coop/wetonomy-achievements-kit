// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;
using TokenSystem.TokenFlow;
using TokenSystem.TokenFlow.Actions;
using TokenSystem.TokenManagerBase.Actions;

namespace Wetonomy.Achievements
{
	public class AchievementFactory : TokenExchanger
	{
		protected Address BurnTokenManager { get; set; }

		protected Address MintTokenManager { get; set; }

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.AddAddress("BurnTokenManager", this.BurnTokenManager);
			state.AddAddress("MintTokenManager", this.MintTokenManager);

			return state;
		}

		public override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.BurnTokenManager = state.GetAddress("BurnTokenManager");
			this.MintTokenManager = state.GetAddress("MintTokenManager");
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("User"))
			{
				this.Acl.AddPermission(
					payload.GetAddress("User"),
					CreateAchievementAction.Type,
					this.Address);
			}

			base.Initialize(payload);
		}

		protected override bool HandlePayloadAction(PayloadAction action)
		{
			switch (action.Type)
			{
				case CreateAchievementAction.Type:
					var recipients = new HashSet<Address>(
						action.Payload.GetList<string>("Recipients").Select(Address.FromBase64String));
					this.CreateAchievement(recipients);
					return true;
				default:
					return base.HandlePayloadAction(action);
			}
		}

		protected override bool HandleForwardAction(ForwardAction action)
		{
			var result = base.HandleForwardAction(action);

			// If we are forwarding a burn action to the "burn manager", mint tokens in exchange
			// Care must be taken to actually forward the action through the AchievementFactory -- otherwise, it is possible to inadvertently burn the tokens instead of exchanging them.
			if (result &&
				action.Type == BurnAction.Type &&
				action.FinalTarget == this.BurnTokenManager)
			{
				var amount = BigInteger.Parse(action.Payload.GetString(BurnAction.Amount));
				var finalAmount = amount * this.ExchangeRateNumerator / this.ExchangeRateDenominator;

				this.SendAction(this.MintTokenManager, MintAction.Type, new Dictionary<string, object>()
				{
					{ MintAction.Amount, finalAmount.ToString() },
					{ MintAction.To, action.Origin.ToBase64String() },
				});
			}

			return result;
		}

		protected Address CreateAchievement(ISet<Address> recipients)
		{
			var address = this.CreateContract<Achievement>(new Dictionary<string, object>()
			{
				{ "MintTokenManager", this.MintTokenManager.ToBase64String() },
				{ "BurnTokenManager", this.BurnTokenManager.ToBase64String() },
				{ "Exchanger", this.Address.ToBase64String() },
			});

			this.Acl.AddPermission(
				address,
				ExchangeAction.Type,
				this.Address);

			return address;
		}
	}
}