using System; 
using System.Collections; 
using System.Collections.Generic;
using Server; 
using Server.Misc;
using Server.Gumps; 
using Server.Items; 
using Server.Network; 
using Server.Targeting; 
using Server.ContextMenus; 


namespace Server.Mobiles 
{ 
   public class AnimalTrainerLord : BaseCreature//was BaseVendor 
   { 
      	private static Dictionary<Mobile, DateTime> m_LastPetSale = new Dictionary<Mobile, DateTime>();
		private static TimeSpan PetSaleCooldown = TimeSpan.FromHours(1);

    	private bool AppraiseMode = false;

      	[Constructable]
      	public AnimalTrainerLord() : base(AIType.AI_Thief, FightMode.None, 10, 1, 0.4, 1.6 ) 
 
      	{ 
			InitStats( 85, 75, 65 ); 
			Name = this.Female ? NameList.RandomName( "female" ) : NameList.RandomName( "male" );
			Title = "the animal broker";

				Body = 0x191;
				Hue = Utility.RandomSkinHue(); 
				
				AddItem( new Boots( Utility.RandomBirdHue() ) );
				AddItem( new ShepherdsCrook() );
				AddItem( new Cloak( Utility.RandomBirdHue() ) );
				AddItem( new FancyShirt( Utility.RandomBirdHue() ) );
				AddItem( new Kilt( Utility.RandomBirdHue() ) );
				AddItem( new BodySash( Utility.RandomBirdHue() ) );
				
				HairItemID = 0x203C;   // The ItemID of the hair you want
				HairHue = 1175; 
					
		}

			public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
			{
				base.GetContextMenuEntries( from, list );
				list.Add( new TamingBODDealerEntry( from, this ) );
			}

			public override void OnMovement( Mobile m, Point3D oldLocation )
			{
				if ( InRange( m, 4 ) && !InRange( oldLocation, 4 ) )
				{
					if ( m is PlayerMobile && !m.Hidden ) 
					{
									switch (Utility.Random(10))
									{
											case 0: Say("Pet Collector here, just tell me you want to sell!"); break;
											case 1: Say("Buying rare animals of all kind, simply tell me you want to sell one"); break;
											case 2: Say("Sell me your tamed pets, will pay well!"); break;
											case 3: Say("If you'd like a price estimate on a pet, just tell me to appraise one"); break;
											case 4: Say("I can appraise an animal for you, just ask me to."); break;
											case 5: Say("I am in need of help with contracts... "); break;
									}
					
					}
				}

			}
		
		private class PetSaleTarget : Target 
		{ 
			private AnimalTrainerLord m_Trainer; 

			public PetSaleTarget( AnimalTrainerLord trainer ) : base( 12, false, TargetFlags.None ) 
			{ 
				m_Trainer = trainer; 
			} 

			protected override void OnTarget( Mobile from, object targeted ) 
			{ 
				if ( targeted is BaseCreature ) 
			m_Trainer.EndPetSale( from, (BaseCreature)targeted ); 
				else if ( targeted == from ) 
			m_Trainer.SayTo( from, 502672 ); // HA HA HA! Sorry, I am not an inn. 
			} 
		} 

		public void BeginPetSale(Mobile from, bool appraise)
		{
			if (Deleted || !from.Alive)
				return;

			if (!appraise)
			{
				DateTime lastSale;

				if (m_LastPetSale.ContainsKey(from))
				{
					lastSale = (DateTime)m_LastPetSale[from];
					TimeSpan remaining = (lastSale + PetSaleCooldown) - DateTime.UtcNow;

					if (remaining > TimeSpan.Zero)
					{
						SayTo(from, "My pens are still full! You must wait " + remaining.Minutes + " minutes and " + remaining.Seconds + " seconds before selling another pet.");
						return;
					}
				}
			}

			AppraiseMode = appraise;

			if (appraise)
				SayTo(from, "Which beast would you like to appraise?");
			else
				SayTo(from, "Which beast are you selling?");

			from.Target = new PetSaleTarget(this);
		}

