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

namespace DOL.GS.Quests.Atlantis.Artifacts
{
	/// <summary>
	/// Quest for the Cloudsong artifact.
	/// </summary>
	/// <author>Aredhel</author>
	class Cloudsong : ArtifactQuest
	{
		/// <summary>
		/// The name of the quest (not necessarily the same as
		/// the name of the reward).
		/// </summary>
		public override string Name
		{
			get { return "Cloudsong"; }
		}

		/// <summary>
		/// The reward for this quest.
		/// </summary>
		private static String m_artifactID = "Cloudsong";
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
						return "Defeat Eramai.";
					case 2:
						return "Turn in the completed book.";
					default:
						return base.Description;
				}
			}
		}

		public Cloudsong()
			: base() { }

		public Cloudsong(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		/// <summary>
		/// This constructor is needed to load quests from the DB.
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
		public Cloudsong(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		/// <summary>
		/// Quest initialisation.
		/// </summary>
		public static void Init()
		{
			ArtifactQuest.Init(m_artifactID, typeof(Cloudsong));
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
			GamePlayer player = source as GamePlayer;
			Scholar scholar = target as Scholar;
			if (player == null || scholar == null)
				return false;

			if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactID)
			{
				Dictionary<String, DbItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactID,
					(eCharacterClass)player.CharacterClass.ID, (eRealm)player.Realm);

				IDictionaryEnumerator versionsEnum = versions.GetEnumerator();
				versionsEnum.MoveNext();

				if (versions.Count > 0 && RemoveItem(player, item))
				{
					GiveItem(scholar, player, ArtifactID, versionsEnum.Value as DbItemTemplate);
					String reply = String.Format("Thank you! Here, take this cloak. {0}",
						"I hope you find it useful. Please don't lose it, I can't replace it!");
					scholar.TurnTo(player);
					scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
					FinishQuest();
					return true;
				}
			}

            return base.ReceiveItem(source, target, item);
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

            if (Step == 1)
            {
                if (text.ToLower() == ArtifactID.ToLower())
                {
                    String reply = String.Format("Do you have the story that goes with Cloudsong? {0} {1} {2}",
                        "I'd very much like to read it. If you don't, go, get the scrolls and use them.",
                        "Then, when you've translated them into a book, return the book to me, and I will",
                        "give you the artifact. Do you have the [story]?");
                    scholar.TurnTo(player);
                    scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    Step = 2;
                    return true;
                }
            }

            if (text.ToLower() == "story" && Step > 1)
            {
                scholar.TurnTo(player);
                scholar.SayTo(player, eChatLoc.CL_PopupWindow,
                    "Well, hand me the story. If you don't have it, go out and get it!");
                return true;
            }

			return false;
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			// Need to do anything here?
		}
	}
}
