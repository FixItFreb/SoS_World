using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.XmlSpawner2
{
    public class XmlTitle : XmlAttachment
    {
        private bool m_Showing = false;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Showing
        {
            get
            {
                return m_Showing;
            }
            set
            {
                m_Showing = value;

                if (AttachedTo is Mobile)
                {
                    ((Mobile)AttachedTo).InvalidateProperties();
                }
                if (AttachedTo is Item)
                {
                    ((Item)AttachedTo).InvalidateProperties();
                }
            }
        }

        private string m_Title = null;    // title string

        [CommandProperty(AccessLevel.GameMaster)]
        public string Title
        {
            get { return m_Title; }
            set
            {
                m_Title = value;
                // change the title
                if (AttachedTo is Mobile)
                {
                    ((Mobile)AttachedTo).InvalidateProperties();
                }
                if (AttachedTo is Item)
                {
                    ((Item)AttachedTo).InvalidateProperties();
                }
            }
        }

        public static void SelectXmlTitle(object o, XmlTitle selected)
        {
            if (o == null)
                return;

            ArrayList titleList = XmlAttach.FindAttachments(o, typeof(XmlTitle));

            if (titleList != null && titleList.Count > 0)
            {
                foreach (XmlTitle t in titleList)
                {
                    if (t == selected)
                    {
                        t.Showing = true;
                    }
                    else
                    {
                        t.Showing = false;
                    }
                }
            }
        }

        public static void NoTitle(object o)
        {
            if (o == null)
                return;

            ArrayList titleList = XmlAttach.FindAttachments(o, typeof(XmlTitle));

            if (titleList != null && titleList.Count > 0)
            {
                foreach (XmlTitle t in titleList)
                {
                    t.Showing = false;
                }
            }
        }

        public static void AddTitles(object o, ObjectPropertyList list)
        {
            if (list == null || o == null) return;

            ArrayList alist = XmlAttach.FindAttachments(o, typeof(XmlTitle));

            if (alist != null && alist.Count > 0)
            {
                string titlestring = null;
                bool hastitle = false;
                foreach (XmlTitle t in alist)
                {
                    if (t == null || t.Deleted) continue;

                    if (t.Showing)
                    {
                        if (hastitle)
                        {
                            titlestring += '\n';
                        }
                        titlestring += Utility.FixHtml(t.Title);
                        hastitle = true;
                    }
                }
                if (hastitle)
                {
                    list.Add(1049644, "{0}", titlestring);
                }
            }
        }

        // These are the various ways in which the message attachment can be constructed.  
        // These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
        // Other overloads could be defined to handle other types of arguments

        // a serial constructor is REQUIRED
        public XmlTitle(ASerial serial)
            : base(serial)
        {
        }

        [Attachable]
        public XmlTitle(string name)
        {
            Name = name;
            Title = String.Empty;
        }

        [Attachable]
        public XmlTitle(string name, string title)
        {
            Name = name;
            Title = title;
        }

        [Attachable]
        public XmlTitle(string name, string title, bool showing)
        {
            Name = name;
            Title = title;
            Showing = showing;
        }

        [Attachable]
        public XmlTitle(string name, string title, double expiresin)
        {
            Name = name;
            Title = title;
            Expiration = TimeSpan.FromMinutes(expiresin);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write((string)m_Title);
            writer.Write(m_Showing);

        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Title = reader.ReadString();
            m_Showing = reader.ReadBool();
        }

        public override void OnDelete()
        {
            base.OnDelete();

            // remove the title when deleted
            if (AttachedTo is Mobile)
            {
                ((Mobile)AttachedTo).InvalidateProperties();
            }
            if (AttachedTo is Item)
            {
                ((Item)AttachedTo).InvalidateProperties();
            }
        }

        public override void OnAttach()
        {
            base.OnAttach();

            // apply the title immediately when attached
            if (AttachedTo is Mobile)
            {
                ((Mobile)AttachedTo).InvalidateProperties();
            }
            if (AttachedTo is Item)
            {
                ((Item)AttachedTo).InvalidateProperties();
            }
        }

        public override string OnIdentify(Mobile from)
        {
            if (from == null || from.AccessLevel == AccessLevel.Player) return null;

            if (Expiration > TimeSpan.Zero)
            {
                return String.Format("{2}: Title {0} expires in {1} mins", Title, Expiration.TotalMinutes, Name);
            }
            else
            {
                return String.Format("{1}: Title {0}", Title, Name);
            }
        }
    }
}
