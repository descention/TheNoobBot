﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using nManager.Helpful;
using nManager.Wow.Class;
using nManager.Wow.Enums;
using nManager.Wow.Helpers;
using nManager.Wow.ObjectManager;
using Math = nManager.Helpful.Math;

namespace nManager.Wow.Bot.Tasks
{
    public static class FarmingTask
    {
        private static UInt128 _lastnode;
        private static bool _wasLooted;
        public static bool CountThisLoot;
        public static bool NodeOrUnit; // true = node / false = unit

        public static void Pulse(IEnumerable<WoWGameObject> nodes, bool ignoreCanOpen = false)
        {
            try
            {
                if (Usefuls.IsFlying || Usefuls.IsSwimming)
                    Fly(nodes, ignoreCanOpen);
                else
                    Ground(nodes, ignoreCanOpen);
            }
            catch (Exception ex)
            {
                Logging.WriteError("FarmingTask > Pulse(IEnumerable<WoWGameObject> nodes): " + ex);
            }
        }

        private static void Fly(IEnumerable<WoWGameObject> nodes, bool ignoreCanOpen = false)
        {
            try
            {
                nodes = nodes.OrderBy(x => x.GetDistance);
                foreach (WoWGameObject node in nodes.Where(node => node.IsValid))
                {
                    WoWGameObject inode = node;
                    if (_curNode != null && _curNode.IsValid && !nManagerSetting.IsBlackListed(_curNode.Guid))
                        inode = _curNode;
                    if (!inode.IsValid)
                    {
                        MovementManager.StopMove();
                        nManagerSetting.AddBlackList(inode.Guid, 2 * 60 * 1000);
                        Logging.Write("Current inode not valid, blacklist.");
                        continue;
                    }
                    _curNode = inode; // we save a inode we potentially bypassed to make sure we run the list.
                    if (!inode.CanOpen && !ignoreCanOpen)
                    {
                        nManagerSetting.AddBlackList(inode.Guid, 5000);
                        return;
                    }
                    float zT;
                    if (ObjectManager.ObjectManager.Me.Position.Z < inode.Position.Z)
                        zT = inode.Position.Z + 5.5f;
                    else
                        zT = inode.Position.Z + 2.5f;

                    Point aboveNode = new Point(inode.Position);
                    aboveNode.Z = aboveNode.Z + 2.5f;
                    Point farAboveNode = new Point(aboveNode);
                    farAboveNode.Z = farAboveNode.Z + 50;
                    if (TraceLine.TraceLineGo(farAboveNode, aboveNode, CGWorldFrameHitFlags.HitTestAllButLiquid))
                    {
                        MovementManager.StopMove();
                        if (TraceLine.TraceLineGo(ObjectManager.ObjectManager.Me.Position, aboveNode, CGWorldFrameHitFlags.HitTestAllButLiquid))
                        {
                            Logging.Write("Node stuck");
                            nManagerSetting.AddBlackList(inode.Guid, 1000 * 60 * 2);
                            return;
                        }
                    }
                    else
                        MovementManager.StopMove();
                    if (_lastnode != inode.Guid)
                    {
                        _lastnode = inode.Guid;
                        Logging.Write("Farm " + inode.Name + " (" + inode.Entry + ") > " + inode.Position);
                    }
                    MovementManager.MoveTo(inode.Position.X, inode.Position.Y, zT, true);

                    Helpful.Timer timer = new Helpful.Timer((int) (ObjectManager.ObjectManager.Me.Position.DistanceTo(inode.Position) / 3 * 1000) + 5000);
                    bool toMine = false;
                    bool landing = false;

                    while (inode.IsValid && !Usefuls.BadBottingConditions && !Usefuls.ShouldFight && !timer.IsReady)
                    {
                        if (!landing)
                        {
                            bool noDirectPath = TraceLine.TraceLineGo(aboveNode, ObjectManager.ObjectManager.Me.Position, CGWorldFrameHitFlags.HitTestAllButLiquid);
                            zT = noDirectPath ? ObjectManager.ObjectManager.Me.Position.Z : aboveNode.Z;

                            if (ObjectManager.ObjectManager.Me.Position.Z < aboveNode.Z)
                            {
                                // elevate in a 45° angle instead of 90°
                                Point direction = Math.GetPosition2DOfLineByDistance(ObjectManager.ObjectManager.Me.Position,
                                    inode.Position,
                                    (inode.Position.Z + 2.5f) - ObjectManager.ObjectManager.Me.Position.Z);
                                // if there is an obstacle, then go mostly vertical but not 90° to prevent spinning around
                                if (TraceLine.TraceLineGo(ObjectManager.ObjectManager.Me.Position,
                                    direction,
                                    CGWorldFrameHitFlags.HitTestAllButLiquid))
                                    direction = Math.GetPosition2DOfLineByDistance(ObjectManager.ObjectManager.Me.Position,
                                        inode.Position, 1.0f);
                                MovementManager.MoveTo(direction.X, direction.Y, inode.Position.Z + 5.0f);
                            }
                            else
                            {
                                MovementManager.MoveTo(inode.Position.X, inode.Position.Y, zT);
                            }

                            if (!ObjectManager.ObjectManager.Me.IsMounted)
                                return;
                            if (!noDirectPath)
                                landing = true;
                        }

                        if (ObjectManager.ObjectManager.Me.Position.DistanceTo2D(inode.Position) < 4.0f &&
                            ObjectManager.ObjectManager.Me.Position.DistanceZ(inode.Position) >= 5.0f && !toMine)
                        {
                            toMine = true;

                            if (!ObjectManager.ObjectManager.Me.IsMounted)
                                return;
                            zT = inode.Position.Z + 1.5f;
                            MovementManager.MoveTo(inode.Position.X, inode.Position.Y, zT);
                            if (inode.GetDistance > 3.0f && TraceLine.TraceLineGo(ObjectManager.ObjectManager.Me.Position, inode.Position, CGWorldFrameHitFlags.HitTestAllButLiquid))
                            {
                                Logging.Write("Node outside view");
                                nManagerSetting.AddBlackList(inode.Guid, 1000 * 120);
                                break;
                            }
                        }
                        else if ((ObjectManager.ObjectManager.Me.Position.DistanceTo2D(inode.Position) < 1.1f ||
                                  (!Usefuls.IsFlying &&
                                   ObjectManager.ObjectManager.Me.Position.DistanceTo2D(inode.Position) < 3.0f)) &&
                                 ObjectManager.ObjectManager.Me.Position.DistanceZ(inode.Position) < 6)
                        {
                            Thread.Sleep(150);
                            MovementManager.StopMove();
                            if (Usefuls.IsFlying)
                            {
                                MountTask.Land();
                            }
                            if (ObjectManager.ObjectManager.Me.GetMove)
                            {
                                MovementManager.StopMove();
                            }
                            while (ObjectManager.ObjectManager.Me.GetMove)
                            {
                                Thread.Sleep(50);
                            }
                            if (!ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.AllInteractMountId()) &&
                                (!inode.IsHerb || inode.IsHerb && !ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.HerbsInteractMountId())))
                            {
                                if (!(SpellManager.HasSpell(169606) && Usefuls.ContinentId == 1116 || Usefuls.ContinentId == 1464)) // Passive Silver Dollar Club given by Stables.
                                    MountTask.DismountMount();
                            }
                            else if (inode.IsHerb && ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.DruidMountId()))
                            {
                                Logging.WriteDebug("Druid IsFlying ? " + Usefuls.IsFlying);
                                if (Usefuls.IsFlying)
                                {
                                    MountTask.Land();
                                    MovementManager.StopMove();
                                    if (Usefuls.IsFlying)
                                    {
                                        Logging.Write("You are still flying after two attemps of Landing.");
                                        Logging.Write("Make sure you have binded the action \"SitOrStand\" on a keyboard key and not any mouse button or special button.");
                                        Logging.Write("If you still have this message, please try a \"Reset Keybindings\" before posting on the forum.");
                                        Logging.Write("A work arround have been used, it may let you actually farm or not. Because it's random, please fix your keybinding issue.");
                                        MountTask.Land(true);
                                    }
                                }
                            }

                            Thread.Sleep(Usefuls.Latency + 200);
                            if (ObjectManager.ObjectManager.Me.InInevitableCombat)
                            {
                                MountTask.DismountMount();
                                return;
                            }
                            _wasLooted = false;
                            CountThisLoot = true;
                            NodeOrUnit = true;
                            Interact.InteractWith(inode.GetBaseAddress);
                            Thread.Sleep(Usefuls.Latency + 500);
                            if (!ObjectManager.ObjectManager.Me.IsCast)
                            {
                                Interact.InteractWith(inode.GetBaseAddress);
                                Thread.Sleep(Usefuls.Latency + 500);
                            }
                            while (ObjectManager.ObjectManager.Me.IsCast)
                            {
                                Thread.Sleep(100);
                            }
                            if (ObjectManager.ObjectManager.Me.InInevitableCombat)
                            {
                                MountTask.DismountMount();
                                CountThisLoot = false;
                                return;
                            }
                            Thread.Sleep(Usefuls.Latency + 100);
                            if (ObjectManager.ObjectManager.Me.InInevitableCombat)
                            {
                                MountTask.DismountMount();
                                CountThisLoot = false;
                                return;
                            }
                            nManagerSetting.AddBlackList(inode.Guid, 1000 * 20);
                            return;
                        }
                        else if (!ObjectManager.ObjectManager.Me.GetMove)
                        {
                            Thread.Sleep(50);
                            if (!ObjectManager.ObjectManager.Me.IsMounted)
                                return;
                            MovementManager.MoveTo(inode.Position.X, inode.Position.Y, zT);
                        }
                        if (States.Farming.PlayerNearest(inode))
                        {
                            Logging.Write("Player near the inode, farm canceled");
                            nManagerSetting.AddBlackList(inode.Guid, 15 * 1000);
                            return;
                        }
                    }
                    if (timer.IsReady)
                        nManagerSetting.AddBlackList(inode.Guid, 60 * 1000);
                    MovementManager.StopMove();
                    if (!_wasLooted)
                        Logging.Write("Farm failed #1");
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("FarmingTask > Fly(IEnumerable<WoWGameObject> nodes): " + ex);
            }
        }

        private static WoWGameObject _curNode;
        public static WoWUnit CurUnit;

        private static void Ground(IEnumerable<WoWGameObject> nodes, bool ignoreCanOpen = false)
        {
            try
            {
                nodes = nodes.OrderBy(x => x.GetDistance);
                foreach (WoWGameObject node in nodes)
                {
                    WoWGameObject inode = node;
                    if (_curNode != null && _curNode.IsValid && !nManagerSetting.IsBlackListed(_curNode.Guid))
                        inode = _curNode;
                    if (!inode.IsValid)
                    {
                        MovementManager.StopMove();
                        nManagerSetting.AddBlackList(inode.Guid, 2 * 60 * 1000);
                        Logging.Write("Current inode not valid, blacklist.");
                        continue;
                    }
                    _curNode = inode; // we save a inode we potentially bypassed to make sure we run the list.
                    if (!inode.CanOpen && !ignoreCanOpen)
                    {
                        nManagerSetting.AddBlackList(inode.Guid, 5000);
                        return;
                    }
                    if (ObjectManager.ObjectManager.Me.Position.DistanceTo(inode.Position) > 5.0f)
                    {
                        if (ObjectManager.ObjectManager.Me.Position.DistanceTo(inode.Position) >=
                            nManagerSetting.CurrentSetting.MinimumDistanceToUseMount ||
                            !nManagerSetting.CurrentSetting.UseGroundMount)
                        {
                            if (MountTask.GetMountCapacity() == MountCapacity.Fly || Usefuls.IsFlying)
                            {
                                if (!Usefuls.IsFlying)
                                {
                                    if (!MountTask.OnFlyMount())
                                        MountTask.Mount(true, true);
                                    else
                                        MountTask.Takeoff();
                                }
                                Fly(nodes);
                                return;
                            }
                            if (Usefuls.IsSwimming)
                            {
                                if (MountTask.GetMountCapacity() == MountCapacity.Swimm && !MountTask.OnAquaticMount())
                                    MountTask.Mount();
                                Fly(nodes);
                                return;
                            }
                        }
                        // fallback to ground mount or feet
                        if (ObjectManager.ObjectManager.Me.Position.DistanceTo(inode.Position) >=
                            nManagerSetting.CurrentSetting.MinimumDistanceToUseMount &&
                            nManagerSetting.CurrentSetting.UseGroundMount)
                        {
                            if (MountTask.GetMountCapacity() == MountCapacity.Ground && !MountTask.OnGroundMount())
                                MountTask.Mount();
                        }
                        if (MovementManager.FindTarget(inode, 5.5f, true, nManagerSetting.CurrentSetting.GatheringSearchRadius * 4.0f) == 0)
                        {
                            nManagerSetting.AddBlackList(inode.Guid, 1000 * 20);
                            _curNode = null;
                            return;
                        }
                        if (_lastnode != inode.Guid)
                        {
                            _lastnode = inode.Guid;
                            Logging.Write("Ground Farm " + inode.Name + " (" + inode.Entry + ") > " + inode.Position);
                        }
                        if (inode.GetDistance < 5.5f) // max range is usually 5.8-9 yards
                            MovementManager.StopMove();
                        if (MovementManager.InMovement)
                            return;
                    }
                    MovementManager.StopMove();
                    while (ObjectManager.ObjectManager.Me.GetMove)
                    {
                        Thread.Sleep(250);
                    }
                    Thread.Sleep(250 + Usefuls.Latency);
                    if (ObjectManager.ObjectManager.Me.InCombat)
                    {
                        if (!ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.AllInteractMountId()) &&
                            (!inode.IsHerb || inode.IsHerb && !ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.HerbsInteractMountId())))
                        {
                            MountTask.DismountMount(); // If we don't have druid mount or Sky Golem, dismount and fight.
                            return;
                        }
                        // We are druid or using sky golem, let's try to loot
                    }
                    _wasLooted = false;
                    CountThisLoot = true;
                    NodeOrUnit = true;
                    Interact.InteractWith(inode.GetBaseAddress);
                    Thread.Sleep(Usefuls.Latency + 300);
                    if (!ObjectManager.ObjectManager.Me.IsCast)
                    {
                        Interact.InteractWith(inode.GetBaseAddress);
                        Thread.Sleep(Usefuls.Latency + 250);
                    }
                    while (ObjectManager.ObjectManager.Me.IsCast)
                    {
                        Thread.Sleep(150);
                    }
                    if (ObjectManager.ObjectManager.Me.InCombat &&
                        (!ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.AllInteractMountId()) &&
                         (!inode.IsHerb || inode.IsHerb && !ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.HerbsInteractMountId()))))
                    {
                        CountThisLoot = false;
                        return;
                    }
                    Thread.Sleep(100 + Usefuls.Latency);
                    if (ObjectManager.ObjectManager.Me.InCombat &&
                        (!ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.AllInteractMountId()) &&
                         (!inode.IsHerb || inode.IsHerb && !ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.HerbsInteractMountId()))))
                    {
                        CountThisLoot = false;
                        return;
                    }
                    if (CountThisLoot && !ObjectManager.ObjectManager.Me.InCombat)
                        nManagerSetting.AddBlackList(inode.Guid, 1000 * 20);

                    Thread.Sleep(1000);
                    if (!_wasLooted)
                    {
                        Logging.Write("Farm failed #2");
                        if (ObjectManager.ObjectManager.Me.InCombat &&
                            (ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.HerbsInteractMountId()) || ObjectManager.ObjectManager.Me.HaveBuff(SpellManager.AllInteractMountId())))
                            MountTask.DismountMount(); // we got cancelled during farm, let's fight this out for good..
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("FarmingTask > Ground(IEnumerable<WoWGameObject> nodes): " + ex);
            }
        }

        public static void TakeFarmingLoots()
        {
            if (CountThisLoot)
            {
                _wasLooted = true;
                CountThisLoot = false;
                LootingTask.LootAndConfirmBoPForAllItems(nManagerSetting.CurrentSetting.AutoConfirmOnBoPItems);
                Thread.Sleep(200);
                if (!Others.IsFrameVisible("LootFrame"))
                {
                    Statistics.Farms++;
                    Logging.Write("Farm successful");
                }
                else
                {
                    Statistics.Farms++;
                    Logging.Write("Farm partially successful");
                    // LootWindow is still open after we accepted BoP, so one item must be stuck. (Unique(X), InventoryFull, ...)
                }
                // We had a valid LOOT_READY anyway, with our force loot function, that would have taken < 1 sec to loot anyway.
                // So let's blacklist inode/unit !
                if (NodeOrUnit && _curNode != null && _curNode.IsValid)
                    nManagerSetting.AddBlackList(_curNode.Guid, 60 * 1000);
                if (!NodeOrUnit && CurUnit != null && CurUnit.IsValid)
                    nManagerSetting.AddBlackList(CurUnit.Guid, 60 * 1000);
                _curNode = null;
                CurUnit = null;
            }
        }
    }
}