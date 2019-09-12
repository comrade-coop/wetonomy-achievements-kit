// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using System.Numerics;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using TokenSystem.TokenFlow;
using TokenSystem.TokenFlow.Actions;
using TokenSystem.TokenManagerBase.Actions;
using TokenSystem.Tokens;
using System.Linq;

namespace Wetonomy.Achievements
{
	public class Achievement : UniformTokenSplitter
	{
		protected Address MintTokenManager { get; set; }

		protected Address BurnTokenManager { get; set; }

		protected Address Exchanger { get; set; }

		protected string Describtion { get; set; }

		protected IDictionary<Address, ITaggedTokens> TokenContributors { get; set; }
			= new Dictionary<Address, ITaggedTokens>();

		protected override void HandleMessage(Message message)
		{
			switch (message.Type)
			{
				case TokensReceivedEvent.Type:
					var sender = message.Payload.Get<Address>(TokensReceivedEvent.From);
					var tokens = message.Payload.GetDictionary(TokensReceivedEvent.TokensTotal);
					if(sender != null)
					{
						if(this.TokenContributors.ContainsKey(sender)) this.TokenContributors[sender].AddToBalance(new TaggedTokens(tokens));
						else this.TokenContributors.Add(sender, new TaggedTokens(tokens));
					}
						
					this.Split(message.Sender, new ReadOnlyTaggedTokens(tokens));
					break;
				default:
					base.HandleMessage(message);
					return;
			}
		}
		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("MintTokenManager", this.MintTokenManager);
			state.Set("BurnTokenManager", this.BurnTokenManager);
			state.Set("Exchanger", this.Exchanger);
			state.Set("Describtion", this.Describtion);
			state.Set("TokenContributors", this.TokenContributors.ToDictionary(
				kv => kv.Key.ToString(),
				kv => (object)kv.Value.GetState()));
			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.BurnTokenManager = state.Get<Address>("BurnTokenManager");
			this.MintTokenManager = state.Get<Address>("MintTokenManager");
			this.Exchanger = state.Get<Address>("Exchanger");
			this.Describtion = state.Get<string>("Describtion");
			this.TokenContributors = state.GetDictionary("TokenContributors").ToDictionary(
				kv => Address.Parse(kv.Key),
				kv => (ITaggedTokens)new TaggedTokens((IDictionary<string, object>)kv.Value));
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