		//RUFO beginfunction
		private void SellPetForGold(Mobile from, BaseCreature pet, int goldamount)
		{
			
			double chance = pet.MinTameSkill/150;
			Item gold = null;

			if (goldamount < 10000)
				gold = new Gold(goldamount);
			else 
				gold = new BankCheck(goldamount);
							
			pet.ControlTarget = null; 
			pet.ControlOrder = OrderType.None; 
			pet.Internalize(); 
			pet.SetControlMaster( null ); 
			pet.SummonMaster = null;
			pet.Delete();

			Container backpack = from.Backpack;
			if ( backpack == null || !backpack.TryDropItem( from, gold, false ) ) 
			{ 
				gold.MoveToWorld( from.Location, from.Map );
			}		
		}
		//RUFO endfunction

		public static int ValuatePet(BaseCreature pet, Mobile broker)
		{
				
			double basevalue = pet.MinTameSkill;
			if (basevalue >= 125)
			{
				pet.MinTameSkill = 124;//divide by 0 check
				basevalue = 124;
			}
			
			if (!pet.CanAngerOnTame) // easier tames are worth less this way
				basevalue /= 1.15;
			
			double final = 0;
			double step = 10;
			double factorial = 1/ ((125-basevalue)/(pet.MinTameSkill*15));
			if (basevalue < step)
				final = basevalue * factorial;				
			else 
			{	
				while ( basevalue > 0 )
				{
					if (basevalue > step)
					{
						basevalue -= step;
						final += step * factorial;
								
					}
					else
					{
						final += basevalue * factorial;
						basevalue = 0;
					}
				}
			}		
			// to counter money-printing tameables
			double petprice = final / 3.0;
			int petpriceint = Convert.ToInt32(petprice);
			
			if (petpriceint <= 10)
				petpriceint = 10;
			
			return petpriceint;

		}
		public void EndPetSale( Mobile from, BaseCreature pet ) 
		{ 
			if ( Deleted || !from.CheckAlive() ) 
				return;    
			
			if ( !pet.Controlled || pet.ControlMaster != from ) 
				SayTo( from, 1042562 ); // You do not own that pet! 
			else if ( pet.IsDeadPet ) 
				SayTo( from, 1049668 ); // Living pets only, please. 
			else if ( pet.Summoned ) 
				SayTo( from, 502673 ); // I can not PetSale summoned creatures. 
			else if ( pet.Body.IsHuman ) 
				SayTo( from, 502672 ); // HA HA HA! Sorry, I am not an inn. 
			else if ( (pet is PackLlama || pet is PackHorse || pet is Beetle) && (pet.Backpack != null && pet.Backpack.Items.Count > 0) ) 
				SayTo( from, 1042563 ); // You need to unload your pet. 
			else if ( pet.Combatant != null && pet.InRange( pet.Combatant, 12 ) && pet.Map == pet.Combatant.Map ) 
					SayTo( from, 1042564 ); // I'm sorry.  Your pet seems to be busy. 
			else 
			{ 
		
			double oldvalue = pet.MinTameSkill;
			int petpriceint = ValuatePet(pet, this);

				if (AppraiseMode)
				{
					this.Say("I can pay you " + petpriceint + " for this pet.");
					pet.MinTameSkill = oldvalue; // resets the value to what it was;
					return;
				} 
				else
				{
					m_LastPetSale[from] = DateTime.UtcNow;
				}

				SellPetForGold(from, pet, petpriceint);
				Titles.AwardFame( from, (pet.Fame / 100), true );

				if (petpriceint <= 400)	
					this.Say( "I have plenty of " + pet.Name + " so I'll give you " + petpriceint + " gold");
			
				else if (petpriceint <= 1000)	
					this.Say( "Thank you {0}, I will add this " + pet.Name + " to my collection!  Here is " + petpriceint + " for your troubles",from.Name  );
				
				else if (petpriceint <= 3000)	
					this.Say( "A Rare find!!! Thank you for " + pet.Name + " it's worth " + petpriceint + " to the right buyer..");
				
				else if (petpriceint <= 6000)	
					this.Say( "What an amazing Specimen!  I will pay you " + petpriceint + " for it! " );
				
				else if (petpriceint >= 12000)	
					this.Say( "I'll pay " + petpriceint + "!  I've always wanted one of these!!! " );
				
				
			}
		}
				
		public override bool HandlesOnSpeech( Mobile from ) 
		{ 
			return true; 
		} 

