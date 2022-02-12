/*
Gudlaugr.
<author>Kelt</author>
 */
using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using System.Reflection;
using System.Collections;
using DOL.AI.Brain;
using DOL.GS.Scripts.DOL.AI.Brain;


namespace DOL.GS.Scripts
{

	public class Gudlaugr : GameNPC
	{
		/// <summary>
		/// Add Gudlaugr to World
		/// </summary>
		public override bool AddToWorld()
		{
			Realm = eRealm.None;
			Model = 650;
			Size = 40;
			Level = 64;
			Strength = 255;
			Dexterity = 120;
			Constitution = 1200;
			Intelligence = 220;
			Health = MaxHealth;
			Piety = 130;
			Empathy = 130;
			Charisma = 130;
			MaxDistance = 4000;
			TetherRange = 3500;
			Faction = FactionMgr.GetFactionByID(779);
			Name = "Gudlaugr";
			BodyType = 1;

			ScalingFactor = 40;
			base.SetOwnBrain(new GudlaugrBrain());
			GudlaugrBrain.StartRage = true;
			base.AddToWorld();
			
			return true;
		}
		
		public override int MaxHealth
		{
			get { return 1500 * Constitution / 100; }
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Gudlaugr NPC Initializing...");
		}
	}

	namespace DOL.AI.Brain
	{
		public class GudlaugrBrain : StandardMobBrain
		{
			public GudlaugrBrain() : base()
			{
			}

			public static bool StartRage = true;

			public override void Think()
			{
				if (Body.InCombat == true && Body.IsAlive && HasAggro)
				{
					if (Body.TargetObject != null)
					{
					}
				}
				else if (Body.IsReturningToSpawnPoint)
				{
					RageMode(!StartRage);
				}
			}
			
			/// <summary>
			/// Called whenever the gudlaugr's body sends something to its brain.
			/// </summary>
			/// <param name="e">The event that occured.</param>
			/// <param name="sender">The source of the event.</param>
			/// <param name="args">The event details.</param>
			public override void Notify(DOLEvent e, object sender, EventArgs args)
			{
				base.Notify(e, sender, args);
				if (sender == this)
				{
					Gudlaugr gud = sender as Gudlaugr;
					if (e == GameObjectEvent.TakeDamage)
					{
						// Someone hit Gudlaugr. The Wolf starts to change model and Size.
						RageMode(StartRage);
						StartRage = false;
					}
				}
			}
			
			
			public void RageMode(bool rage) // We define here transmorph
			{
				GudlaugrBrain gudBrain = new GudlaugrBrain();
			
				if (!rage)
				{
					Body.ScalingFactor = 40;
					Body.Model = 650;
					Body.Size = 40;
					gudBrain.AggroLevel = 0;
					gudBrain.AggroRange = 0;
				}
				else
				{
					Body.ScalingFactor = 60;
					Body.Model = 649;
					Body.Size = 75;
					gudBrain.AggroLevel = 200;
					gudBrain.AggroRange = 550;

				}
				
			}
			
		}
	}
}