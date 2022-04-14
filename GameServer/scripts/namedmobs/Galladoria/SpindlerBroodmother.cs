﻿using System;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Events;
using DOL.Database;
using DOL.GS;
namespace DOL.GS
{
    public class SpindlerBroodmother : GameEpicBoss
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SpindlerBroodmother()
            : base()
        {
        }
        public override int GetResist(eDamageType damageType)
        {
            switch (damageType)
            {
                case eDamageType.Slash: return 60; // dmg reduction for melee dmg
                case eDamageType.Crush: return 60; // dmg reduction for melee dmg
                case eDamageType.Thrust: return 60; // dmg reduction for melee dmg
                default: return 85; // dmg reduction for rest resists
            }
        }

        public override double AttackDamage(InventoryItem weapon)
        {
            return base.AttackDamage(weapon) * Strength / 100;
        }

        public override int MaxHealth
        {
            get { return 20000; }
        }

        public override int AttackRange
        {
            get { return 450; }
            set { }
        }

        public override bool HasAbility(string keyName)
        {
            if (IsAlive && keyName == GS.Abilities.CCImmunity)
                return true;

            return base.HasAbility(keyName);
        }

        public override double GetArmorAF(eArmorSlot slot)
        {
            return 700;
        }

        public override double GetArmorAbsorb(eArmorSlot slot)
        {
            // 85% ABS is cap.
            return 0.55;
        }

        public override void Die(GameObject killer)
        {
            SpawnAfterDead();
            base.Die(killer);
        }

        public override bool AddToWorld()
        {
            foreach (GameNPC npc in GetNPCsInRadius(4000))
            {
                if (npc.RespawnInterval == -1 && npc.Brain is SBDeadAddsBrain)
                {
                    npc.RemoveFromWorld();
                }
            }

            INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(60166449);
            LoadTemplate(npcTemplate);
            Strength = npcTemplate.Strength;
            Dexterity = npcTemplate.Dexterity;
            Constitution = npcTemplate.Constitution;
            Quickness = npcTemplate.Quickness;
            Piety = npcTemplate.Piety;
            Intelligence = npcTemplate.Intelligence;
            Charisma = npcTemplate.Charisma;
            Empathy = npcTemplate.Empathy;

            RespawnInterval = ServerProperties.Properties.SET_SI_EPIC_ENCOUNTER_RESPAWNINTERVAL * 60000; //1min is 60000 miliseconds
            Faction = FactionMgr.GetFactionByID(96);
            Faction.AddFriendFaction(FactionMgr.GetFactionByID(96));
            SpindlerBroodmotherBrain sBrain = new SpindlerBroodmotherBrain();
            SetOwnBrain(sBrain);
            SaveIntoDatabase();
            LoadedFromScript = false;
            base.AddToWorld();
            return true;
        }

        public void SpawnAfterDead()
        {
            for (int i = 0; i < Util.Random(20, 25); i++) // Spawn 20-25 adds
            {
                SBDeadAdds Add = new SBDeadAdds();
                Add.X = X + Util.Random(-50, 80);
                Add.Y = Y + Util.Random(-50, 80);
                Add.Z = Z;
                Add.CurrentRegion = CurrentRegion;
                Add.Heading = Heading;
                Add.AddToWorld();
            }
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC[] npcs;

            npcs = WorldMgr.GetNPCsByNameFromRegion("Spindler Broodmother", 191, (eRealm) 0);
            if (npcs.Length == 0)
            {
                log.Warn("Spindler Broodmother not found, creating it...");

                log.Warn("Initializing Spindler Broodmother...");
                SpindlerBroodmother SB = new SpindlerBroodmother();
                SB.Name = "Spindler Broodmother";
                SB.Model = 904;
                SB.Realm = 0;
                SB.Level = 81;
                SB.Size = 125;
                SB.CurrentRegionID = 191; //galladoria

                SB.Strength = 500;
                SB.Intelligence = 220;
                SB.Piety = 220;
                SB.Dexterity = 200;
                SB.Constitution = 200;
                SB.Quickness = 125;
                SB.BodyType = 5;
                SB.MeleeDamageType = eDamageType.Slash;
                SB.Faction = FactionMgr.GetFactionByID(96);
                SB.Faction.AddFriendFaction(FactionMgr.GetFactionByID(96));

                SB.X = 21283;
                SB.Y = 51707;
                SB.Z = 10876;
                SB.MaxDistance = 2000;
                SB.TetherRange = 2500;
                SB.MaxSpeedBase = 300;
                SB.Heading = 0;

                SpindlerBroodmotherBrain ubrain = new SpindlerBroodmotherBrain();
                ubrain.AggroLevel = 100;
                ubrain.AggroRange = 500;
                SB.SetOwnBrain(ubrain);
                INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(60166449);
                SB.LoadTemplate(npcTemplate);
                SB.AddToWorld();
                SB.Brain.Start();
                SB.SaveIntoDatabase();
            }
            else
                log.Warn(
                    "Spindler Broodmother exist ingame, remove it and restart server if you want to add by script code.");
        }
    }
}

