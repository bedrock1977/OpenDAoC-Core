using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Cache;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.API;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerTitles;
using DOL.GS.Quests;
using log4net;

namespace DOL.GS.DailyQuest.Albion
{
	public class EveryLittleBitHelpsQuestAlb : Quests.DailyQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "[Daily] Every little bit helps";
		protected const int minimumLevel = 40;
		protected const int maximumLevel = 50;

		private static GameNPC ReyAlb = null; // Start NPC

		private int _playersKilledMid = 0;
		private int _playersKilledHib = 0;
		protected const int MAX_KILLGOAL = 5;

		// Constructors
		public EveryLittleBitHelpsQuestAlb() : base()
		{
		}

		public EveryLittleBitHelpsQuestAlb(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public EveryLittleBitHelpsQuestAlb(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public EveryLittleBitHelpsQuestAlb(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		public override int Level
		{
			get
			{
				// Quest Level
				return minimumLevel;
			}
		}
		
		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;

			#region defineNPCs

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Rey", eRealm.Albion);

			if (npcs.Length > 0)
				foreach (GameNPC npc in npcs)
				{
					if (npc.CurrentRegionID == 1 && npc.X == 583867 && npc.Y == 477355)
					{
						ReyAlb = npc;
						break;
					}
				}

			if (ReyAlb == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Rey , creating it ...");
				ReyAlb = new GameNPC();
				ReyAlb.Model = 26;
				ReyAlb.Name = "Rey";
				ReyAlb.GuildName = "Bone Collector";
				ReyAlb.Realm = eRealm.Albion;
				//Druim Ligen Location
				ReyAlb.CurrentRegionID = 1;
				ReyAlb.Size = 60;
				ReyAlb.Level = 59;
				ReyAlb.X = 583867;
				ReyAlb.Y = 477355;
				ReyAlb.Z = 2600;
				ReyAlb.Heading = 3054;
				ReyAlb.Flags |= GameNPC.eFlags.PEACE;
				ReyAlb.AddToWorld();
				if (SAVE_INTO_DATABASE)
				{
					ReyAlb.SaveIntoDatabase();
				}
			}

			#endregion

			#region defineItems
			#endregion

			#region defineObject
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(ReyAlb, GameObjectEvent.Interact, new DOLEventHandler(TalkToRey));
			GameEventMgr.AddHandler(ReyAlb, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRey));

			/* Now we bring to Rey the possibility to give this quest to players */
			ReyAlb.AddQuestToGive(typeof (EveryLittleBitHelpsQuestAlb));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" Alb initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//if not loaded, don't worry
			if (ReyAlb == null)
				return;
			// remove handlers
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(ReyAlb, GameObjectEvent.Interact, new DOLEventHandler(TalkToRey));
			GameEventMgr.RemoveHandler(ReyAlb, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRey));

			/* Now we remove to Rey the possibility to give this quest to players */
			ReyAlb.RemoveQuestToGive(typeof (EveryLittleBitHelpsQuestAlb));
		}

		protected static void TalkToRey(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(ReyAlb.CanGiveQuest(typeof (EveryLittleBitHelpsQuestAlb), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			EveryLittleBitHelpsQuestAlb quest = player.IsDoingQuest(typeof (EveryLittleBitHelpsQuestAlb)) as EveryLittleBitHelpsQuestAlb;

			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					switch (quest.Step)
					{
						case 1:
							ReyAlb.SayTo(player, "Find and kill enemies of Midgard and Hibernia. You will find suitable players in the frontiers.");
							break;
						case 2:
							ReyAlb.SayTo(player, "Hello " + player.Name + ", did you [kill enemies] for your reward?");
							break;
					}
				}
				else
				{
					ReyAlb.SayTo(player, "Hello "+ player.Name +", I am Rey. My master, Fen, has tasked me with collecting bones for a project he's working on. "+
					                     "I'm way behind quota and could use some... subcontractors to [help me out]. \n\n"+
					                     "\nCan you lend me a hand? A leg could probably work too.");
				}
			}
				// The player whispered to the NPC
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "help me out":
							player.Out.SendQuestSubscribeCommand(ReyAlb, QuestMgr.GetIDForQuestType(typeof(EveryLittleBitHelpsQuestAlb)), "Will you undertake " + questTitle + "?");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "kill enemies":
							if (quest.Step == 2)
							{
								player.Out.SendMessage("Thank you for your contribution!", eChatType.CT_Chat, eChatLoc.CL_PopupWindow);
								quest.FinishQuest();
							}
							break;
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}
		
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (EveryLittleBitHelpsQuestAlb)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			//if (player.HasFinishedQuest(typeof(Academy_47)) == 0) return false;

