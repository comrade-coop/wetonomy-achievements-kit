// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Kits;
using StrongForce.Core.Permissions;
using TokenSystem.TokenManagerBase;
using TokenSystem.TokenManagerBase.Actions;

namespace Wetonomy.Achievements
{
	public class AchievementsKit : KitContract
	{
		protected int ExchangeRateNumerator { get; set; }

		protected int ExchangeRateDenominator { get; set; }

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("ExchangeRateNumerator", this.ExchangeRateNumerator);
			state.Set("ExchangeRateDenominator", this.ExchangeRateDenominator);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.ExchangeRateNumerator = state.Get<int>("ExchangeRateNumerator");
			this.ExchangeRateDenominator = state.Get<int>("ExchangeRateDenominator");
		}

		protected override void Instantiate(Address initialManager)
		{
			var burnTokenManager = this.CreateContract<TokenManager>(new Dictionary<string, object>()
			{
				{ "Admin", this.Address.ToString() },
				{ "User", AccessControlList.AnyAddress },
			});

			var mintTokenManager = this.CreateContract<TokenManager>(new Dictionary<string, object>()
			{
				{ "Admin", this.Address.ToString() },
				{ "User", AccessControlList.AnyAddress },
			});

			var achievementFactory = this.CreateContract<AchievementFactory>(new Dictionary<string, object>()
			{
				{ "Admin", initialManager.ToString() },
				{ "User", AccessControlList.AnyAddress },
				{ "BurnTokenManager", burnTokenManager.ToString() },
				{ "MintTokenManager", mintTokenManager.ToString() },
				{ "ExchangeRateNumerator", this.ExchangeRateNumerator },
				{ "ExchangeRateDenominator", this.ExchangeRateDenominator },
			});

			this.SendMessage(burnTokenManager, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionSender, achievementFactory.ToString() },
				{ AddPermissionAction.PermissionType, BurnOtherAction.Type },
				{ AddPermissionAction.PermissionTarget, burnTokenManager.ToString() },
			});

			this.SendMessage(burnTokenManager, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionSender, AccessControlList.AnyAddress },
				{ RemovePermissionAction.PermissionType, TransferAction.Type },
				{ RemovePermissionAction.PermissionTarget, mintTokenManager.ToString() },
			});

			this.SendMessage(mintTokenManager, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionSender, achievementFactory.ToString() },
				{ AddPermissionAction.PermissionType, MintAction.Type },
				{ AddPermissionAction.PermissionTarget, mintTokenManager.ToString() },
			});

			this.ChangeAdmin(burnTokenManager, initialManager);

			this.ChangeAdmin(mintTokenManager, initialManager);
		}

		private void ChangeAdmin(Address target, Address newAdmin)
		{
			this.SendMessage(target, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionSender, newAdmin.ToString() },
				{ AddPermissionAction.PermissionType, AddPermissionAction.Type },
				{ AddPermissionAction.PermissionTarget, target.ToString() },
			});

			this.SendMessage(target, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionSender, newAdmin.ToString() },
				{ AddPermissionAction.PermissionType, RemovePermissionAction.Type },
				{ AddPermissionAction.PermissionTarget, target.ToString() },
			});

			this.SendMessage(target, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionSender, this.Address.ToString() },
				{ RemovePermissionAction.PermissionType, AddPermissionAction.Type },
				{ RemovePermissionAction.PermissionTarget, target.ToString() },
			});

			this.SendMessage(target, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionSender, this.Address.ToString() },
				{ RemovePermissionAction.PermissionType, RemovePermissionAction.Type },
				{ RemovePermissionAction.PermissionTarget, target.ToString() },
			});
		}
	}
}