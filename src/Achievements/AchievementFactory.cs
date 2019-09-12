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
using TokenSystem.Tokens;

namespace Wetonomy.Achievements
{
	public class AchievementFactory : TokenExchanger
	{
		protected List<Address> Achievements { get; set; }

		protected Address BurnTokenManager { get; set; }

		protected Address MintTokenManager { get; set; }

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("BurnTokenManager", this.BurnTokenManager);
			state.Set("MintTokenManager", this.MintTokenManager);
			state.Set("Achievements", this.Achievements);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.BurnTokenManager = state.Get<Address>("BurnTokenManager");
			this.MintTokenManager = state.Get<Address>("MintTokenManager");
			this.Achievements = state.GetList<Address>("Achievements").ToList();
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("User"))
			{
				this.Acl.AddPermission(
					payload.Get<Address>("User"),
					CreateAchievementAction.Type,
					this.Address);
			}

			base.Initialize(payload);
		}

		protected override void HandleMessage(Message message)
		{
			switch (message.Type)
			{
				case CreateAchievementAction.Type:
					
					this.CreateAchievement(message.Payload);
					break;
				default:
					base.HandleMessage(message);
					return;
			}
		}

		protected Address CreateAchievement(IDictionary<string, object> payload)
		{
			var recipients = payload.GetList<Address>("Recipients").ToHashSet();
			var describtion = payload.Get<string>("Describtion");
			var address = this.CreateContract<Achievement>(new Dictionary<string, object>()
			{
				{ "MintTokenManager", this.MintTokenManager.ToString() },
				{ "BurnTokenManager", this.BurnTokenManager.ToString() },
				{ "Exchanger", this.Address.ToString() },
				{ "Describtion", describtion },
				// { "TokenContributors", new Dictionary<Address, object>()}
			});

			this.Acl.AddPermission(
				address,
				ExchangeAction.Type,
				this.Address);

			this.Achievements.Add(address);

			return address;
		}
	}
}