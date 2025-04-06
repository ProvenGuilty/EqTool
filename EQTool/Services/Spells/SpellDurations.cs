﻿using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    public class SpellDurations
    {
        private readonly FightHistory fightHistory;
        private readonly ActivePlayer activePlayer;
        public SpellDurations(FightHistory fightHistory, ActivePlayer activePlayer)
        {
            this.fightHistory = fightHistory;
            this.activePlayer = activePlayer;
        }

        public Spell MatchDragonRoar(List<Spell> spells, DateTime timestamp)
        {
            if (!string.IsNullOrWhiteSpace(activePlayer.Player?.Zone))
            {
                if (Zones.ZoneInfoMap.TryGetValue(activePlayer.Player.Zone, out var zone))
                {
                    var matchingnpcs = fightHistory.IsEngaged(zone.NPCThatAOE.Select(a => a.Name).ToList(), timestamp);
                    foreach (var item in matchingnpcs)
                    {
                        var npc = zone.NPCThatAOE.FirstOrDefault(a => a.Name == item);
                        var matchedspell = spells.FirstOrDefault(a => npc.SpellEffects.Contains(a.name));
                        if (matchedspell != null)
                        {
                            return matchedspell;
                        }
                    }
                }
                foreach (var npc in zone.NPCThatAOE)
                {
                    var matchedspell = spells.FirstOrDefault(a => npc.SpellEffects.Contains(a.name));
                    if (matchedspell != null)
                    {
                        return matchedspell;
                    }
                }
            }

            return null;
        }

        public bool IsDragonRoarSpell(Spell spell)
        {
            foreach (var zone in Zones.ZoneInfoMap.Values)
            {
                foreach (var npc in zone.NPCThatAOE)
                {
                    var matchedspell = npc.SpellEffects.Any(a => a == spell.name);
                    if (matchedspell)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public Spell MatchClosestLevelToSpell(Spell spell, DateTime timestamp)
        {
            return MatchClosestLevelToSpell(new List<Spell> { spell }, timestamp);
        }

        public Spell MatchClosestLevelToSpell(List<Spell> spells, DateTime timestamp)
        {
            var playerClass = activePlayer.Player.PlayerClass;
            var playerLevel = activePlayer.Player.Level;

            if (playerClass.HasValue)
            {
                var closestlevel = playerLevel;
                var smallestdelta = closestlevel;
                Spell closestspell = null;
                foreach (var spell in spells)
                {
                    foreach (var item in spell.Classes)
                    {
                        var delta = Math.Abs(item.Value - playerLevel);
                        if (delta < smallestdelta)
                        {
                            closestspell = spell;
                            smallestdelta = delta;
                        }
                    }
                }

                if (closestspell != null)
                {
                    return closestspell;
                }
            }

            foreach (var item in spells)
            {
                foreach (var level in item.Classes)
                {
                    if (level.Value > 0 && level.Value <= 60)
                    {
                        return item;
                    }
                }
            }

            return spells.FirstOrDefault();
        }

        public static int MatchClosestLevelToSpell(Spell spell, PlayerClasses? playerClass, int? playerLevel)
        {
            if (playerClass.HasValue && playerLevel.HasValue)
            {
                if (spell.Classes.TryGetValue(playerClass.Value, out var foundlewvel))
                {
                    return playerLevel.Value < foundlewvel ? foundlewvel : playerLevel.Value;
                }
            }

            if (playerLevel.HasValue)
            {
                foreach (var item in spell.Classes.OrderByDescending(a => a.Value))
                {
                    return (playerLevel < item.Value ? item.Value : playerLevel) ?? 30;
                }
                var closestlevel = playerLevel.Value;
                foreach (var item in spell.Classes)
                {
                    var delta = Math.Abs(item.Value - closestlevel);
                    if (delta < closestlevel)
                    {
                        closestlevel = delta;
                    }
                }
            }

            var level = spell.Classes.Any() ? spell.Classes.FirstOrDefault().Value : (int?)null;
            if (((level.HasValue && level <= 0) || !level.HasValue) && playerLevel.HasValue)
            {
                level = playerLevel.Value;
            }

            if ((level.HasValue && level <= 0) || !level.HasValue)
            {
                level = 30;
            }

            return level.Value;
        }

        public static int GetDuration_inSeconds(Spell spell, PlayerClasses? playerClass, int? playerLevel)
        {
            var duration = spell.buffduration;
            int spell_ticks;
            var level = MatchClosestLevelToSpell(spell, playerClass, playerLevel);

            switch (spell.buffdurationformula)
            {
                case 0:
                    spell_ticks = 0;
                    break;
                case 1:
                    spell_ticks = (int)Math.Ceiling(level / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 2:
                    spell_ticks = (int)Math.Ceiling(level / 5.0f * 3);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 3:
                    spell_ticks = level * 30;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 4:
                    spell_ticks = duration == 0 ? 50 : duration;
                    break;
                case 5:
                    spell_ticks = duration;
                    if (spell_ticks == 0)
                    {
                        spell_ticks = 3;
                    }

                    break;
                case 6:
                    spell_ticks = (int)Math.Ceiling(level / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 7:
                    spell_ticks = level;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 8:
                    spell_ticks = level + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 9:
                    spell_ticks = (level * 2) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 10:
                    spell_ticks = (level * 3) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 11:
                case 12:
                case 15:
                    spell_ticks = duration;
                    break;
                case 50:
                    spell_ticks = 72000;
                    break;
                case 3600:
                    spell_ticks = duration == 0 ? 3600 : duration;
                    break;
                default:
                    spell_ticks = duration;
                    break;
            }

            return spell_ticks * 6;
        }
    }
}
