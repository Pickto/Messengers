using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;

namespace Messengers
{
	public class MessengerScreen : ScreenBase
	{
		private Hero messenger;
		private List<string> members;
		private MessengerViewModel _datasource;

		private GauntletLayer _gauntletLayer;

		private GauntletMovie _movie;

		private bool _firstRender;

		public MessengerScreen(Hero messenger, List<string> members)
		{
			this.messenger = messenger;
			this.members = members;
		}
		protected override void OnInitialize()
		{
			base.OnInitialize();
			_datasource = new MessengerViewModel(ref messenger);
			MessengersBehaviours.Instance.TargetToDeliverySelection = new SelectorVM<SelectorItemVM>(members, 0, new Action<SelectorVM<SelectorItemVM>>((SelectorVM<SelectorItemVM> selector)=> 
			{	
				MessengersBehaviours.Instance.TargetToDeliverySelection = selector;
				MessengersBehaviours.Instance.days = Messenger.DaysCount(messenger, MessengersBehaviours.Instance.HeroesNotInParty[MessengersBehaviours.Instance.TargetToDeliverySelection.SelectedItem.StringItem]);
				MessengersBehaviours.Instance.cost = Messenger.CostCount(messenger, MessengersBehaviours.Instance.HeroesNotInParty[MessengersBehaviours.Instance.TargetToDeliverySelection.SelectedItem.StringItem]);
				_datasource.RefreshProperties();
			}));
			MessengersBehaviours.Instance.days = Messenger.DaysCount(messenger, MessengersBehaviours.Instance.HeroesNotInParty[MessengersBehaviours.Instance.TargetToDeliverySelection.SelectedItem.StringItem]);
			MessengersBehaviours.Instance.cost = Messenger.CostCount(messenger, MessengersBehaviours.Instance.HeroesNotInParty[MessengersBehaviours.Instance.TargetToDeliverySelection.SelectedItem.StringItem]);
			_gauntletLayer = new GauntletLayer(100);
			_gauntletLayer.IsFocusLayer = true;
			AddLayer(_gauntletLayer);
			_gauntletLayer.InputRestrictions.SetInputRestrictions();
			ScreenManager.TrySetFocus(_gauntletLayer);
			_movie = _gauntletLayer.LoadMovie("MessengerScreen", _datasource);
			_firstRender = true;
		}
		protected override void OnFrameTick(float dt)
		{
			base.OnFrameTick(dt);
		}
	}
}
