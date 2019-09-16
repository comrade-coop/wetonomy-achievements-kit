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

			state.Set("BurnTokenManager", this.BurnTokenManager);
			state.Set("MintTokenManager", this.MintTokenManager);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.BurnTokenManager = state.Get<Address>("BurnTokenManager");
			this.MintTokenManager = state.Get<Address>("MintTokenManager");
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
					var recipients = message.Payload.GetList<Address>("Recipients").ToHashSet();
					this.CreateAchievement(recipients);
					break;
				default:
					base.HandleMessage(message);
					return;
			}
		}

		protected Address CreateAchievement(ISet<Address> recipients)
		{
			var address = this.CreateContract<Achievement>(new Dictionary<string, object>()
			{
				{ "MintTokenManager", this.MintTokenManager.ToString() },
				{ "BurnTokenManager", this.BurnTokenManager.ToString() },
				{ "Exchanger", this.Address.ToString() },
			});

			this.Acl.AddPermission(
				address,
				ExchangeAction.Type,
				this.Address);

			return address;
		}
	}
}