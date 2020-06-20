using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.SaveSystem;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.HUDExtensions;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Map.Siege;
using TaleWorlds.Core.ViewModelCollection;
using SandBox.GauntletUI;

namespace Messengers
{
    class MessengersBehaviours: CampaignBehaviorBase
    {
        public Dictionary<string, Messenger> messengerMap = new Dictionary<string, Messenger>();
        public Dictionary<string, Hero> HeroesNotInParty;
        public Dictionary<string, IFaction> FactionsInWar;
        public SelectorVM<SelectorItemVM> TargetToDeliverySelection;
        public int cost;
        public int days;

        public static readonly MessengersBehaviours Instance = new MessengersBehaviours();
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
        }
        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            obj.AddPlayerLine("messenger_ask_to_find_companion", "hero_main_options", "messenger_ask_to_find_companion_ask", "Could you help me bring someone to me?", new ConversationSentence.OnConditionDelegate(conversation_is_companion_on_global_map), null, 100, new ConversationSentence.OnClickableConditionDelegate(exist_companion_not_in_party));
            obj.AddDialogLine("messenger_ok_on_ask_to_find_companion", "messenger_ask_to_find_companion_ask", "messenger_who_is", "Yes, of course, who would you like to find?", null, new ConversationSentence.OnConsequenceDelegate(open_menu_to_choice), 100, null);
        }
        private void DailyTick()
        {
            foreach (Messenger messenger in new List<Messenger>(messengerMap.Values))
            {
                messenger.DailyTick();
            }
        }
        private bool conversation_is_companion_on_global_map()
        {
            return !Hero.OneToOneConversationHero.IsNoble && Hero.OneToOneConversationHero.IsPlayerCompanion && (MobileParty.MainParty.CurrentSettlement == null || LocationComplex.Current == null);
        }
        private bool is_avaible_to_make_peace()
        {
            return conversation_is_companion_on_global_map() && Hero.MainHero.IsFactionLeader;
        }
        private bool exist_companion_not_in_party(out TextObject text)
        {
            HeroesNotInParty = new Dictionary<string, Hero>();
            IEnumerable<Hero> members = Hero.MainHero.Clan.Heroes.Concat(Hero.MainHero.Clan.Companions);
            foreach (Hero hero in members)
            {
                if (!MobileParty.MainParty.MemberRoster.Troops.Contains(hero.CharacterObject))
                {
                    if (!messengerMap.Values.Select((Messenger x) => { return x.target; }).Concat(messengerMap.Values.Select((Messenger x) => { return x.messenger; })).Contains(hero))
                    {
                        if (!hero.IsChild && !hero.IsPrisoner && !hero.IsPartyLeader && !hero.IsOccupiedByAnEvent() && hero.GovernorOf == null && hero.IsAlive)
                        {
                            HeroesNotInParty.Add(hero.Name.ToString(), hero);
                        }
                    }
                }
            }
            if (HeroesNotInParty.IsEmpty())
            {
                text = new TextObject("No lost companions");
                return false;
            }
            else
            {
                text = new TextObject("");
                return true;
            }

        }
        private bool conversation_make_peace_condition(out TextObject text)
        {
            //var skills = Hero.OneToOneConversationHero.GetSkillValue(SkillObject.Find(new MBGUID(603979789)));
            Campaign.Current.Kingdoms.First().IsAtWarWith(Hero.MainHero.Clan);
            foreach (IFaction faction in Campaign.Current.Factions)
            {
                if (faction != Hero.MainHero.Clan)
                {
                    if (faction.IsAtWarWith(Hero.MainHero.Clan))
                    {
                        FactionsInWar.Add(faction.Name.ToString(), faction);
                    }
                }
            }
            if (FactionsInWar.IsEmpty())
            {
                text = new TextObject("No factions with whom the war is declared");
                return false;
            }
            else
            {
                text = new TextObject("");
                return true;
            }
        }
        private void open_menu_to_choice()
        {
            ScreenManager.PushScreen(new MessengerScreen(Hero.OneToOneConversationHero, new List<string>(HeroesNotInParty.Keys)));
        }
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("messengerMap", ref messengerMap);
            if (messengerMap == null)
                messengerMap = new Dictionary<string, Messenger>();
        }
        public class MySaveDefiner : SaveableTypeDefiner
        {
            public MySaveDefiner() : base(378491783)
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(Messenger), 378491784);
            }

            protected override void DefineContainerDefinitions()
            {
                ConstructContainerDefinition(typeof(Dictionary<string, Messenger>));
            }
        }
        public static string DaysEnding(int days_count)
        {
            if (days_count == 1)
                return "day";
            else
                return "days";
        }
    }
}
