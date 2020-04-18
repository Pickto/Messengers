using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;
namespace Messengers
{
	public class MessengerViewModel : ViewModel
	{
		Hero messenger;

		[DataSourceProperty]
		public SelectorVM<SelectorItemVM> TargetToDeliverySelection
		{
			get
			{
				return MessengersBehaviours.Instance.TargetToDeliverySelection;
			}
		}

		[DataSourceProperty]
		public string MessengerDescription
		{
			get
			{
				return $"It will take about {MessengersBehaviours.Instance.days} {MessengersBehaviours.DaysEnding(MessengersBehaviours.Instance.days)} and will cost {MessengersBehaviours.Instance.cost} dinars.";
			}						 
		}
		public MessengerViewModel(ref Hero messenger)
		{
			this.messenger = messenger;
		}
		private void ExitMessengerMenu()
		{
			Mission.Current.EndMission();
			ScreenManager.PopScreen();
			if (Hero.MainHero.Gold >= MessengersBehaviours.Instance.cost)
			{
				Hero.MainHero.ChangeHeroGold(-MessengersBehaviours.Instance.cost);
				MessengersBehaviours.Instance.messengerMap.Add(messenger.Name.ToString(), new Messenger(messenger, MessengersBehaviours.Instance.HeroesNotInParty[TargetToDeliverySelection.SelectedItem.StringItem]));
			}
		}
		private void CancelMessengerrMenu()
		{
			Mission.Current.EndMission();
			ScreenManager.PopScreen();
		}
		public void RefreshProperties()
		{
			OnPropertyChanged("messengerDescription");
		}
	}
}
