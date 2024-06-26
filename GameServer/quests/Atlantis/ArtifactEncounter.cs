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
using DOL.Database;
using DOL.Events;

namespace DOL.GS.Quests.Atlantis
{
	/// <summary>
	/// Base class for all artifact encounters.
	/// </summary>
	/// <author>Aredhel</author>
	public class ArtifactEncounter : AbstractQuest
	{
		public ArtifactEncounter(GamePlayer questingPlayer)
			: base(questingPlayer) { }

		public ArtifactEncounter(GamePlayer questingPlayer, DbQuest dbQuest)
			: base(questingPlayer, dbQuest) { }

		public override bool CheckQuestQualification(GamePlayer player)
		{
			return (player == null);
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			// TODO.
		}
	}
}