namespace DOL.AI.Brain
{
    public class SpindlerBroodmotherBrain : StandardMobBrain
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SpindlerBroodmotherBrain()
            : base()
        {
            AggroLevel = 100;
            AggroRange = 600;
        }
        public static bool Spawn_Splinders = false;
        public override void Think()
        {
            if (!HasAggressionTable())
            {
                //set state to RETURN TO SPAWN
                Body.Health = Body.MaxHealth;
                Spawn_Splinders = false;
                StartCastMezz = false;
                CanCast = false;
                RandomTarget = null;
                FSM.SetCurrentState(eFSMStateType.RETURN_TO_SPAWN);
                foreach (GameNPC npc in Body.GetNPCsInRadius(4000))
                {
                    if (npc.Brain is SBAddsBrain && npc != null && npc.IsAlive)
                    {
                        npc.RemoveFromWorld();
                    }
                }
            }
            if (HasAggro)
            {
                if(Spawn_Splinders==false)
                {
                    new RegionTimer(Body, new RegionTimerCallback(SpawnSplinder), 10000);
                    Spawn_Splinders = true;
                }
                if (StartCastMezz== false)
                {
                    new RegionTimer(Body, new RegionTimerCallback(PickRandomTarget), Util.Random(20000, 30000));
                    StartCastMezz = true;
                }
            }

            base.Think();
        }

        public int SpawnSplinder(RegionTimer timer)
        {
            if (HasAggro && Body.IsAlive)
            {
                SBAdds Add = new SBAdds();
                Add.X = Body.X + Util.Random(-50, 80);
                Add.Y = Body.Y + Util.Random(-50, 80);
                Add.Z = Body.Z;
                Add.CurrentRegion = Body.CurrentRegion;
                Add.Heading = Body.Heading;
                Add.AddToWorld();
                new RegionTimer(Body, new RegionTimerCallback(ResetSpawnSplinder), Util.Random(15000,25000));
            }
            return 0;
        }
        public int ResetSpawnSplinder(RegionTimer timer)
        {
            Spawn_Splinders = false;
            return 0;
        }
        #region broodmother mezz
        public static bool CanCast = false;
        public static bool StartCastMezz = false;
        public static GamePlayer randomtarget = null;
        public static GamePlayer RandomTarget
        {
            get { return randomtarget; }
            set { randomtarget = value; }
        }
        List<GamePlayer> Enemys_To_Mezz = new List<GamePlayer>();
        public int PickRandomTarget(RegionTimer timer)
        {
            if (HasAggro)
            {
                foreach (GamePlayer player in Body.GetPlayersInRadius(2000))
                {
                    if (player != null)
                    {
                        if (player.IsAlive && player.Client.Account.PrivLevel == 1)
                        {
                            if (!Enemys_To_Mezz.Contains(player))
                            {
                                Enemys_To_Mezz.Add(player);
                            }
                        }
                    }
                }
                if (Enemys_To_Mezz.Count > 0)
                {
                    if (CanCast == false)
                    {
                        GamePlayer Target = (GamePlayer)Enemys_To_Mezz[Util.Random(0, Enemys_To_Mezz.Count - 1)];//pick random target from list
                        RandomTarget = Target;//set random target to static RandomTarget
                        new RegionTimer(Body, new RegionTimerCallback(CastMezz), 3000);
                        CanCast = true;
                    }
                }
            }
            return 0;
        }
        public int CastMezz(RegionTimer timer)
        {
            if (HasAggro && RandomTarget != null)
            {
                GamePlayer oldTarget = (GamePlayer)Body.TargetObject;//old target
                if (RandomTarget != null && RandomTarget.IsAlive)
                {
                    Body.TargetObject = RandomTarget;
                    Body.TurnTo(RandomTarget);
                    Body.CastSpell(BossMezz, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                }
                if (oldTarget != null) Body.TargetObject = oldTarget;//return to old target
                new RegionTimer(Body, new RegionTimerCallback(ResetMezz), 5000);
            }
            return 0;
        }
        public int ResetMezz(RegionTimer timer)
        {
            RandomTarget = null;
            CanCast = false;
            StartCastMezz = false;
            return 0;
        }
        #endregion

        protected Spell m_BossmezSpell;
        /// <summary>
        /// The Mezz spell.
        /// </summary>
        protected Spell BossMezz
        {
            get
            {
                if (m_BossmezSpell == null)
                {
                    DBSpell spell = new DBSpell();
                    spell.AllowAdd = false;
                    spell.CastTime = 0;
                    spell.RecastDelay = 0;
                    spell.ClientEffect = 5376;
                    spell.Icon = 5376;
                    spell.TooltipId = 5376;
                    spell.Name = "Mesmerized";
                    spell.Range = 1500;
                    spell.Radius = 300;
                    spell.SpellID = 11716;
                    spell.Duration = 60;
                    spell.Target = "Enemy";
                    spell.Type = "Mesmerize";
                    spell.Uninterruptible = true;
                    spell.MoveCast = true;
                    spell.DamageType = (int) eDamageType.Spirit; //Spirit DMG Type
                    m_BossmezSpell = new Spell(spell, 70);
                    SkillBase.AddScriptedSpell(GlobalSpellsLines.Mob_Spells, m_BossmezSpell);
                }
                return m_BossmezSpell;
            }
        }
    }
}

