﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.SaveSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;

namespace Messengers
{
    public class Messenger
    {
        [SaveableField(1)]
        public Hero messenger;
        [SaveableField(2)]
        public Hero target;
        [SaveableField(3)]
        public int days_need;
        public Messenger(Hero messenger, Hero target)
        {
            this.messenger = messenger;
            this.target = target;
            days_need = DaysCount(messenger, target);
            MobileParty.MainParty.MemberRoster.RemoveTroop(messenger.CharacterObject);
            messenger.StayingInSettlement = MessengersBehaviours.messengerSettlement;
            messenger.ChangeState(Hero.CharacterStates.Disabled);
            InformationManager.DisplayMessage(new InformationMessage($"{messenger.Name} sent for { target.Name}. It will take about { days_need} {MessengersBehaviours.DaysEnding(days_need)}.")); 
        }
        public void DailyTick()
        {
            if (days_need > 0)
            {
                days_need -= 1;
            }
            else
            {
                if (!MobileParty.MainParty.MemberRoster.Contains(target.CharacterObject) && !target.IsPrisoner && !target.IsPartyLeader && target.IsAlive && !target.IsHeroOccupied(Hero.EventRestrictionFlags.Default) && target.GovernorOf == null)
                {
                    MobileParty.MainParty.AddElementToMemberRoster(target.CharacterObject, 1);
                    InformationManager.DisplayMessage(new InformationMessage($"{messenger.Name} returns to the squad, bringing with him { target.Name}."));
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage($"{messenger.Name} returns to the squad."));
                }
                MobileParty.MainParty.AddElementToMemberRoster(messenger.CharacterObject, 1);
                messenger.ChangeState(Hero.CharacterStates.Active);
                MessengersBehaviours.Instance.messengerMap.Remove(messenger.Name.ToString());
            }
        }
        public static int DaysCount(Hero messenger, Hero target)
        {
            return Math.Max(1, (int)(target.GetMapPoint().Position2D.Distance(messenger.GetMapPoint().Position2D) / 100));
        }
        public static int CostCount(Hero messenger, Hero target)
        {
            float modifier = 1 - Math.Min(0.9f, messenger.GetRelationWithPlayer() / 100.0f);
            return Math.Max(1, (int)(target.GetMapPoint().Position2D.Distance(messenger.GetMapPoint().Position2D) * modifier));
        }
    }
}