﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using nManager.Helpful;
using nManager.Wow.Bot.Tasks;
using nManager.Wow.Class;
using nManager.Wow.Enums;
using nManager.Wow.ObjectManager;
using nManager.Wow.Patchables;
using Timer = nManager.Helpful.Timer;

namespace nManager.Wow.Helpers
{
    public class Usefuls
    {
        private static readonly object Locker = new object();
        private static int _lastContainerNumFreeSlots;
        private static Timer _timerContainerNumFreeSlots = new Timer(0);

        private static int _lastHonorPoint;
        private static readonly Timer TimerHonorPoint = new Timer(1000);

        private static Timer _timePlayerUsingVehicle = new Timer(0);
        private static bool _lastResultPlayerUsingVehicle;

        private static int _lastLatency;
        private static Timer _timerLatency = new Timer(0);
        private static readonly List<int> AchievementsDoneCache = new List<int>();
        private static readonly List<int> AchievementsNotDoneCache = new List<int>();
        private static readonly Object ThisLock = new Object();
        private static readonly Timer AfkTimer = new Timer(500);
        public static string AfkKeyPress;

        public static bool InGame
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadByte(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.gameState) > 0;
                }
                catch (Exception exception)
                {
                    if (exception.ToString() == "Process is not open for read/write.")
                        Thread.Sleep(500);
                    Logging.WriteError("InGame: " + exception);
                    return false;
                }
            }
        }

        public static bool IsLoading
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.isLoading) != 0;
                }
                catch (Exception e)
                {
                    Logging.WriteError("IsLoading: " + e);
                    return false;
                }
            }
        }

        public static UInt64 GetMoneyCopper
        {
            get
            {
                lock (Usefuls.Locker)
                {
                    try
                    {
                        string randomString = Others.GetRandomString(Others.Random(4, 10));
                        Lua.LuaDoString(randomString + " = GetMoney()");
                        return Others.ToUInt64(Lua.GetLocalizedText(randomString));
                    }
                    catch (Exception e)
                    {
                        Logging.WriteError("GetMoneyCopper: " + e);
                        return 0;
                    }
                }
            }
        }

        public static bool IsFlyableArea
        {
            get
            {
                lock (Usefuls.Locker)
                {
                    try
                    {
                        string randomString = Others.GetRandomString(Others.Random(4, 10));
                        Lua.LuaDoString(randomString + " = tostring(IsFlyableArea())");
                        if (Lua.GetLocalizedText(randomString) == "true")
                        {
                            Point pointAbove = ObjectManager.ObjectManager.Me.Position;
                            pointAbove.Z += 30;
                            return !TraceLine.TraceLineGo(pointAbove);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.WriteError("IsFlyableArea: " + e);
                    }
                    return false;
                }
            }
        }

        public static bool IsOutdoors
        {
            get
            {
                lock (Usefuls.Locker)
                {
                    try
                    {
                        string randomString = Others.GetRandomString(Others.Random(4, 10));
                        Lua.LuaDoString(randomString + " = tostring(IsOutdoors())");
                        return Lua.GetLocalizedText(randomString) == "true";
                    }
                    catch (Exception e)
                    {
                        Logging.WriteError("IsOutdoors: " + e);
                        return false;
                    }
                }
            }
        }

        public static int GetContainerNumFreeSlots
        {
            get
            {
                try
                {
                    lock (Usefuls.Locker)
                    {
                        if (!_timerContainerNumFreeSlots.IsReady)
                            return _lastContainerNumFreeSlots;

                        _timerContainerNumFreeSlots = new Timer(1000);
                        string randomString = Others.GetRandomString(Others.Random(4, 10));
                        string result = Lua.LuaDoString(
                            randomString + " = 0; for i = 0, 4 do if GetContainerNumFreeSlots(i) ~= nil then " +
                            randomString + " = " + randomString + " + GetContainerNumFreeSlots(i); end end  ",
                            randomString);
                        if (Regex.IsMatch(result, @"^[0-9]+$"))
                            _lastContainerNumFreeSlots = Others.ToInt32(result);
                        else
                            Logging.WriteError("GetContainerNumFreeSlots failed, \"" + result + "\" returned.");
                        return _lastContainerNumFreeSlots;
                    }

                    /*
                    try
                    {
                        int nbBag = 4;
                        int BACKPACK_SLOT = 16;

                        int freeSlot = 0;

                        for (int b = 0; b <= nbBag - 1; b++)
                        {
                            UInt128 bagGuid = Memory.WowMemory.Memory.ReadUInt64(Process.Process.wowModule + (uint)Addresses.Container.EquippedBagGUID + (uint)(b * 0x8));

                            if (bagGuid > 0)
                            {
                                WoWContainer t = new WoWContainer(ObjectManager.GetObjectByGuid(bagGuid).GetBaseAddress);
                                if (t.GetBaseAddress > 0)
                                {
                                    freeSlot += t.NumberSlot;

                                    for (int s = 1; s <= t.NumberSlot; s++)
                                    {
                                        if (t.GetSlot(s) > 0)
                                            freeSlot--;
                                    }
                                }
                            }
                        }


                        //Backpack 
                        freeSlot += BACKPACK_SLOT;
                        for (int s = 0; s <= BACKPACK_SLOT - 1; s++)
                        {
                            if (ObjectManager.Me.GetDescriptor<UInt128>(Descriptors.PlayerFields.PLAYER_FIELD_PACK_SLOT_1 + (s * 0x8)) > 0)
                                freeSlot--;
                        }

                        return freeSlot;

                    }
                    catch
                    {
                        return 0;
                    }
                     * */
                }
                catch (Exception e)
                {
                    Logging.WriteError("GetContainerNumFreeSlots: " + e);
                    return 50;
                }
            }
        }

        public static int GetHonorPoint
        {
            get
            {
                lock (Usefuls.Locker)
                {
                    try
                    {
                        if (!TimerHonorPoint.IsReady)
                            return _lastHonorPoint;

                        TimerHonorPoint.Reset();

                        string randomString = Others.GetRandomString(Others.Random(4, 10));
                        Lua.LuaDoString("_, " + randomString + " = GetCurrencyInfo(392);");

                        int t;
                        try
                        {
                            string localized = Lua.GetLocalizedText(randomString);
                            if (localized != null)
                                t = Others.ToInt32(localized);
                            else t = -1;
                        }
                        catch
                        {
                            t = -1;
                        }

                        if (t >= -1 || t <= 4000)
                            _lastHonorPoint = t;

                        return _lastHonorPoint;
                    }
                    catch (Exception e)
                    {
                        Logging.WriteError("GetHonorPoint: " + e);
                        return 0;
                    }
                }
            }
        }

        public static bool PlayerUsingVehicle
        {
            get
            {
                lock (Usefuls.Locker)
                {
                    try
                    {
                        if (!_timePlayerUsingVehicle.IsReady)
                            return _lastResultPlayerUsingVehicle;

                        string randomString = Others.GetRandomString(Others.Random(4, 10));
                        Lua.LuaDoString(randomString + " = tostring(UnitUsingVehicle(\"player\"));");
                        _lastResultPlayerUsingVehicle = Lua.GetLocalizedText(randomString) == "true";
                        _timePlayerUsingVehicle = new Timer(500);
                        return _lastResultPlayerUsingVehicle;
                    }
                    catch (Exception e)
                    {
                        Logging.WriteError("PlayerUsingVehicle: " + e);
                        return false;
                    }
                }
            }
        }

        public static int Latency
        {
            get
            {
                try
                {
                    if (!_timerLatency.IsReady)
                        return _lastLatency;

                    _timerLatency = new Timer(30 * 1000);
                    string luaResult = Others.GetRandomString(Others.Random(4, 10));
                    Lua.LuaDoString("_,_,_,worldLag=GetNetStats() " + luaResult + "=worldLag");
                    _lastLatency = Others.ToInt32(Lua.GetLocalizedText(luaResult));
                    /*
                    if (nManager.Settings.CurrentSettings.HackLatency) : Todo: Allow user with really high latency to reduce latency by 50%, 75%.
                        return _lastLatency/2;
                    */
                    return _lastLatency == 0 ? 1 : _lastLatency; // Never return 0 so Thread.Sleep wont freeze thread.
                }
                catch (Exception e)
                {
                    Logging.WriteError("Latency: " + e);
                    return 1;
                }
            }
        }

        public static void TagMonstersArround(Spell spellToUses, float range, int entry)
        {
            TagMonstersArround(spellToUses, range, new List<int> {entry});
        }

        public static void TagMonstersArround(Spell spellToUses, float range, List<int> entry = null)
        {
            Usefuls.SleepGlobalCooldown();
            foreach (WoWUnit u in ObjectManager.ObjectManager.GetObjectWoWUnit60Yards())
            {
                if (!u.IsAlive || u.InCombat || (entry != null && !entry.Contains(u.Entry)) || UnitRelation.GetReaction(ObjectManager.ObjectManager.Me.Faction, u.Faction) <= Reaction.Hostile)
                {
                    continue;
                }
                if (!u.IsInRange(range))
                    continue;
                if (ObjectManager.ObjectManager.Me.Target != u.Guid)
                {
                    Interact.InteractWith(u.GetBaseAddress, true);
                    Thread.Sleep(100);
                    spellToUses.Cast(true);
                    return;
                }
            }
        }

        public static void SleepGlobalCooldown()
        {
            int i = 1;
            while (SpellManager.IsOnGlobalCooldown)
            {
                if (i < 100)
                    Thread.Sleep(300 / i);
                else
                    Thread.Sleep(5);
                i++;
            }
        }

        public static int GetWoWTime
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.GetTime);
                }
                catch (Exception e)
                {
                    Logging.WriteError("GetWoWTime: " + e);
                    return 0;
                }
            }
        }

        public static bool IsFalling
        {
            get { return ObjectManager.ObjectManager.Me.IsFalling; }
        }

        public static bool IsSwimming
        {
            get { return ObjectManager.ObjectManager.Me.IsSwimming; }
        }

        public static bool IsFlying
        {
            get { return ObjectManager.ObjectManager.Me.IsFlying; }
        }

        public static string RealmName
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadUTF8String(Memory.WowMemory.Memory.ReadUInt(Memory.WowProcess.WowModule + (uint) Addresses.Login.realmName) + (uint) Addresses.Login.realmNameOffset);
                }
                catch (Exception e)
                {
                    Logging.WriteError("RealmName: " + e);
                    return "";
                }
            }
        }

        public static string MapZoneName
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadUTF8String(Memory.WowMemory.Memory.ReadUInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.zoneMap));
                }
                catch (Exception e)
                {
                    Logging.WriteError("MapZoneName: " + e);
                    return "";
                }
            }
        }

        public static string SubMapZoneName
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadUTF8String(Memory.WowMemory.Memory.ReadUInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.subZoneMap));
                }
                catch (Exception e)
                {
                    Logging.WriteError("SubMapZoneName: " + e);
                    return "";
                }
            }
        }

        public static string MapName
        {
            get
            {
                try
                {
                    return ContinentNameByContinentId(ContinentId);
                }
                catch (Exception e)
                {
                    Logging.WriteError("MapName: " + e);
                    return "Azeroth";
                }
            }
        }

        public static int ContinentIdByContinentName(string name)
        {
            switch (name)
            {
                case "Outland":
                    return 530;
                case "Maelstrom":
                    return 646;
                case "Pandaria":
                    return 870;
                case "AllianceGunship":
                    return 123456;
                default:
                    WoWMap map = WoWMap.FromMPQName(name);
                    return (int) map.Record.Id;
            }
        }

        public static string ContinentNameByContinentId(int cId)
        {
            switch (cId)
            {
                case 530: // Expansion01 => Outland
                    return "Outland";
                case 646: // Deephome => Maelstrom
                    return "Maelstrom";
                case 870: // HawaiiMainLand => Pandaria
                    return "Pandaria";
                case 123456:
                    return "AllianceGunship";
                default:
                    WoWMap map = WoWMap.FromId(cId);
                    return map.MapMPQName;
            }
        }

        public static string ContinentNameMpqByContinentId(int cId)
        {
            if (cId == 123456)
                return "AllianceGunship";
            WoWMap map = WoWMap.FromId(cId);
            return map.MapMPQName;
        }

        public static string ContinentNameMpq
        {
            get
            {
                try
                {
                    return ContinentNameMpqByContinentId(ContinentId);
                }
                catch (Exception e)
                {
                    Logging.WriteError("ContinentNameMpq: " + e);
                    return "Azeroth";
                }
            }
        }

        public static int ContinentId
        {
            get
            {
                if (_continentId == 1116 || _continentId == 0 || _continentId == 1 || _continentId == 530 || _continentId == 870)
                    return _continentId;
                if (_continentId == 646)
                {
                    var go = ObjectManager.ObjectManager.GetObjectByGuid(ObjectManager.ObjectManager.Me.TransportGuid) as WoWGameObject;
                    return (go != null && go.IsValid && go.Entry == 204018) ? 123456 : 646;
                }
                return Garrison.GarrisonMapIdList.Contains(_continentId) ? 1116 : _continentId;
            }
            set { _continentId = value; }
        }

        public static int RealContinentId
        {
            get { return _continentId; }
        }

        public static List<WoWMap.MapDbcRecord> GetInstancesList()
        {
            List<WoWMap.MapDbcRecord> result = WoWMap.WoWMaps(WoWMap.InstanceType.Party, WoWMap.MapType.ADTType);
            result.AddRange(WoWMap.WoWMaps(WoWMap.InstanceType.Party, WoWMap.MapType.WDTOnlyType));
            return result;
        }

        public static List<WoWMap.MapDbcRecord> GetRaidsList()
        {
            List<WoWMap.MapDbcRecord> result = WoWMap.WoWMaps(WoWMap.InstanceType.Raid, WoWMap.MapType.ADTType);
            result.AddRange(WoWMap.WoWMaps(WoWMap.InstanceType.Raid, WoWMap.MapType.WDTOnlyType));
            return result;
        }

        public static List<WoWMap.MapDbcRecord> GetBattlegroundsList()
        {
            List<WoWMap.MapDbcRecord> result = WoWMap.WoWMaps(WoWMap.InstanceType.Battleground, WoWMap.MapType.ADTType);
            result.AddRange(WoWMap.WoWMaps(WoWMap.InstanceType.Battleground, WoWMap.MapType.WDTOnlyType));
            return result;
        }

        public static List<uint> CachedRaidList;

        public static bool IsInRaid
        {
            get
            {
                if (CachedRaidList == null)
                {
                    CachedRaidList = new List<uint>();
                    var raidList = GetRaidsList();
                    for (int i = 0; i < raidList.Count; i++)
                    {
                        CachedRaidList.Add(raidList[i].Id);
                    }
                }
                return CachedRaidList.Contains((uint) ContinentId);
            }
        }

        public static List<uint> CachedInstanceList;

        public static bool IsInDungeon
        {
            get
            {
                if (CachedInstanceList == null)
                {
                    CachedInstanceList = new List<uint>();
                    var instanceList = GetInstancesList();
                    for (int i = 0; i < instanceList.Count; i++)
                    {
                        CachedInstanceList.Add(instanceList[i].Id);
                    }
                }
                return CachedInstanceList.Contains((uint) ContinentId);
            }
        }

        public static List<uint> CachedBattlegroundList;

        public static bool IsInBattleground
        {
            get
            {
                if (CachedBattlegroundList == null)
                {
                    CachedBattlegroundList = new List<uint>();
                    var battlegroundList = GetBattlegroundsList();
                    for (int i = 0; i < battlegroundList.Count; i++)
                    {
                        CachedBattlegroundList.Add(battlegroundList[i].Id);
                    }
                }
                return CachedBattlegroundList.Contains((uint) ContinentId);
            }
        }

        private static int _continentId;

        public static int SubAreaId
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.SubAreaId);
                }
                catch (Exception e)
                {
                    Logging.WriteError("SubAreaId: " + e);
                    return 0;
                }
            }
        }

        public static int AreaId
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.AreaId);
                }
                catch (Exception e)
                {
                    Logging.WriteError("AreaId: " + e);
                    return 0;
                }
            }
        }

        public static int HearthstoneSubAreaId
        {
            get
            {
                try
                {
                    return Memory.WowMemory.Memory.ReadInt(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.HearthstoneSubAreaId);
                }
                catch (Exception e)
                {
                    Logging.WriteError("HearthstoneSubAreaId: " + e);
                    return 0;
                }
            }
        }

        public static uint WowVersion(string textBuild = "")
        {
            try
            {
                if (textBuild == "")
                    textBuild = Memory.WowMemory.Memory.ReadUTF8String(Memory.WowProcess.WowModule + (uint)Addresses.GameInfo.buildWoWVersionString);
                return Others.ToUInt32(textBuild);
                // "World of WarCraft: Retail Build (build 18966)"
                if (textBuild == "")
                    textBuild = Memory.WowMemory.Memory.ReadUTF8String(Memory.WowProcess.WowModule + (uint) Addresses.GameInfo.buildWoWVersionString);
                if (!textBuild.Contains(' '))
                    return 0;
                string[] textBuildStrings = textBuild.Split(' ');
                string wowBuildString = textBuildStrings[textBuildStrings.Length - 1].Remove(5);
                uint wowBuild = Others.ToUInt32(wowBuildString);
                return wowBuild;
            }
            catch (Exception e)
            {
                Logging.WriteError("WowVersion: " + e);
                return 0;
            }
        }

        /// <summary>
        ///     Make key registre Wow.
        /// </summary>
        /// <returns></returns>
        public static void MakeWowRegistry()
        {
            try
            {
                MessageBox.Show(String.Format("{0}.", Translate.Get(Translate.Id.Please_select_exe_in_the_install_folder_of_the_game)));
                string path = Others.DialogBoxOpenFile("", "Profile files (Wow.exe)|Wow.exe");
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft") ??
                                  Registry.LocalMachine.CreateSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft");
                if (key != null) key.SetValue("InstallPath", path.Replace("Wow.exe", ""), RegistryValueKind.String);
            }
            catch (Exception e)
            {
                Logging.WriteError("MakeWowRegistry(): " + e);
            }
        }

        public static void DisableFIPS()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa\\FipsAlgorithmPolicy", true))
            {
                if (key != null)
                {
                    int val = (int) key.GetValue("Enabled", null, RegistryValueOptions.None);
                    if (val != 0)
                    {
                        Logging.Write("We had to disable FIPS for you, else you wouldn't be able to run the bot properly, learn more about FIPS here: https://en.wikipedia.org/wiki/FIPS_140-2");
                        key.SetValue("Enabled", 0, RegistryValueKind.DWord);
                    }
                }
            }
            // Disable FIPS policy.
            /*
             * The Federal Information Processing Standard (FIPS) Publication 140-2, (FIPS PUB 140-2), 
             * is a U.S. government computer security standard used to accredit cryptographic modules. 
             * The title is Security Requirements for Cryptographic Modules.
             * It therefor prevent any software from running non-accreditted cryptographic modules such as MD5.
             */
            // Only computer of the US Governement should have FIPS enabled.
        }


        /// <summary>
        ///     Launch World Of Warcraft.
        /// </summary>
        /// <typeparam></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int LaunchWow(string param = "")
        {
            try
            {
                RegistryKey registre = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Blizzard Entertainment\World of Warcraft");
                if (registre == null)
                {
                    MakeWowRegistry();
                    LaunchWow();
                    return 0;
                }
                object val = registre.GetValue("InstallPath");
                if (val == null)
                {
                    MakeWowRegistry();
                    LaunchWow();
                    return 0;
                }
                if (!File.Exists(val + "Wow.exe"))
                {
                    MakeWowRegistry();
                    LaunchWow();
                    return 0;
                }
                var proc = new Process {StartInfo = {FileName = val + "Wow.exe", Arguments = param}};
                proc.Start();
                return proc.Id;
            }
            catch (Exception e)
            {
                Logging.WriteError("LaunchWow(): " + e);
            }
            return 0;
        }

        public static int ContainerNumFreeSlots(BagType bagType)
        {
            int unspecified = 0;
            int quiver = 0;
            int ammoPouch = 0;
            int soulBag = 0;
            int leatherworkingBag = 0;
            int inscriptionBag = 0;
            int herbBag = 0;
            int enchantingBag = 0;
            int engineeringBag = 0;
            int keyring = 0;
            int gemBag = 0;
            int miningBag = 0;
            int unknown = 0;
            int vanityPets = 0;
            int lureBag = 0;
            string bag0FreeSlots = Others.GetRandomString(Others.Random(4, 10));
            string bag0Type = Others.GetRandomString(Others.Random(4, 10));
            string bag1FreeSlots = Others.GetRandomString(Others.Random(4, 10));
            string bag1Type = Others.GetRandomString(Others.Random(4, 10));
            string bag2FreeSlots = Others.GetRandomString(Others.Random(4, 10));
            string bag2Type = Others.GetRandomString(Others.Random(4, 10));
            string bag3FreeSlots = Others.GetRandomString(Others.Random(4, 10));
            string bag3Type = Others.GetRandomString(Others.Random(4, 10));
            string bag4FreeSlots = Others.GetRandomString(Others.Random(4, 10));
            string bag4Type = Others.GetRandomString(Others.Random(4, 10));
            string randomString = Others.GetRandomString(Others.Random(4, 10));
            Lua.LuaDoString(bag0FreeSlots + "," + bag0Type + " = GetContainerNumFreeSlots(0); " +
                            bag1FreeSlots + "," + bag1Type + " = GetContainerNumFreeSlots(1); " +
                            bag2FreeSlots + "," + bag2Type + " = GetContainerNumFreeSlots(2); " +
                            bag3FreeSlots + "," + bag3Type + " = GetContainerNumFreeSlots(3); " +
                            bag4FreeSlots + "," + bag4Type + " = GetContainerNumFreeSlots(4); " +
                            "if(" + bag1Type + " == nil) then " + bag1Type + " = 16777216 end " +
                            "if(" + bag2Type + " == nil) then " + bag2Type + " = 16777216 end " +
                            "if(" + bag3Type + " == nil) then " + bag3Type + " = 16777216 end " +
                            "if(" + bag4Type + " == nil) then " + bag4Type + " = 16777216 end " +
                            randomString + " = " + bag0FreeSlots + " .. \",\" .. " + bag0Type + " .. \";\" .. " + bag1FreeSlots + " .. \",\" .. " + bag1Type + " .. \";\" .. " +
                            bag2FreeSlots + " .. \",\" .. " + bag2Type + " .. \";\" .. " + bag3FreeSlots + " .. \",\" .. " + bag3Type + " .. \";\" .. " + bag4FreeSlots +
                            " .. \",\" .. " + bag4Type);
            string result = Lua.GetLocalizedText(randomString);
            if (!String.IsNullOrEmpty(result) && result.Contains(";"))
            {
                string[][] bags = result.Split(';').Select(s => s.Split(',')).ToArray();
                foreach (var t in bags)
                {
                    if (t.Count() <= 1)
                        continue;
                    if (t[0] == "0")
                        continue;
                    int currBagFreeSlots = Others.ToInt32(t[0]);
                    var currBagType = (BagType) Others.ToInt32(t[1]);
                    if (currBagType == BagType.Unspecified)
                    {
                        unspecified += currBagFreeSlots;
                        continue;
                    }
                    if (currBagType.HasFlag(BagType.None) || !currBagType.HasFlag(bagType))
                        continue;
                    switch (bagType)
                    {
                        case BagType.MiningBag:
                            miningBag += currBagFreeSlots;
                            break;
                        case BagType.HerbBag:
                            herbBag += currBagFreeSlots;
                            break;
                        case BagType.LeatherworkingBag:
                            leatherworkingBag += currBagFreeSlots;
                            break;
                        case BagType.GemBag:
                            gemBag += currBagFreeSlots;
                            break;
                        case BagType.EnchantingBag:
                            enchantingBag += currBagFreeSlots;
                            break;
                        case BagType.InscriptionBag:
                            inscriptionBag += currBagFreeSlots;
                            break;
                        case BagType.LureBag:
                            lureBag += currBagFreeSlots;
                            break;
                        case BagType.SoulBag:
                            soulBag += currBagFreeSlots;
                            break;
                        case BagType.VanityPets:
                            vanityPets += currBagFreeSlots;
                            break;
                        case BagType.Unknown:
                            unknown += currBagFreeSlots;
                            break;
                        case BagType.AmmoPouch:
                            ammoPouch += currBagFreeSlots;
                            break;
                        case BagType.Keyring:
                            keyring += currBagFreeSlots;
                            break;
                        case BagType.Quiver:
                            quiver += currBagFreeSlots;
                            break;
                        case BagType.EngineeringBag:
                            engineeringBag += currBagFreeSlots;
                            break;
                    }
                }
                switch (bagType)
                {
                    case BagType.Unspecified:
                        return unspecified;
                    case BagType.MiningBag:
                        return miningBag + unspecified;
                    case BagType.HerbBag:
                        return herbBag + unspecified;
                    case BagType.LeatherworkingBag:
                        return leatherworkingBag + unspecified;
                    case BagType.GemBag:
                        return gemBag + unspecified;
                    case BagType.EnchantingBag:
                        return enchantingBag + unspecified;
                    case BagType.InscriptionBag:
                        return inscriptionBag + unspecified;
                    case BagType.LureBag:
                        return lureBag + unspecified;
                    case BagType.SoulBag:
                        return soulBag + unspecified;
                    case BagType.VanityPets:
                        return vanityPets + unspecified;
                    case BagType.Unknown:
                        return unknown + unspecified;
                    case BagType.AmmoPouch:
                        return ammoPouch + unspecified;
                    case BagType.Keyring:
                        return keyring + unspecified;
                    case BagType.Quiver:
                        return quiver + unspecified;
                    case BagType.EngineeringBag:
                        return engineeringBag + unspecified;
                }
            }
            return 0;
        }

        public static void DisMount()
        {
            try
            {
                MountTask.DismountTimer.Reset();
                Lua.RunMacroText(ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.DruidMountId()) ? "/cancelform" : "/dismount");
            }
            catch (Exception e)
            {
                Logging.WriteError("DisMount(): " + e);
            }
        }

        public static void EjectVehicle()
        {
            try
            {
                Lua.LuaDoString("VehicleExit();");
            }
            catch (Exception e)
            {
                Logging.WriteError("EjectVehicle(): " + e);
            }
        }

        public static void OpenAllBags()
        {
            try
            {
                Lua.LuaDoString("OpenAllBags();");
            }
            catch (Exception e)
            {
                Logging.WriteError("OpenAllBags(): " + e);
            }
        }

        public static void CloseAllBags()
        {
            try
            {
                Lua.LuaDoString("CloseAllBags();");
            }
            catch (Exception e)
            {
                Logging.WriteError("CloseAllBags(): " + e);
            }
        }

        public static string GetPlayerName(UInt128 guid)
        {
            try
            {
                return GetName.GetPlayerName.GetPlayerNameByGuid(guid);
                uint next = Memory.WowMemory.Memory.ReadUInt(Memory.WowProcess.WowModule + (uint) Addresses.PlayerNameStore.PlayerNameStorePtr + (uint) Addresses.PlayerNameStore.PlayerNameNextOffset);
                uint ptr = next;
                while (true)
                {
                    uint ncstart = ptr + 16;
                    if (Memory.WowMemory.Memory.ReadUInt128(ncstart) == guid)
                        return Memory.WowMemory.Memory.ReadUTF8String(ncstart + (uint) Addresses.PlayerNameStore.PlayerNameStringOffset);
                    ptr = Memory.WowMemory.Memory.ReadUInt(ptr);
                    if (ptr == 0)
                        break;
                    if (ptr == next)
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("GetPlayerName(UInt128 guid): " + e);
            }
            return "Unknow";
        }

        public static void ResetAllInstances()
        {
            Lua.LuaDoString("ResetInstances()");
            // Can only reset an instance up to 10 times per hours.
        }

        public static Reaction GetReputationReaction(int reputationId)
        {
            //_, _, standingID, _, _, _, _, _, _, _, _, _, _, _, _, _= GetFactionInfoByID(970);
            string randomString = Others.GetRandomString(Others.Random(4, 10));
            string standing = Lua.LuaDoString("_, _," + randomString + ", _, _, _, _, _, _, _, _, _, _, _, _, _= GetFactionInfoByID(" + reputationId + ")", randomString);

            return (Reaction) Others.ToInt32(standing);
        }

        public static bool IsCompletedAchievement(int achievementId, bool meOnly = false)
        {
            if (achievementId <= 0)
                return true;
            if (AchievementsDoneCache.Contains(achievementId))
                return true;
            if (AchievementsNotDoneCache.Contains(achievementId))
                return false;
            string randomString = Others.GetRandomString(Others.Random(4, 10));
            string randomString2 = Others.GetRandomString(Others.Random(4, 10));
            string randomString3 = Others.GetRandomString(Others.Random(4, 10));
            string query;
            if (meOnly)
                query = "_, _, _, " + randomString2 + ", _, _, _, _, _, _, _, _, " + randomString3 + " = GetAchievementInfo(" + achievementId + "); if " + randomString2 + " and " + randomString3 + " then " +
                        randomString + "=\"1\" else " + randomString + "=\"0\" end;";
            else
                query = "_, _, _, " + randomString2 + " = GetAchievementInfo(" + achievementId + "); if " + randomString2 + " then " + randomString + "=\"1\" else " + randomString + "=\"0\" end;";
            Lua.LuaDoString(query);
            string ret = Lua.GetLocalizedText(randomString);
            if (ret == "1")
            {
                AchievementsDoneCache.Add(achievementId);
                return true;
            }
            AchievementsNotDoneCache.Add(achievementId);
            return false;
        }

        private static bool _lastHardwareActionDone = true;

        public static void UpdateLastHardwareAction()
        {
            if (Memory.WowMemory.IsGameFrameLocked || !_lastHardwareActionDone || !InGame || IsLoading)
                return;
            _lastHardwareActionDone = false;
            // The below Memory Write update LastHardwareAction with the current WoW.GetTime().
            // However, it wont get you out of AFK Status if you was already AFK upon starting TheNoobBot.
            // To remove the AFK Status, you need to call CGGameUI__UpdatePlayerAFK right after updating LastHardwareAction.
            // Memory.WowMemory.Memory.WriteUInt(Memory.WowProcess.WowModule + (uint)Addresses.GameInfo.LastHardwareAction, Memory.WowProcess.WowModule + (uint)Addresses.GameInfo.GetTime);

            // The below code use LUA to get a key that is not binded in World of Warcraft.
            // It will then press it and let WoW handle the "LastHardwareAction + UpdatePlayerAFK" task at once.
            if (String.IsNullOrEmpty(AfkKeyPress) || AfkKeyPress == "B")
            {
                Thread.Sleep(10);
                AfkKeyPress = Keybindings.GetAFreeKey(true);
                AfkTimer.Reset();
            }
            if (!AfkTimer.IsReady)
            {
                _lastHardwareActionDone = true;
                return;
            }
            Keyboard.DownKey(Memory.WowProcess.MainWindowHandle, AfkKeyPress);
            Thread.Sleep(10);
            Keyboard.UpKey(Memory.WowProcess.MainWindowHandle, AfkKeyPress);
            AfkTimer.Reset();
            _lastHardwareActionDone = true;
        }

        public class WoWCurrency
        {
            public int Entry;
            public string Name;
            public int CurrentAmount;
            public int FileId;
            public int EarnedThisWeek;
            public int WeeklyMax;
            public int TotalMax;
            public bool IsDiscovered;

            public bool IsValid
            {
                get { return Name != ""; }
            }
        }

        public static WoWCurrency GetCurrencyInfo(int entry)
        {
            string randomString = Others.GetRandomString(Others.Random(5, 10));
            string result = Lua.LuaDoString(
                randomString + " = \"\"; " +
                "local name, currentAmount, texture, earnedThisWeek, weeklyMax, totalMax, isDiscovered = GetCurrencyInfo(" + entry + "); " +
                randomString +
                " = tostring(name) .. \"##\" .. tostring(currentAmount) .. \"##\" .. tostring(texture) .. \"##\" .. tostring(earnedThisWeek)  .. \"##\" .. tostring(weeklyMax)  .. \"##\" .. tostring(totalMax)  .. \"##\" .. tostring(isDiscovered);"
                , randomString);
            if (!string.IsNullOrWhiteSpace(result))
            {
                string[] ar = {"##"};
                string[] slipped = result.Split(ar, StringSplitOptions.None);
                if (slipped.Length == 6)
                {
                    var tmp = new WoWCurrency
                    {
                        Entry = entry,
                        Name = slipped[0],
                        CurrentAmount = Others.ToInt32(slipped[1]),
                        FileId = Others.ToInt32(slipped[2]),
                        EarnedThisWeek = Others.ToInt32(slipped[3]),
                        WeeklyMax = Others.ToInt32(slipped[4]),
                        TotalMax = Others.ToInt32(slipped[5]),
                        IsDiscovered = Others.ToBoolean(slipped[6])
                    };
                    return tmp;
                }
            }
            return new WoWCurrency();
        }

        public static string GetClientLanguage()
        {
            string randomString = Others.GetRandomString(Others.Random(5, 10));
            string result = Lua.LuaDoString(randomString + " =  GetLocale();", randomString);

            return result;
        }

        public static bool IsInPetBattle
        {
            get { return Memory.WowMemory.Memory.ReadByte(Memory.WowProcess.WowModule + (uint) Addresses.PetBattle.IsInBattle) == 1; }
        }

        public static Point GetSafeResPoint()
        {
            Single degree = 0;
            Point posCorpse = ObjectManager.ObjectManager.Me.PositionCorpse;
            Point bestPoint = new Point();
            Point rezPos = new Point(0, 0, 0);

            while (degree < 360) //Search for safe rez point, if no safe point found, just rez and get killed again!
            {
                //Calculate position on a circle 15degrees at a time and check if we can go there
                float x = (float) (posCorpse.X + 30f * System.Math.Cos(Helpful.Math.DegreeToRadian(degree)));
                float y = (float) (posCorpse.Y + 30f * System.Math.Sin(Helpful.Math.DegreeToRadian(degree)));
                rezPos = new Point(x, y, PathFinder.GetZPosition(x, y));

                if (ObjectManager.ObjectManager.Me.PositionCorpse.DistanceTo(rezPos) < 36.0f && !nManagerSetting.IsBlackListedZone(rezPos))
                {
                    bool validPoint;
                    List<Point> path = PathFinder.FindPath(rezPos, out validPoint); //Valid Point?

                    if (validPoint && Helpful.Math.DistanceListPoint(path) <= 40f) //Point Valid : keep only the point that is the farthest from the mobs
                    {
                        WoWUnit mobc = ObjectManager.ObjectManager.GetNearestWoWUnit(ObjectManager.ObjectManager.GetWoWUnitHostile(), rezPos); //Closest mob from current point
                        WoWUnit mobb = ObjectManager.ObjectManager.GetNearestWoWUnit(ObjectManager.ObjectManager.GetWoWUnitHostile(), bestPoint); //Closest mob from best point

                        if (!bestPoint.IsValid)
                            bestPoint = rezPos; //First iterration

                        if (bestPoint.DistanceTo(mobb.Position) < rezPos.DistanceTo(mobc.Position))
                        {
                            bestPoint = rezPos;
                        }
                    }
                }

                degree += 15f;
            }

            return bestPoint;
        }

        public static bool BadBottingConditions
        {
            get { return !Products.Products.IsStarted || !InGame || IsLoading || !ObjectManager.ObjectManager.Me.IsValid || ObjectManager.ObjectManager.Me.IsDeadMe; }
        }

        public static bool ShouldFight
        {
            get { return ObjectManager.ObjectManager.Me.InInevitableCombat; }
        }
    }
}