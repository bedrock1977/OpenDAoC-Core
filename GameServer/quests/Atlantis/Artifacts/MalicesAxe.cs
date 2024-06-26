/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using DOL.Events;
using DOL.GS.Quests;
using DOL.Database;
using DOL.GS.PacketHandler;
using System.Collections;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
	/// <summary>
	/// Quest for the Malice's Axe artifact.
	/// </summary>
	/// <author>Aredhel</author>
	public class MalicesAxe : ArtifactQuest
	{
		/// <summary>
		/// The name of the quest (not necessarily the same as
		/// the name of the reward).
		/// </summary>
		public override string Name
		{
			get { return "Malice's Axe"; }
		}

		/// <summary>
		/// The artifact ID.
		/// </summary>
		private static String m_artifactID = "Malice's Axe";
		public override String ArtifactID
		{
			get { return m_artifactID; }
		}

		/// <summary>
		/// Description for the current step.
		/// </summary>
		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "Defeat Malamis.";
					case 2:
						return "Turn in the completed book.";
					case 3:
						return "Choose between a [slashing] version of Malice's Axe or a [crushing] one. Both one-handed and two-handed versions are available for both.";
					case 4:
						return "Choose between one handed or two handed versions.";
					default:
						return base.Description;
				}
			}
		}

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MalicesAxe()
			: base() { }

		public MalicesAxe(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
		public MalicesAxe(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Quest initialisation.
		/// </summary>
		public static void Init()
		{
			ArtifactQuest.Init(m_artifactID, typeof(MalicesAxe));
		}

        /// <summary>
        /// Check if player is eligible for this quest.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (!base.CheckQuestQualification(player))
                return false;

            // TODO: Check if this is the correct level for the quest.
            return (player.Level >= 45);
        }

		/// <summary>
		/// Handle an item given to the scholar.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, GameLiving target, DbInventoryItem item)
		{
			if (base.ReceiveItem(source, target, item))
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactID)
			{
				scholar.TurnTo(player);
				if (RemoveItem(player, item))
				{
					String reply = String.Format("You now have a decision to make, {0}. {1} {2} {3} {4} {5}",
						player.Name,
						"I can unlock your Malice Axe so it uses [slashing] skills or so it uses",
						"[crushing] skills. In both cases, I can unlock it as a one-handed weapon",
						"or a two-handed one. All you must do is decide which kind of damage you",
						"would like to do to your enemies. Once you have chosen, you cannot change",
						"your mind.");
					scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
					Step = 3;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Handle whispers to the scholar.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, GameLiving target, string text)
		{
			if (base.WhisperReceive(source, target, text))
				return true;

			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 1 && text.ToLower() == ArtifactID.ToLower())
			{
				String reply = "Ah, yes, the axe of Malice. It has an interesting tale, but I'm not sure I believe it. Did you find the story of the axe? If you have, please give it to me now.";
				scholar.TurnTo(player);
				scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
				Step = 2;
				return true;
			}
			else if (Step == 3)
			{
				switch (text.ToLower())
				{
					case "slashing":
					case "crushing":
						{
							SetCustomProperty("DamageType", text.ToLower());
							String reply = String.Format("Would you like your {0} Malice's Axe to be {1}",
								text.ToLower(),
								"[one handed] or [two handed]?");
							scholar.TurnTo(player);
							scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
							Step = 4;
							return true;
						}
				}
				return false;
			}
			else if (Step == 4)
			{
				switch (text.ToLower())
				{
					case "one handed":
					case "two handed":
						{
							String versionID = String.Format("{0};{1};",
								GetCustomProperty("DamageType"), text.ToLower());
							Dictionary<String, DbItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID,
								(eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);
                            DbItemTemplate template = versions[versionID];
							if (template == null)
							{
								log.Warn(String.Format("Artifact version {0} not found", versionID));
								return false;
							}
							if (GiveItem(scholar, player, ArtifactID, template))
							{
								String reply = String.Format("Here's your {0}. May it serve you well. {1}",
									template.Name,
									"Just don't lose it. You can't ever replace it.");
								scholar.TurnTo(player);
								scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
								FinishQuest();
								return true;
							}
							return false;
						}
				}
				return false;
			}

			return false;
		}
	}
}
