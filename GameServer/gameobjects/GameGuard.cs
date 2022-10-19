using System.Collections;
using DOL.AI.Brain;
using DOL.Language;

namespace DOL.GS
{
    public class GameGuard : GameNPC
    {
        public GameGuard() : base()
        {
            m_ownBrain = new GuardBrain();
            m_ownBrain.Body = this;
        }

        public GameGuard(INpcTemplate template) : base(template)
        {
            m_ownBrain = new GuardBrain();
            m_ownBrain.Body = this;
        }

        public override bool IsStealthed => (Flags & eFlags.STEALTH) != 0;

        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = new ArrayList(4);
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.GetExamineMessages.Examine", 
                                                GetName(0, true, player.Client.Account.Language, this), GetPronoun(0, true, player.Client.Account.Language),
                                                GetAggroLevelString(player, false)));
            return list;
        }

        public void StartAttack(GameObject attackTarget)
        {
            attackComponent.StartAttack(attackTarget);

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
            {
                if (player != null)
                    switch (Realm)
                    {
                        case eRealm.Albion:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Albion.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                        case eRealm.Midgard:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Midgard.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                        case eRealm.Hibernia:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Hibernia.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                    }
            }
        }
    }
}