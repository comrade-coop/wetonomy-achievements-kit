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

			state.Set("MintTokenManager", this.MintTokenManager);
			state.Set("BurnTokenManager", this.BurnTokenManager);
			state.Set("Exchanger", this.Exchanger);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.BurnTokenManager = state.Get<Address>("BurnTokenManager");
			this.MintTokenManager = state.Get<Address>("MintTokenManager");
			this.Exchanger = state.Get<Address>("Exchanger");
		}

		protected override void Split(Address tokenManager, IReadOnlyTaggedTokens availableTokens)
		{
			if (tokenManager.Equals(this.BurnTokenManager))
			{
				this.SendMessage(this.Exchanger, ExchangeAction.Type, new Dictionary<string, object>()
				{
					{ ExchangeAction.Amount, availableTokens.TotalBalance.ToString() },
					{ ExchangeAction.FromTokenManager, this.BurnTokenManager.ToString() },
					{ ExchangeAction.ToTokenManager, this.MintTokenManager.ToString() },
				});
			}
			else
			{
				base.Split(tokenManager, availableTokens);
			}
		}
	}
}