		public override void OnSpeech( SpeechEventArgs e ) 
		{
			if( e.Mobile.InRange( this, 4 ))
			{
			if ( ( e.Speech.ToLower() == "sell" ) )//was sellpet
			{
			BeginPetSale( e.Mobile, false );
			}
			else if ( ( e.Speech.ToLower() == "appraise" ) )
			{
			BeginPetSale( e.Mobile, true );
			}
			else 
			{ 
			base.OnSpeech( e ); 
			}
			}
		
		} 

		public AnimalTrainerLord( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 

			writer.Write( (int) 0 ); // version 
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 

			int version = reader.ReadInt(); 
		} 
	} 
	
		public class TamingBODDealerEntry : ContextMenuEntry
		{
			private static TimeSpan Delay = TimeSpan.FromHours(6);
			
			private static Dictionary<PlayerMobile,DateTime> LastUsers = new Dictionary<PlayerMobile,DateTime>();//pas sérialisé car c'est pas si important
			
			private Mobile m_Mobile;
			private Mobile m_Giver;
			
			public TamingBODDealerEntry( Mobile from, Mobile giver ) : base( 6146, 3 )
			{
				m_Mobile = from;
				m_Giver = giver;
			}

			private static int GetTamingBODAmount(Mobile from)
			{
			    if (from == null)
			        return 1;

			    double totalSkill = from.Skills[SkillName.Druidism].Value
			                      + from.Skills[SkillName.Taming].Value
			                      + from.Skills[SkillName.Herding].Value
			                      + from.Skills[SkillName.Veterinary].Value;

			    int skillSum = (int)totalSkill;

			    if (skillSum < 100)
			        return Utility.RandomMinMax(1, 3);
				// 125 in taming, druidism, herding, veterinary
			    else if (skillSum == 500)
			        return Utility.RandomMinMax(11, 21);
			    else if (skillSum >= 499)
			        return Utility.RandomMinMax(9, 17);
			    else if (skillSum >= 400)
			        return Utility.RandomMinMax(7, 13);
			    else if (skillSum >= 300)
			        return Utility.RandomMinMax(5, 9);
			    else if (skillSum >= 200)
			        return Utility.RandomMinMax(3, 7);
			    else // 100 - 199
			        return Utility.RandomMinMax(2, 5);
			}

			public override void OnClick()
			{
				if( !( m_Mobile is PlayerMobile ) )
					return;
				
				PlayerMobile mobile = (PlayerMobile) m_Mobile;
				DateTime lastUse;
				
    			if (LastUsers.TryGetValue(mobile, out lastUse))
    			{
    			    TimeSpan cooldown = Delay - (DateTime.UtcNow - lastUse);

    			    if (cooldown > TimeSpan.Zero)
    			    {
				     m_Giver.Say(String.Format("I'll have another contract for you in  {0} hour{1} and {2} minute{3}.",
  					 cooldown.Hours, cooldown.Hours == 1 ? "" : "s",
  					 cooldown.Minutes, cooldown.Minutes == 1 ? "" : "s"));
    			        return;
    			    } 
    			}
				
				if(mobile.Skills[SkillName.Taming].Value +  mobile.Skills[SkillName.Druidism].Value < 50.0)
				{
					m_Giver.Say( "Sorry, I don't think you have the skills to help me. Come back when you have learned more about the art of animal taming!" );
				}
				else if(CanGetContract(mobile))
				{
					mobile.SendGump(new SpeechGump( mobile, "The Animal Broker", SpeechFunctions.SpeechText( m_Giver, m_Mobile, "Animal Broker" ) ));
					int amount = GetTamingBODAmount(mobile);
					mobile.AddToBackpack(new TamingBOD(amount));
					LastUsers[mobile] = DateTime.UtcNow;
				}
			}
			
			private bool CanGetContract(PlayerMobile asker)
			{
				//if(asker.AccessLevel > AccessLevel.Player)return true;
				
				if(!LastUsers.ContainsKey(asker))
				{
					LastUsers.Add(asker,DateTime.UtcNow);
					return true;
				}
				else
				{
					if(DateTime.UtcNow-LastUsers[asker] < Delay)
					{
						return false;
					}
					else
					{
						LastUsers[asker]=DateTime.UtcNow;
						return true;
					}
				}
			}
		}
	} 



