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
using DOL.GS.Realm;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.Heretic, "Heretic", "Acolyte")]
	public class ClassHeretic : ClassAcolyte
	{
		public ClassHeretic()
			: base()
		{
			m_profession = "PlayerClass.Profession.TempleofArawn";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.PIE;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}

		public override List<PlayerRace> EligibleRaces => new List<PlayerRace>()
		{
             // Comment out OpenDAoC changes, had line commented out
             PlayerRace.Korazh, PlayerRace.Avalonian, PlayerRace.Briton, PlayerRace.Inconnu,
		};
	}
}