/////////////////////////////////////////Minions here//////////////////////////////////
namespace DOL.GS
{
    public class SBAdds : GameNPC
    {
        public SBAdds() : base()
        {
        }
        public override int GetResist(eDamageType damageType)
        {
            switch (damageType)
            {
                case eDamageType.Slash: return 30; // dmg reduction for melee dmg
                case eDamageType.Crush: return 30; // dmg reduction for melee dmg
                case eDamageType.Thrust: return 30; // dmg reduction for melee dmg
                default: return 50; // dmg reduction for rest resists
            }
        }
        public override double GetArmorAF(eArmorSlot slot)
        {
            return 400;
        }
        public override double GetArmorAbsorb(eArmorSlot slot)
        {
            // 85% ABS is cap.
            return 0.25;
        }
        public override int MaxHealth
        {
            get { return 5000; }
        }

        public override double AttackDamage(InventoryItem weapon)
        {
            return base.AttackDamage(weapon) * Strength / 100;
        }
        public override short Quickness { get => base.Quickness; set => base.Quickness = 80; }
        public override short Strength { get => base.Strength; set => base.Strength = 250; }
        public override bool AddToWorld()
        {
            Model = 904;
            Name = "Newly-born spindler";
            MeleeDamageType = eDamageType.Slash;
            RespawnInterval = -1;
            Size = (byte) Util.Random(50, 60);
            Level = (byte) Util.Random(56, 59);
            Faction = FactionMgr.GetFactionByID(96);
            Faction.AddFriendFaction(FactionMgr.GetFactionByID(96));
            Realm = 0;
            SBAddsBrain adds = new SBAddsBrain();
            LoadedFromScript = true;
            SetOwnBrain(adds);
            base.AddToWorld();
            return true;
        }

        public override void DropLoot(GameObject killer) //no loot
        {
        }
        public override void Die(GameObject killer)
        {
            base.Die(killer); //null to not gain experience
        }
    }
}

namespace DOL.AI.Brain
{
    public class SBAddsBrain : StandardMobBrain
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SBAddsBrain()
            : base()
        {
            AggroLevel = 100;
            AggroRange = 1000;
        }

        public override void Think()
        {
            Body.IsWorthReward = false;
            foreach (GamePlayer player in Body.GetPlayersInRadius(2000))
            {
                if (player != null && player.IsAlive)
                {
                    if (player.CharacterClass.ID is 48 or 47 or 42 or 46) //bard,druid,menta,warden
                    {
                        if (Body.TargetObject != player)
                        {
                            Body.TargetObject = player;
                            Body.StartAttack(player);
                        }
                    }
                    else
                    {
                        Body.TargetObject = player;
                        Body.StartAttack(player);
                    }
                }
            }

            base.Think();
        }
    }
}

//////////////////////////////////adds after main boss die////////////////////////
namespace DOL.GS
{
    public class SBDeadAdds : GameNPC
    {
        public SBDeadAdds() : base()
        {
        }
        public override int GetResist(eDamageType damageType)
        {
            switch (damageType)
            {
                case eDamageType.Slash: return 20; // dmg reduction for melee dmg
                case eDamageType.Crush: return 20; // dmg reduction for melee dmg
                case eDamageType.Thrust: return 20; // dmg reduction for melee dmg
                default: return 30; // dmg reduction for rest resists
            }
        }
        public override double GetArmorAF(eArmorSlot slot)
        {
            return 300;
        }
        public override double GetArmorAbsorb(eArmorSlot slot)
        {
            // 85% ABS is cap.
            return 0.15;
        }
        public override int MaxHealth
        {
            get { return 800; }
        }

        public override double AttackDamage(InventoryItem weapon)
        {
            return base.AttackDamage(weapon) * Strength / 100;
        }
        public override bool AddToWorld()
        {
            Model = 904;
            Name = "underdeveloped spindler";
            MeleeDamageType = eDamageType.Slash;
            RespawnInterval = -1;
            Strength = 100;
            IsWorthReward = false; //worth no reward
            Size = (byte) Util.Random(30, 40);
            Level = 50;
            Faction = FactionMgr.GetFactionByID(96);
            Faction.AddFriendFaction(FactionMgr.GetFactionByID(96));
            Realm = 0;
            SBDeadAddsBrain adds = new SBDeadAddsBrain();
            LoadedFromScript = true;
            SetOwnBrain(adds);
            base.AddToWorld();
            return true;
        }
    }
}

namespace DOL.AI.Brain
{
    public class SBDeadAddsBrain : StandardMobBrain
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SBDeadAddsBrain()
            : base()
        {
            AggroLevel = 100;
            AggroRange = 1000;
        }
        public override void Think()
        {
            base.Think();
        }
    }
}