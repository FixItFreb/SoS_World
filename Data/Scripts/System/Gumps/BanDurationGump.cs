using System;
using System.Net;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Server;
using Server.Items;
using Server.Prompts;
using Server.Network;
using Server.Accounting;
using Server.Commands;

namespace Server.Gumps
{
	public class BanDurationGump : Gump
	{
		private ArrayList m_List;

		public void AddButtonLabeled( int x, int y, int buttonID, string text )
		{
			AddButton( x, y - 1, 4005, 4007, buttonID, GumpButtonType.Reply, 0 );
			AddHtml( x + 35, y, 240, 20, text, false, false );
		}

		public void AddTextField( int x, int y, int width, int height, int index )
		{
			AddBackground( x - 2, y - 2, width + 4, height + 4, 0x2486 );
			AddTextEntry( x + 2, y + 2, width - 4, height - 4, 0, index, "" );
		}

		public static ArrayList MakeList( object obj )
		{
			ArrayList list = new ArrayList( 1 );
			list.Add( obj );
			return list;
		}

		public BanDurationGump( Account a ) : this( MakeList( a ) )
		{
		}

		public BanDurationGump( ArrayList list ) : base( (640 - 500) / 2, (480 - 305) / 2 )
		{
			m_List = list;

			int width = 500;
			int height = 305;

			AddPage( 0 );

			AddBackground( 0, 0, width, height, 0x1453 );

			//AddImageTiled( 10, 10, width - 20, 20, 2624 );
			//AddAlphaRegion( 10, 10, width - 20, 20 );
			AddHtml( 10, 10, width - 20, 20, "<CENTER>Ban Duration</CENTER>", false, false );

			//AddImageTiled( 10, 40, width - 20, height - 50, 2624 );
			//AddAlphaRegion( 10, 40, width - 20, height - 50 );

			AddButtonLabeled( 15, 45, 1, "Infinite" );
			AddButtonLabeled( 15, 65, 2, "From D:H:M:S" );

			AddInput( 3, 0, "Days" );
			AddInput( 4, 1, "Hours" );
			AddInput( 5, 2, "Minutes" );
			AddInput( 6, 3, "Seconds" );

			AddHtml( 170, 45, 240, 20, "Comments:", false, false );
			AddTextField( 170, 65, 315, height - 80, 10 );
		}

		public void AddInput( int bid, int idx, string name )
		{
			int x = 15;
			int y = 95 + (idx * 50);

			AddButtonLabeled( x, y, bid, name );
			AddTextField( x + 35, y + 20, 100, 20, idx );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( from.AccessLevel < AccessLevel.Administrator )
				return;

			TextRelay d = info.GetTextEntry( 0 );
			TextRelay h = info.GetTextEntry( 1 );
			TextRelay m = info.GetTextEntry( 2 );
			TextRelay s = info.GetTextEntry( 3 );

			TextRelay c = info.GetTextEntry( 10 );

			TimeSpan duration;
			bool shouldSet;

			string fromString = from.ToString();

			switch ( info.ButtonID )
			{
				case 0:
				{
					for ( int i = 0; i < m_List.Count; ++i )
					{
						Account a = (Account)m_List[i];

						a.SetUnspecifiedBan( from );
					}

					from.SendMessage( "Duration unspecified." );
					return;
				}
				case 1: // infinite
				{
					duration = TimeSpan.MaxValue;
					shouldSet = true;
					break;
				}
				case 2: // From D:H:M:S
				{
					if ( d != null && h != null && m != null && s != null )
					{
						try
						{
							duration = new TimeSpan( Utility.ToInt32( d.Text ), Utility.ToInt32( h.Text ), Utility.ToInt32( m.Text ), Utility.ToInt32( s.Text ) );
							shouldSet = true;

							break;
						}
						catch
						{
						}
					}

					duration = TimeSpan.Zero;
					shouldSet = false;

					break;
				}
				case 3: // From D
				{
					if ( d != null )
					{
						try
						{
							duration = TimeSpan.FromDays( Utility.ToDouble( d.Text ) );
							shouldSet = true;

							break;
						}
						catch
						{
						}
					}

					duration = TimeSpan.Zero;
					shouldSet = false;

					break;
				}
				case 4: // From H
				{
					if ( h != null )
					{
						try
						{
							duration = TimeSpan.FromHours( Utility.ToDouble( h.Text ) );
							shouldSet = true;

							break;
						}
						catch
						{
						}
					}

					duration = TimeSpan.Zero;
					shouldSet = false;

					break;
				}
				case 5: // From M
				{
					if ( m != null )
					{
						try
						{
							duration = TimeSpan.FromMinutes( Utility.ToDouble( m.Text ) );
							shouldSet = true;

							break;
						}
						catch
						{
						}
					}

					duration = TimeSpan.Zero;
					shouldSet = false;

					break;
				}
				case 6: // From S
				{
					if ( s != null )
					{
						try
						{
							duration = TimeSpan.FromSeconds( Utility.ToDouble( s.Text ) );
							shouldSet = true;

							break;
						}
						catch
						{
						}
					}

					duration = TimeSpan.Zero;
					shouldSet = false;

					break;
				}
				default: return;
			}

			if ( shouldSet ) {
				string comment = null;
				
				if ( c != null ) {
					comment = c.Text.Trim();

					if ( comment.Length == 0 )
						comment = null;
				}

				for ( int i = 0; i < m_List.Count; ++i )
				{
					Account a = (Account)m_List[i];

					a.SetBanTags( from, DateTime.Now, duration );

					if ( comment != null )
						a.Comments.Add( new AccountComment( from.RawName, String.Format( "Duration: {0}, Comment: {1}", (( duration == TimeSpan.MaxValue )? "Infinite" : duration.ToString()), comment ) ) );
				}

				if ( duration == TimeSpan.MaxValue )
					from.SendMessage( "Ban Duration: Infinite" );
				else
					from.SendMessage( "Ban Duration: {0}", duration );
			}
			else
			{
				from.SendMessage( "Time values were improperly formatted." );
				from.SendGump( new BanDurationGump( m_List ) );
			}
		}
	}
}