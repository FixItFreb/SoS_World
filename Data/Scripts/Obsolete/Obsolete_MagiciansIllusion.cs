using System;
using Server;

namespace Server.Items
{
    public class MagiciansIllusion : DoubleBladedStaff
	{
		public override int InitMinHits{ get{ return 80; } }
		public override int InitMaxHits{ get{ return 160; } }

        [Constructable]
        public MagiciansIllusion()
        {
            Name = "Magician's Illusion";
            Hue = 1072;
            WeaponAttributes.HitLightning = 15;
            WeaponAttributes.HitLowerAttack = 15;
            WeaponAttributes.HitMagicArrow = 5;
            WeaponAttributes.SelfRepair = 2;
            SkillBonuses.SetValues( 0, SkillName.Magery, 5.0 );
            Attributes.BonusMana = 30;
            Attributes.ReflectPhysical = 15;
            Attributes.SpellChanneling = 1;
            Attributes.SpellDamage = 25;
            IntRequirement = 100;
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, "Artefact");
        }

        public override void GetDamageTypes( Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct )
        {
            phys = 100;
            cold = 0;
            fire = 0;
            nrgy = 0;
            pois = 0;
            chaos = 0;
            direct = 0;
        }
        public MagiciansIllusion( Serial serial )
            : base( serial )
        {
        }
        public override void Serialize( GenericWriter writer )
        {
            base.Serialize( writer );
            writer.Write( (int)0 );
        }
        private void Cleanup( object state ){ Item item = new Artifact_MagiciansIllusion(); Server.Misc.Cleanup.DoCleanup( (Item)state, item ); }

public override void Deserialize( GenericReader reader )
        {
            base.Deserialize( reader ); Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerStateCallback( Cleanup ), this );
            int version = reader.ReadInt();
        }
    }
}