			//if (!CheckPartAccessible(player,typeof(CityOfCamelot)))
			//	return false;

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			EveryLittleBitHelpsQuestAlb quest = player.IsDoingQuest(typeof (EveryLittleBitHelpsQuestAlb)) as EveryLittleBitHelpsQuestAlb;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, "Good, now go out there and shed some blood!");
			}
			else
			{
				SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
				quest.AbortQuest();
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(EveryLittleBitHelpsQuestAlb)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			if(ReyAlb.CanGiveQuest(typeof (EveryLittleBitHelpsQuestAlb), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (EveryLittleBitHelpsQuestAlb)) != null)
				return;

			if (response == 0x00)
			{
				player.Out.SendMessage("Thank you for helping me.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Check if we can add the quest!
				if (!ReyAlb.GiveQuest(typeof (EveryLittleBitHelpsQuestAlb), player, 1))
					return;

				ReyAlb.SayTo(player, "You will find suitable players in the frontiers.");

			}
		}

		//Set quest name
		public override string Name
		{
			get { return questTitle; }
		}

		// Define Steps
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "You will find suitable players in the frontiers. \n" +
						       "Players Killed: Hibernia ("+ _playersKilledHib +" | "+ MAX_KILLGOAL +")" +
						       "Players Killed: Midgard ("+ _playersKilledMid +" | "+ MAX_KILLGOAL +")";
					case 2:
						return "Return to Rey in Castle Sauvage for your Reward.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(EveryLittleBitHelpsQuestAlb)) == null)
				return;

			if (sender != m_questPlayer)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;

				if (gArgs.Target.Realm == eRealm.Midgard && gArgs.Target.Realm != player.Realm && gArgs.Target is GamePlayer && _playersKilledMid < MAX_KILLGOAL) 
				{
					_playersKilledMid++;
					player.Out.SendMessage("[Daily] Killed Midgard Enemy: (" + _playersKilledMid + " | " + MAX_KILLGOAL + ")", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
					player.Out.SendQuestUpdate(this);
				}
				else if (gArgs.Target.Realm == eRealm.Hibernia && gArgs.Target.Realm != player.Realm && gArgs.Target is GamePlayer && _playersKilledHib < MAX_KILLGOAL) 
				{
					_playersKilledHib++;
					player.Out.SendMessage("[Daily] Killed Hibernia Enemy: (" + _playersKilledHib + " | " + MAX_KILLGOAL + ")", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
					player.Out.SendQuestUpdate(this);
				}
				
				if (_playersKilledMid >= MAX_KILLGOAL && _playersKilledHib >= MAX_KILLGOAL)
				{
					// FinishQuest or go back to Rey
					Step = 2;
				}
				
			}
		}
		
		public override string QuestPropertyKey
		{
			get => "EveryLittleBitHelpsQuestAlb";
			set { ; }
		}
		
		public override void LoadQuestParameters()
		{
			_playersKilledHib = GetCustomProperty("PlayersKilledHib") != null ? int.Parse(GetCustomProperty("PlayersKilledHib")) : 0;
			_playersKilledMid = GetCustomProperty("PlayersKilledMid") != null ? int.Parse(GetCustomProperty("PlayersKilledMid")) : 0;
		}

		public override void SaveQuestParameters()
		{
			SetCustomProperty("PlayersKilledHib", _playersKilledHib.ToString());
			SetCustomProperty("PlayersKilledMid", _playersKilledMid.ToString());
		}


		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}

		public override void FinishQuest()
		{
			m_questPlayer.GainExperience(eXPSource.Quest, (m_questPlayer.ExperienceForNextLevel - m_questPlayer.ExperienceForCurrentLevel)/5, true);
			m_questPlayer.AddMoney(Money.GetMoney(0,0,m_questPlayer.Level * 2,32,Util.Random(50)), "You receive {0} as a reward.");
			AtlasROGManager.GenerateOrbAmount(m_questPlayer, 1000);
			_playersKilledHib = 0;
			_playersKilledMid = 0;
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
			
		}
	}
}
