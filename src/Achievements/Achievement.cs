// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using TokenSystem.TokenFlow;
using TokenSystem.TokenFlow.Actions;
using TokenSystem.TokenManagerBase.Actions;
using TokenSystem.Tokens;

namespace Wetonomy.Achievements
{
	public class Achievement : UniformTokenSplitter
	{
		protected Address MintTokenManager { get; set; }

		protected Address BurnTokenManager { get; set; }

		protected Address Exchanger { get; set; }

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.AddAddress("MintTokenManager", this.BurnTokenManager);
			state.AddAddress("BurnTokenManager", this.BurnTokenManager);
			state.AddAddress("Exchanger", this.Exchanger);

			return state;
		}

		public override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.BurnTokenManager = state.GetAddress("BurnTokenManager");
			this.MintTokenManager = state.GetAddress("MintTokenManager");
			this.Exchanger = state.GetAddress("Exchanger");
		}

		protected override void Split(Address tokenManager, IReadOnlyTaggedTokens availableTokens)
		{
			if (tokenManager.Equals(this.BurnTokenManager))
			{
				// Burn, forwarding through the Exchanger; it should re-mint the tokens in response. This would subsequently result in splitting the tokens when they are received.
				this.SendAction(this.Exchanger, ExchangeAction.Type, new Dictionary<string, object>()
				{
					{ ExchangeAction.Amount, availableTokens.TotalBalance.ToString() },
					{ ExchangeAction.FromTokenManager, this.BurnTokenManager.ToBase64String() },
					{ ExchangeAction.ToTokenManager, this.MintTokenManager.ToBase64String() },
				});
			}
			else
			{
				base.Split(tokenManager, availableTokens);
			}
		}
	}
}