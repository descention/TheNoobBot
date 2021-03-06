﻿using System;
using System.ComponentModel;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Profiles_Converters.Converters
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class AvoidMobs : object
    {
        [XmlElement("AvoidMob", typeof (MobType), Form = XmlSchemaForm.Unqualified), XmlElement("Mob", typeof (MobType), Form = XmlSchemaForm.Unqualified), XmlChoiceIdentifier("ItemsElementName")]
        public MobType[] Items { get; set; }

        [XmlElement("ItemsElementName"), XmlIgnore]
        public ItemsChoiceType[] ItemsElementName { get; set; }
    }


    [XmlInclude(typeof (BlacklistMobType))]
    [XmlInclude(typeof (AvoidmobType))]
    [Serializable]
    public class MobType : object
    {
        private uint _entryId;

        [XmlAttribute]
        public string Name { get; set; }

        [XmlIgnore]
        [XmlAttribute("Id")]
        public uint Id // alias for reading
        {
            get { return _entryId; }
            set { _entryId = value; }
        }

        [XmlAttribute("Entry")]
        public uint Entry // force write Entry only
        {
            get { return _entryId; }
            set { _entryId = value; }
        }
    }

    [Serializable]
    public class SubProfileType : object
    {
        [XmlElement("AvoidMobs", typeof (AvoidMobs)), XmlElement("Blacklist", typeof (Blacklist)), XmlElement("Blackspots", typeof (Blackspots)),
         XmlElement("ContinentId", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "nonNegativeInteger"), XmlElement("ForceMail", typeof (ForceMail)), XmlElement("GrindArea", typeof (GrindArea)),
         XmlElement("Hotspots", typeof (Hotspots)), XmlElement("MailBlue", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified),
         XmlElement("MailGreen", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified),
         XmlElement("MailGrey", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified), XmlElement("MailPurple", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified),
         XmlElement("MailWhite", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified), XmlElement("Mailboxes", typeof (Mailboxes)),
         XmlElement("MaxLevel", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"), XmlElement("MinDurability", typeof (float), Form = XmlSchemaForm.Unqualified),
         XmlElement("MinFreeBagSlots", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "nonNegativeInteger"),
         XmlElement("MinLevel", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"), XmlElement("Name", typeof (string), Form = XmlSchemaForm.Unqualified),
         XmlElement("ProtectedItems", typeof (ProtectedItems)), XmlElement("SellBlue", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified),
         XmlElement("SellGreen", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified), XmlElement("SellGrey", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified),
         XmlElement("SellPurple", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified), XmlElement("SellWhite", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified),
         XmlElement("TargetElites", typeof (AdvancedBoolean), Form = XmlSchemaForm.Unqualified), XmlElement("Vendors", typeof (Vendors)), XmlElement("Quest", typeof (Quest)),
         XmlElement("QuestOrder", typeof (QuestOrderType)),
         XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items { get; set; }

        [XmlElement("ItemsElementName"), XmlIgnore]
        public ItemsChoiceType2[] ItemsElementName { get; set; }
    }

    [Serializable]
    public enum AdvancedBoolean : uint
    {
        True = 1,
        @true = 1,
        False = 0,
        @false = 0,
    }

    [Serializable]
    public class QuestOrderType : object
    {
        [XmlElement("Quest", typeof (Quest)), XmlChoiceIdentifier("ItemsElementName"), XmlElement("PickUp", typeof (PickUp)), XmlElement("TurnIn", typeof (TurnIn)),
         XmlElement("CustomBehavior", typeof (CustomBehavior)), XmlElement("If", typeof (If)), XmlElement("Objective", typeof (ObjectiveMetaType)), XmlElement("While", typeof (While)),
         XmlElement("MoveTo", typeof (MoveTo))]
        public object[] Items { get; set; }

        [XmlElement("ItemsElementName"), XmlIgnore]
        public ItemsChoiceType4[] ItemsElementName { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Blacklist : object
    {
        [XmlElement("Mob", Form = XmlSchemaForm.Unqualified)]
        public BlacklistMobType[] Mob { get; set; }
    }


    [Serializable]
    public class BlacklistMobType : MobType
    {
        [XmlAttribute]
        public BlacklistFlagsType Flags { get; set; }
    }

    [Serializable]
    public enum BlacklistFlagsType
    {
        All,
        Combat,
        Interact,
        Loot,
        Node,
        Pull,
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Blackspots : object
    {
        [XmlElement("Blackspot", Form = XmlSchemaForm.Unqualified)]
        public BlackspotType[] Blackspot { get; set; }
    }

    [Serializable]
    public class BlackspotType : object
    {
        public BlackspotType()
        {
            Height = 0F;
        }

        [XmlAttribute, DefaultValue("")]
        public string Name { get; set; }

        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }

        [XmlAttribute]
        public float Radius { get; set; }

        [XmlAttribute, DefaultValue(typeof (float), "0")]
        public float Height { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ForceMail : object
    {
        [XmlElement("Item", Form = XmlSchemaForm.Unqualified)]
        public ItemType[] Items { get; set; }
    }

    [Serializable]
    public class ItemType : object
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute(DataType = "positiveInteger")]
        public string Entry { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class GrindArea : object
    {
        [XmlElement("Blacklist", typeof (Blacklist)), XmlElement("Factions", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"), XmlElement("Hotspots", typeof (Hotspots)),
         XmlElement("LootRadius", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"),
         XmlElement("MaxDistance", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"),
         XmlElement("MaximumHotspotTime", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"), XmlElement("Name", typeof (string), Form = XmlSchemaForm.Unqualified),
         XmlElement("RandomizeHotspots", typeof (Boolean), Form = XmlSchemaForm.Unqualified), XmlElement("TargetMaxLevel", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"),
         XmlElement("TargetMinLevel", typeof (string), Form = XmlSchemaForm.Unqualified, DataType = "positiveInteger"), XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items { get; set; }

        [XmlElement("ItemsElementName"), XmlIgnore]
        public ItemsChoiceType1[] ItemsElementName { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Hotspots : object
    {
        [XmlElement("Hotspot")]
        public Point3DType[] Hotspot { get; set; }
    }

    [Serializable]
    [XmlRoot("Hotspot", Namespace = "", IsNullable = false)]
    public class Point3DType : object
    {
        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }
    }

    [Serializable]
    [XmlType(IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {
        Blacklist,
        Factions,
        Hotspots,
        LootRadius,
        MaxDistance,
        MaximumHotspotTime,
        Name,
        RandomizeHotspots,
        TargetMaxLevel,
        TargetMinLevel,
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Mailboxes : object
    {
        [XmlElement("Mailbox", Form = XmlSchemaForm.Unqualified)]
        public MailboxType[] Items { get; set; }
    }


    [Serializable]
    public class MailboxType : object
    {
        public MailboxType()
        {
            Nav = NavType.Run;
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute, DefaultValue(NavType.Run)]
        public NavType Nav { get; set; }

        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }
    }


    [Serializable]
    public enum NavType
    {
        Fly,
        Run,
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ProtectedItems : object
    {
        [XmlElement("Item", Form = XmlSchemaForm.Unqualified)]
        public ItemType[] Items { get; set; }
    }


    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Vendors : object
    {
        [XmlElement("Vendor", Form = XmlSchemaForm.Unqualified)]
        public VendorType[] Items { get; set; }
    }


    [Serializable]
    public class VendorType : object
    {
        public VendorType()
        {
            Nav = NavType.Run;
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute(DataType = "positiveInteger")]
        public string Entry { get; set; }

        [XmlAttribute(DataType = "positiveInteger")]
        public string Entry2 { get; set; }

        [XmlAttribute]
        public VendorTypeType Type { get; set; }

        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }

        [XmlAttribute, DefaultValue(NavType.Run)]
        public NavType Nav { get; set; }
    }


    [Serializable]
    public enum VendorTypeType
    {
        Food,
        Repair,
    }

    [Serializable]
    [XmlType(IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {
        AvoidMobs,
        Blacklist,
        Blackspots,
        ContinentId,
        ForceMail,
        GrindArea,
        Hotspots,
        MailBlue,
        MailGreen,
        MailGrey,
        MailPurple,
        MailWhite,
        Mailboxes,
        MaxLevel,
        MinDurability,
        MinFreeBagSlots,
        MinLevel,
        Name,
        ProtectedItems,
        SellBlue,
        SellGreen,
        SellGrey,
        SellPurple,
        SellWhite,
        TargetElites,
        Vendors,
        Quest,
        QuestOrder
    }

    [Serializable]
    [XmlType(IncludeInSchema = false)]
    public enum ItemsChoiceType4
    {
        Quest,
        PickUp,
        TurnIn,
        CustomBehavior,
        If,
        Objective,
        While,
        MoveTo
    }


    [Serializable]
    public class AvoidmobType : MobType
    {
    }


    [Serializable]
    [XmlType(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {
        AvoidMob,
        Mob,
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class HBProfile : SubProfileType
    {
        [XmlElement("SubProfile", Form = XmlSchemaForm.Unqualified)]
        public SubProfileType[] Items1 { get; set; }
    }
}