using HarmonyLib;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BetterInfoNS
{
    public class BetterInfo : Mod
    {
        //public static Harmony? HarmonyInstance = new Harmony("BetterInfo");
		
		
		override public void Ready()
		{
						
			try
			{
				//HarmonyInstance = new Harmony("BetterInfo");
				Harmony.PatchAll(typeof(BetterInfo));
			}
			catch(Exception e)
			{
				Logger.Log("Patching failed: " + e.Message);
			}
			AdvancedSettingsScreen.AdvancedCombatStatsEnabled = true;
		}
		private void OnDestroy()
        {
            Harmony.UnpatchSelf();
        }

		/*
		[HarmonyPatch(typeof(Combatable), "GetCombatableDescription")]
		[HarmonyPrefix]
		private static void Combatable__GetCombatableDescription_Prefix(out string __state, string ____combatableDescription)
		{
			__state = ____combatableDescription;
		}
		

		[HarmonyPatch(typeof(Combatable), "GetCombatableDescription")]
		[HarmonyPostfix]
		private static void Combatable__GetCombatableDescription_Postfix(ref Combatable __instance, ref string __state, ref string ____combatableDescription)
		{
			if (string.IsNullOrEmpty(__state))  
			{
				if (!__instance.MyGameCard.IsDemoCard)
				{
					____combatableDescription += "</i>";
					//____combatableDescription += "\\d";
					____combatableDescription += __instance.GetCombatableDescriptionAdvanced();

				}
			}
		}
        */

		/*
		[HarmonyPatch(typeof(Combatable), "OnEquipItem")]
		[HarmonyPostfix]
		private static void Combatable__OnEquipItem_Postfix(ref CardData __instance, ref string ___descriptionOverride)
		{
			___descriptionOverride = "";
		}
		[HarmonyPatch(typeof(Combatable), "Damage")]
		[HarmonyPostfix]
		private static void Combatable__Damage_Postfix(ref string ___descriptionOverride)
		{
			___descriptionOverride = "";
		}
		[HarmonyPatch(typeof(Combatable), "ExitConflict")]
		[HarmonyPostfix]
		private static void Combatable__ExitConflict_Postfix(ref string ___descriptionOverride)
		{
			___descriptionOverride = "";
		}
		[HarmonyPatch(typeof(Combatable), "GetDamage")]
		[HarmonyPostfix]
		private static void Combatable__GetDamage_Postfix(ref string ___descriptionOverride)
		{
			___descriptionOverride = "";
		}
		
		[HarmonyPatch(typeof(GameCard), "Unequip")]
		[HarmonyPostfix]
		private static void CardData__Unequip_Postfix(Equipable equipable, ref GameCard __instance)
		{
			Logger.Log("uneq");
			if (__instance.CardData is Combatable)
				__instance.CardData.OnUnequipItem(equipable);
		}
		
		[HarmonyPatch(typeof(CardData), "OnUnequipItem")]
		[HarmonyPrefix]
		private static void CardData__OnUnequipItem_Prefix(ref CardData __instance, ref string ___descriptionOverride)
		{
			Logger.Log("uneq1");
			if (__instance is Combatable)
				___descriptionOverride = "";
		}
		*/
		/*
		[HarmonyPatch(typeof(Combatable), "GetCombatableDescription")]
		[HarmonyPrefix]
		private static bool Combatable__GetCombatableDescription_Prefix(ref Combatable __instance, ref string __result)
		{
			__result = "";
			return false;
		}*/
		
		[HarmonyPatch(typeof(Combatable), "GetCombatableDescriptionAdvanced")]
		[HarmonyPostfix]
		private static void Combatable__GetCombatableDescriptionAdvanced_Postfix(ref Combatable __instance, ref string __result)
		{
			string labelSpeed = SokLoc.Translate("label_combat_speed");
			string labelChance = SokLoc.Translate("label_hit_chance");
			string labelDamage = SokLoc.Translate("label_damage");
			string labelDefence = SokLoc.Translate("label_defence");
			float baseAttackDamageFromEnum1 = __instance.ProcessedCombatStats.AttackDamage;
			float baseAttackDamageFromEnum2 = CombatStats.IncrementAttackDefence(__instance.ProcessedCombatStats.AttackDamage, 1); // with .5 chance attack is baffed by 1
			float baseAttackDamageFromEnum = baseAttackDamageFromEnum1 * 0.5f + baseAttackDamageFromEnum2 * 0.5f - 1f; // -1 because smallest opposing defense is 1            if (baseAttackDamageFromEnum <= 0.5f) baseAttackDamageFromEnum += 0.5f; // if attack dmg is 0 it gets buffed to 1 with .5 chance
			float calculatedDmg = 0f;
			float num = 0f;
			float hitChanceFromEnum = __instance.ProcessedCombatStats.HitChance;
			float attackTimeFromEnum = __instance.ProcessedCombatStats.AttackSpeed;
			
			foreach (SpecialHit specialHit in __instance.ProcessedCombatStats.SpecialHits)
			{
				num += specialHit.Chance;
				float specialDmg = 0f;
				if (specialHit.Target == SpecialHitTarget.Target || specialHit.Target == SpecialHitTarget.RandomEnemy || specialHit.Target == SpecialHitTarget.AllEnemy)
				{
					switch (specialHit.HitType)
					{
						case SpecialHitType.Poison:
							specialDmg += (specialHit.Chance / 100f) * (baseAttackDamageFromEnum + 0.1f);
							break;
						case SpecialHitType.Crit:
							specialDmg += (specialHit.Chance / 100f) * (baseAttackDamageFromEnum * 2f);
							break;
						case SpecialHitType.Bleeding:
							specialDmg += (specialHit.Chance / 100f) * (baseAttackDamageFromEnum + 2.5f);
							break;
						case SpecialHitType.Sick:
							specialDmg += (specialHit.Chance / 100f) * (baseAttackDamageFromEnum + 0.2f);
							break;
						case SpecialHitType.Stun:
						case SpecialHitType.LifeSteal:
						case SpecialHitType.Damage:
							specialDmg += (specialHit.Chance / 100f) * (baseAttackDamageFromEnum);
							break;
					}
				}
				
				if (specialHit.Target == SpecialHitTarget.AllEnemy)
				{
					specialDmg *= 2.5f;
				}
				
				if (specialHit.Target == SpecialHitTarget.Self && specialHit.HitType == SpecialHitType.Frenzy)
				{
					float possibl = Mathf.Clamp((10f / attackTimeFromEnum) * (specialHit.Chance / 100f), 0, 1);
					attackTimeFromEnum = (1 - possibl) * attackTimeFromEnum + possibl * CombatStats.IncrementAttackSpeed(__instance.ProcessedCombatStats.AttackSpeed, 1);
				}
				
				calculatedDmg += specialDmg;
			}
			calculatedDmg += baseAttackDamageFromEnum * (100f - num) / 100f;
			
			float defenceFromEnum = Mathf.CeilToInt((float)__instance.ProcessedCombatStats.Defence * 0.5f);
			
			string AttackSpeed = __instance.ProcessedCombatStats.GetAttackSpeedTranslation() + " (" + __instance.ProcessedCombatStats.AttackSpeed +"s)";
			string AttackDamage = __instance.ProcessedCombatStats.GetAttackDamageTranslation() + " (" + baseAttackDamageFromEnum1 +"dmg)";
			string HitChance = __instance.ProcessedCombatStats.GetHitChanceTranslation() + " (" + (hitChanceFromEnum * 100f).ToString("0") +"%)";
			string Defence = __instance.ProcessedCombatStats.GetDefenceTranslation() + " (" + defenceFromEnum +"dmg)";
			
			string LabelEstimatedDmgS = "Estimated dmg/s";
			string averageAttackDamagePerSecond = (calculatedDmg / attackTimeFromEnum * hitChanceFromEnum).ToString("0.00");
			
			string attackType =  __instance.MyGameCard != null ?  __instance.ProcessedAttackType.ToString() : __instance.BaseAttackType.ToString();


			__result = "</i><size=90%>" ;
			__result += attackType;
			__result += "\n" + labelSpeed + " " + AttackSpeed +
				"\n" + labelChance + " " + HitChance +
				"\n" + labelDamage + " " + AttackDamage +
				"\n" + labelDefence + ": " + Defence +
				"\n" + LabelEstimatedDmgS + ": " + averageAttackDamagePerSecond + "</size>";

			
			if (__instance is Animal animal && animal.CreateCard != "")
			{
				__result += "\n\n" + "Makes one " + WorldManager.instance.GetCardPrefab(animal.CreateCard)?.Name + " every " + animal.CreateTime.ToString() + "s.";
			}
			if (__instance is Mob mob && (CardopediaScreen.instance.HoveredEntry == null || CardopediaScreen.instance.HoveredEntry.MyCardData != __instance))
			{
				__result += "\n\n" + BetterInfo.GetSummaryFromAllCards(mob.Drops, "label_can_drop");
			}
		}
		
		
		[HarmonyPatch(typeof(CardData), "GetPossibleDrops")]
		[HarmonyPostfix]
		private static void CardData__GetPossibleDrops_Postfix(ref CardData __instance, ref  List<string> __result)
		{
		// Logger.Log("CardData__GetPossibleDrops_Postfix");
			if (__result.Count == 0 && __instance is Mob mob && __instance is not Enemy)
			{
				List<string>? drops = mob?.Drops?.GetCardsInBag();
				if (drops != null && drops.Count > 0)
				{
					__result.AddRange(drops);
				}
				if (mob?.CanHaveInventory ?? false)
				{
					__result.AddRange((from x in mob?.PossibleEquipables
						where x.blueprint != null
						select x.blueprint?.Id)?.ToList());
				}
			}
		}
		
		[HarmonyPatch(typeof(StablePortal), "UpdateCard")]
		[HarmonyPrefix]
		private static void StablePortal__UpdateCard_Prefix(out bool __state, ref string ___descriptionOverride)
		{
			__state = string.IsNullOrWhiteSpace(___descriptionOverride);
		}
		
		[HarmonyPatch(typeof(StablePortal), "UpdateCard")]
		[HarmonyPostfix]
		private static void StablePortal__UpdateCard_Postfix(bool __state, ref string ___descriptionOverride, ref CardData __instance)
		{
			if (__state && !string.IsNullOrWhiteSpace(___descriptionOverride) && !__instance.MyGameCard.IsDemoCard)
			{
				___descriptionOverride += "\n" + SokLoc.Translate("label_wave", LocParam.Create("wave", WorldManager.instance.CurrentRunVariables.ForestWave.ToString()));
			}
		}
		
		[HarmonyPatch(typeof(BoosterpackData), "GetSummary")]
		[HarmonyPrefix]
		private static bool BoosterpackData__GetSummary_Prefix(out string __result, BoosterpackData __instance)
		{
		//Logger.Log("Boosterpack__GetSummary_Prefix");
			__result = "";
			List<CardChance> list = new List<CardChance>();
			foreach (CardBag cardBag in __instance.CardBags)
			{
				__result += BetterInfo.GetSummaryFromAllCards(cardBag) + "\n";
			}
			//__result = BetterInfo.GetSummaryFromAllCards(list);
			return false;
		}
		
		[HarmonyPatch(typeof(Harvestable), "UpdateDescription")]
		[HarmonyPrefix]
		private static bool Harvestable__UpdateDescription_Prefix(Harvestable __instance, ref string ___descriptionOverride)
		{
			//Logger.Log("Harvestable__UpdateDescription_Prefix");
			___descriptionOverride = SokLoc.Translate(__instance.DescriptionTerm, LocParam.Create("required_count", __instance.RequiredVillagerCount.ToString())) + "\n\n";
			if (__instance.Id == "catacombs")
			{
				CardBag cardBag = new CardBag();
				cardBag.CardBagType = CardBagType.Chances;
				cardBag.CardsInPack = 1;
				cardBag.Chances = new List<CardChance> (__instance.MyCardBag.Chances);
				cardBag.Chances.Add(new CardChance("goblet", __instance.Amount > 1 ? 2 : 0 ));
				___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", __instance.IsUnlimited ? 0 : __instance.Amount) + "\n";
			}
			else if (__instance.Id == "cave")
			{
				CardBag cardBag = new CardBag();
				cardBag.CardBagType = CardBagType.Chances;
				cardBag.CardsInPack = 1;
				cardBag.Chances = new List<CardChance>();
				
				foreach (CardChance chance in __instance.MyCardBag.Chances)
				{
				
					cardBag.Chances.Add(chance.Id == "treasure_map" ? new CardChance(chance.Id, __instance.Amount > 1 ? 2 : 0 ) : chance);
				}
				___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", __instance.IsUnlimited ? 0 : __instance.Amount) + "\n";
			}
			else if (__instance.Id == "ruins")
			{
				CardBag cardBag = new CardBag();
				cardBag.CardBagType = CardBagType.Chances;
				cardBag.CardsInPack = 1;
				cardBag.Chances = new List<CardChance>();
				
				foreach (CardChance chance in __instance.MyCardBag.Chances)
				{
				
					cardBag.Chances.Add(chance.Id == "blueprint_fountain_of_youth" ? new CardChance(chance.Id, __instance.Amount > 1 ? 2 : 0 ) : chance);
				}
				___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", __instance.IsUnlimited ? 0 : __instance.Amount) + "\n";
			}
			else if (__instance.Id == "old_tome")
			{
                List<CardData> list = WorldManager.instance.CardDataPrefabs.Where((CardData x) => x.MyCardType == CardType.Ideas && !WorldManager.instance.HasFoundCard(x.Id) && !x.HideFromCardopedia).ToList();
                list.RemoveAll((CardData x) => x.CardUpdateType == CardUpdateType.Spirit);
                if (!WorldManager.instance.CurrentRunVariables.VisitedIsland)
                {
                    list.RemoveAll((CardData x) => x.CardUpdateType == CardUpdateType.Island);
                }

				CardBag cardBag = new CardBag();
				cardBag.CardBagType = CardBagType.Chances;
				cardBag.CardsInPack = 1;
				cardBag.Chances = new List<CardChance>();
				if (list.Count <= 0)
				{
					cardBag.Chances.Add(new CardChance("map",1));
				}
				else
				{
					list.ForEach((CardData x) => cardBag.Chances.Add(new CardChance(x.Id,1)));
				}
				___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", __instance.IsUnlimited ? 0 : __instance.Amount) + "\n";	
			}
			else if (__instance is FishingSpot)
			{
				foreach (CardBag cardBag in __instance.GetCardBags())
				{
					___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", __instance.IsUnlimited ? 0 : __instance.Amount) + "\n";
				}
			}
			else foreach (CardBag cardBag in __instance.GetCardBags())
			{
				___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", __instance.IsUnlimited ? 0 : __instance.Amount) + "\n";
			}
			
			return false;
		}
		
		[HarmonyPatch(typeof(CombatableHarvestable), "UpdateCard")]
		[HarmonyPrefix]
		private static void CombatableHarvestable__UpdateDescription_Prefix(CombatableHarvestable __instance, ref string ___descriptionOverride)
		{
			if(string.IsNullOrEmpty(___descriptionOverride))
			{
		//Logger.Log("CombatableHarvestable__UpdateDescription_Prefix");
				___descriptionOverride = SokLoc.Translate(__instance.DescriptionTerm) + "\n\n";
				foreach (CardBag cardBag in __instance.GetCardBags())
				{
					___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop",__instance.IsUnlimited ? 0 : __instance.Amount) + "\n";
				}
			}
		}
		
		[HarmonyPatch(typeof(TravellingCart), "UpdateCard")]
		[HarmonyPrefix]
		private static void TravellingCart__UpdateDescription_Prefix(TravellingCart __instance, ref string ___descriptionOverride)
		{
			if(string.IsNullOrEmpty(___descriptionOverride))
			{
				___descriptionOverride = SokLoc.Translate(__instance.DescriptionTerm) + "\n\n";
				foreach (CardBag cardBag in __instance.GetCardBags())
				{
					___descriptionOverride += BetterInfo.GetSummaryFromAllCards(cardBag, "label_can_drop", 0) + "\n";
				}
			}
		}
		
		[HarmonyPatch(typeof(CardopediaScreen), "GetDropSummaryFromCard")]
		[HarmonyPrefix]
		private static bool CardopediaScreen__GetDropSummaryFromCard_Prefix(out string __result, CardData cardData)
		{
		
			__result = "";
			return cardData is not Harvestable && cardData is not CombatableHarvestable && cardData is not Mob;
		}
		
		
        private class CardIdWithEquipmentCombat
        {
            public List<string> Equipment = new List<string>();

            public float TotalCombatLevel;

            public string Id { get; set; }

            public CardIdWithEquipmentCombat(string id, List<string> equipment, float totalCombatlevel)
            {
                Id = id;
                Equipment = equipment;
                TotalCombatLevel = totalCombatlevel;
            }

            public CardIdWithEquipment ToCardIdWithEquipment()
            {
                return new CardIdWithEquipment(Id, Equipment);
            }

            public override string ToString()
            {
                if (Equipment.Count == 0)
                {
                    return Id;
                }
                return Id + " (" + string.Join(", ", Equipment) + ")";
            }
        }

        private static List<Equipable> GetEquipableOfType(List<Equipable> equipables, EquipableType t)
        {
            return equipables.Where((Equipable x) => x.EquipableType == t).ToList();
        }

        private static List<CardIdWithEquipmentCombat> GetAllPossibleEnemiesWithEquipment(List<Combatable> enemyPool)
        {
            List<CardIdWithEquipmentCombat> list = new List<CardIdWithEquipmentCombat>();
            foreach (Combatable item in enemyPool)
            {
                list.Add(new CardIdWithEquipmentCombat(item.Id, new List<string>(), item.RealBaseCombatStats.CombatLevel));
                if (!item.HasInventory)
                {
                    continue;
                }
                List<Equipable> equipableOfType = GetEquipableOfType(item.PossibleEquipables, EquipableType.Head);
                equipableOfType.Add(null);
                List<Equipable> equipableOfType2 = GetEquipableOfType(item.PossibleEquipables, EquipableType.Weapon);
                equipableOfType2.Add(null);
                List<Equipable> equipableOfType3 = GetEquipableOfType(item.PossibleEquipables, EquipableType.Torso);
                equipableOfType3.Add(null);
                foreach (Equipable item2 in equipableOfType)
                {
                    foreach (Equipable item3 in equipableOfType2)
                    {
                        foreach (Equipable item4 in equipableOfType3)
                        {
                            List<string> list2 = new List<string>();
                            CombatStats combatStats = new CombatStats();
                            combatStats.InitStats(item.RealBaseCombatStats);
                            if (item2 != null)
                            {
                                combatStats.AddStats(item2.MyStats);
                                list2.Add(item2.Id);
                            }
                            if (item3 != null)
                            {
                                combatStats.AddStats(item3.MyStats);
                                list2.Add(item3.Id);
                            }
                            if (item4 != null)
                            {
                                combatStats.AddStats(item4.MyStats);
                                list2.Add(item4.Id);
                            }
                            if (list2.Count > 0)
                            {
                                list.Add(new CardIdWithEquipmentCombat(item.Id, list2, combatStats.CombatLevel));
                            }
                        }
                    }
                }
            }
            return list;
        }
		public static List<CardChance> CardBagToChance(CardBag bag)
		{
		//Logger.Log("CardBagToChance");
			List<CardChance> chances = new List<CardChance>();
			
			if (bag.CardBagType == CardBagType.SetCardBag)
			{
				chances = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, bag.SetCardBag);
			}
			else if (bag.CardBagType == CardBagType.SetPack)
			{
				chances = bag.SetPackCards.Select((string x) => new CardChance(x, 1)).ToList();
			}
			else if (bag.CardBagType == CardBagType.Chances)
			{
				chances = bag.Chances;
			}
			else if (bag.CardBagType == CardBagType.Enemies)
			{
				SetCardBagType setCardBagForEnemyCardBag = WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBag(bag.EnemyCardBag);
				chances = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, setCardBagForEnemyCardBag);
				chances.RemoveAll(delegate(CardChance x)
				{
					Combatable? combatable = WorldManager.instance.GameDataLoader.GetCardFromId(x.Id) as Combatable;
					return (combatable != null && combatable.ProcessedCombatStats.CombatLevel > bag.StrengthLevel) ? true : false;
				});
			}
			
			List<CardChance> chances2 = new List<CardChance>();
			foreach (CardChance c in chances)
			{
				if (c.IsEnemy)
				{
					chances2.Add(c);
					continue;
				}
				if ((c.HasMaxCount && WorldManager.instance.AllCards.Count((GameCard card) => card.CardData.Id == c.Id && card.MyBoard.IsCurrent) >= c.MaxCountToGive)
					|| (c.HasPrerequisiteCard && !WorldManager.instance.GivenCards.Contains(c.PrerequisiteCardId)))
				{
					continue;
				}
				CardData cardPrefab = WorldManager.instance.GetCardPrefab(c.Id);
				if ((!WorldManager.instance.CurrentRunOptions.IsPeacefulMode || !(cardPrefab is Enemy))
					&& (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode || !(cardPrefab.Id == "catacombs"))
					&& ((cardPrefab.MyCardType != CardType.Ideas && cardPrefab.MyCardType != CardType.Rumors) || !WorldManager.instance.CurrentSave.FoundCardIds.Contains(c.Id)))
				{
					chances2.Add(c);
				}
			}
			
			float num = 0f;
			
			foreach (CardChance chance in chances2)
			{
				num += (float)chance.Chance;
			}
			foreach (CardChance chance2 in chances2)
			{
				chance2.PercentageChance = (float)chance2.Chance / num;
			}
			List<CardChance> chances3 = new List<CardChance>();
			
			foreach (CardChance c in chances2)
			{
				if (c.IsEnemy)
				{
					//Logger.Log("attempting to calculate enemy chance");
					SetCardBagType setCardBagForEnemyCardBag = WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBag(c.EnemyBag);
                    
                    List<CardIdWithEquipmentCombat> allPossibleEnemiesWithEquipment = GetAllPossibleEnemiesWithEquipment(SpawnHelper.GetEnemyPoolFromCardbags(setCardBagForEnemyCardBag.AsList(), true));
                    allPossibleEnemiesWithEquipment.RemoveAll((CardIdWithEquipmentCombat x) => x.TotalCombatLevel > c.Strength);
                    allPossibleEnemiesWithEquipment = allPossibleEnemiesWithEquipment.OrderByDescending((CardIdWithEquipmentCombat x) => x.TotalCombatLevel).ToList();
                    List<string> enemyList = allPossibleEnemiesWithEquipment.Select((CardIdWithEquipmentCombat x) => x.Id).Distinct().ToList();
                    
                    List<CardChance> enemyChances = new List<CardChance>();
					
                    foreach (string enemyId in enemyList)
					{
                        enemyChances.Add(new CardChance(enemyId, 1));
					}
					
					foreach (CardChance chance4 in enemyChances)
					{
						chance4.PercentageChance = (float)chance4.Chance * c.PercentageChance / (float)enemyChances.Count;
					}
                    chances3.AddRange(enemyChances);
					continue;
				}
				else
				{
					//Logger.Log("asasa");
					chances3.Add(c);
				}
			}
			
			return chances3.OrderByDescending((CardChance x) => x.PercentageChance).ToList();;
		}
		
		
		public static string GetSummaryFromAllCards(CardBag cardBag, string prefix = "label_may_contain", int customNumber = -1)
		{
			//Logger.Log("getting summary");
			List<CardChance> allCards = BetterInfo.CardBagToChance(cardBag);
			if (allCards.Count == 0)
			{
				return "";
			}
			List<CardChance> list = allCards.Distinct().ToList();
			List<string> list2 = new List<string>();
			int num = 0;
			float unDiscoveredChance = 0f;
			float unDiscChance = 0f;
			foreach (CardChance item2 in list)
			{
				CardData cardPrefab = WorldManager.instance.GetCardPrefab(item2.Id);
				string item = cardPrefab.FullName;
				if (cardPrefab.MyCardType == CardType.Ideas)
				{
					item = SokLoc.Translate("label_an_idea");
				}
				if (cardPrefab.MyCardType == CardType.Rumors)
				{
					item = SokLoc.Translate("label_a_rumor");
				}
				item =  Icons.Circle + (item2.PercentageChance * 100).ToString("0") + "%: " + item;
				if (!WorldManager.instance.CurrentSave.FoundCardIds.Contains(item2.Id))
				{
					num++;
					unDiscoveredChance += item2.PercentageChance * 100;
				}
				else if (!list2.Contains(item))
				{
					list2.Add(item);
				}
			}
			//list2 = list2.Select((string x) => Icons.Circle + x + ).ToList();
			string text = list2.Any() ? string.Join("\n", list2) : "";
			string text2 = "";
			
			if (!string.IsNullOrEmpty(prefix))
			{
				text2 = SokLoc.Translate(prefix) + "\n";
			}
			
			if (customNumber == -1)
			{
				text2 += cardBag.CardsInPack.ToString() + "x\n";
			}
			else if (customNumber > 0)
			{
				text2 += customNumber.ToString() + "x\n";
			}
			
			if (num > 0)
			{
				text2 = text2 + Icons.Circle + unDiscoveredChance.ToString("0") + "%: <size=90%>" + SokLoc.Translate("label_undiscovered_cards", LocParam.Plural("count", num)) +"</size>\n";
			}
			return text2 + text + "\n";
		}
		
				
		[HarmonyPatch(typeof(Equipable), "GetAdvancedEquipableInfo")]
		[HarmonyPostfix]
		private static void Equipable__GetAdvancedEquipableInfo_Postfix(ref Equipable __instance, ref string __result)
		{
			if (__instance.AttackType != AttackType.None)  
			{
				__result = __instance.AttackType.ToString() + "\n" + __result;
			}
		}
		/*
		[HarmonyPatch(typeof(Equipable), "GetEquipableInfo")]
		[HarmonyPostfix]
		private static void Equipable__GetEquipableInfo_Postfix(ref Equipable __instance, ref string __result)
		{
			__result = __result.Replace("\\d","</i>\n\n");
		}*/
		
		[HarmonyPatch(typeof(GameScreen), "Update")]
		[HarmonyPostfix]
		private static void GameScreen__Update_Postfix(GameScreen __instance)
		{
			GameCard hoveredCard = null;
			if (WorldManager.instance.DraggingCard != null)
			{
				hoveredCard = WorldManager.instance.DraggingCard;
			}
			else if (WorldManager.instance.HoveredCard != null)
			{
				hoveredCard = WorldManager.instance.HoveredCard;
			}

			if (hoveredCard == null)
			{
				Boosterpack boosterpack2 = null;
				if (WorldManager.instance.DraggingDraggable is Boosterpack)
				{
					boosterpack2 = WorldManager.instance.DraggingDraggable as Boosterpack;
				}
				else if (WorldManager.instance.HoveredDraggable is Boosterpack)
				{
					boosterpack2 = WorldManager.instance.HoveredDraggable as Boosterpack;
				}
				if (boosterpack2 != null)
				{
					__instance.InfoText.text = __instance.InfoText.text + "\n\n" + boosterpack2.PackData.GetSummary();
				}
			}
			if (hoveredCard != null && hoveredCard.IsPartOfStack())
			{ //"\n-—--—-———-——-—-——--—\n"
				__instance.InfoText.text += "\n<u>_    __   _       _     _ _          _</u>\n\n"+hoveredCard.CardData.FullName + "\n<size=90%>" + Regex.Replace(Regex.Replace(hoveredCard.CardData.Description, "<(/|)size(=.*?%|)>", ""), @"\\d", "\n\n") + "</size>";	
			}

// do shell display on island
			if (WorldManager.instance.CurrentBoard.Id == "island")
			{
				GameScreen.instance.MoneyText.text = $"{GetShellCount()} {Icons.Shell}  {WorldManager.instance.GetGoldCount(includeInChest: true)} {Icons.Gold}";
			}
		}

		public static int GetShellCount()
		{
			int num = 0;
			foreach (GameCard allCard in WorldManager.instance.AllCards)
			{
				if (allCard.MyBoard.IsCurrent && allCard.CardData is Chest chest && chest.HeldCardId == "shell")
				{
					num += chest.CoinCount;
				}
				else if (allCard.MyBoard.IsCurrent && allCard.CardData is Shell)
				{
					num ++;
				}
			}
			return num;
		}

		[HarmonyPatch(typeof(Blueprint), "GetText")]
		[HarmonyPostfix]
		private static void Blueprint__GetText_Postfix(ref Blueprint __instance, ref string __result)
		{
			string text = __instance.Subprints[0].ResultCard;
			if (string.IsNullOrEmpty(text) && __instance.Subprints[0].ExtraResultCards.Length != 0)
			{
				text = __instance.Subprints[0].ExtraResultCards[0];
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			CardData cardPrefab = WorldManager.instance.GetCardPrefab(text);
			cardPrefab.UpdateCardText();
			if (cardPrefab is Equipable equipable)
			{
				__result += "\n\n<size=90%>" + equipable.MyStats.SummarizeSpecialHits() + "\n\n" + equipable.GetEquipableInfoAdvanced();
			}
		}


	}
	
}