// Project:         Climates & Calories mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility;

namespace ClimatesCalories
{
    /// <summary>
    /// Abstract class for all food items common behaviour
    /// </summary>
    public abstract class AbstractItemFood : DaggerfallUnityItem
    {
        // In leu of a real enum.
        public const int StatusFresh = 0;
        public const int StatusStale = 1;
        public const int StatusMouldy = 2;
        public const int StatusRotten = 3;
        public const int StatusPutrid = 4;
        protected virtual bool CanSpoil { get; set; } = false;

        public AbstractItemFood(ItemGroups itemGroup, int templateIndex) : base(itemGroup, templateIndex)
        {
            message = StatusFresh;
        }

        public abstract uint GetCalories();

        public int FoodStatus
        {
            get { return message; }
            set { message = value; }
        }

        public virtual string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Smelly ";
                case StatusMouldy:
                    return "Mouldy ";
                case StatusRotten:
                    return "Rotten ";
                case StatusPutrid:
                    return "Putrid ";
                default:
                    return "";
            }
        }

        // Use template world archive for fresh/stale food, or template index for other states
        public override int InventoryTextureArchive
        {
            get
            {
                if (TemplateIndex > 102099 || FoodStatus == StatusFresh || FoodStatus == StatusStale)
                    return WorldTextureArchive;
                else
                    return TemplateIndex;
            }
        }

        // Use template world record for fresh food, or status for other states
        public override int InventoryTextureRecord
        {
            get
            {
                if (TemplateIndex > 102099)
                    return WorldTextureRecord;

                switch (FoodStatus)
                {
                    case StatusFresh:
                    case StatusStale:
                    default:
                        return WorldTextureRecord;
                    case StatusMouldy:
                        return 0;
                    case StatusRotten:
                        return 1;
                    case StatusPutrid:
                        return 1;
                }
            }
        }

        public bool RotFood()
        {
            if (FoodStatus < StatusPutrid)
            {
                FoodStatus++;
                shortName = GetFoodStatus() + ItemTemplate.name;
                value = 0;
                return false;
            }
            return true;
        }

        public override bool UseItem(ItemCollection collection)
        {
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            uint hunger = gameMinutes - playerEntity.LastTimePlayerAteOrDrankAtTavern;
            uint cals = GetCalories() / ((uint)FoodStatus + 1);
            string feel = "invigorated";

            if (FoodStatus == StatusPutrid)
            {
                DaggerfallUI.MessageBox(string.Format("This {0} is too disgusting to force down.", shortName));
            }
            else if (hunger + 1500 >= cals)
            {
                if (hunger > cals + 240)
                {
                    playerEntity.LastTimePlayerAteOrDrankAtTavern = gameMinutes - 240;
                }
                playerEntity.LastTimePlayerAteOrDrankAtTavern += cals;

                collection.RemoveItem(this);
                DaggerfallUI.MessageBox(string.Format("You eat the {0}.", shortName));

                if (FoodStatus > StatusStale || (TemplateIndex == ItemRawMeat.templateIndex || TemplateIndex == ItemRawFish.templateIndex))
                {
                    feel = TemplateIndex == ItemRawMeat.templateIndex ? "nauseated" : "invigorated";
                    bool unlucky = Dice100.SuccessRoll(playerEntity.Stats.LiveLuck);
                    if (unlucky || (TemplateIndex == ItemRawMeat.templateIndex || TemplateIndex == ItemRawFish.templateIndex))
                    {
                        feel = "disgusted";
                        Diseases[] diseaseListA = { Diseases.StomachRot };
                        Diseases[] diseaseListB = { Diseases.StomachRot, Diseases.SwampRot, Diseases.BloodRot, Diseases.Cholera, Diseases.YellowFever };
                        if (FoodStatus > StatusMouldy)
                            FormulaHelper.InflictDisease(playerEntity as DaggerfallEntity, playerEntity as DaggerfallEntity, diseaseListB);
                        else
                            FormulaHelper.InflictDisease(playerEntity as DaggerfallEntity, playerEntity as DaggerfallEntity, diseaseListA);
                    }
                }
                else
                {
                    GameManager.Instance.PlayerEntity.IncreaseHealth((int)( cals / 10));
                    GameManager.Instance.PlayerEntity.IncreaseMagicka((int)( cals / 10));
                    GameManager.Instance.PlayerEntity.IncreaseFatigue((int)( cals / 5));
                }
                

                DaggerfallUI.AddHUDText("You feel " + feel + " by the meal.");
            }
            else
            {
                DaggerfallUI.MessageBox(string.Format("You are not hungry enough to eat the {0} right now.", shortName));
            }
            return true;
        }

        public void FoodOnHUD()
        {
            if (ClimateCalories.isVampire)
            {
                DaggerfallUI.AddHUDText("The meal does nothing for you.");
            }
            else
            {
                DaggerfallUI.AddHUDText("You feel invigorated by the meal.");
            }
        }
    }

    //Apple
    public class ItemApple : AbstractItemFood
    {
        public const int templateIndex = 532;

        public ItemApple() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemApple).ToString();
            return data;
        }
    }

    //Orange
    public class ItemOrange : AbstractItemFood
    {
        public const int templateIndex = 533;

        public ItemOrange() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemOrange).ToString();
            return data;
        }
    }

    //Bread
    public class ItemBread : AbstractItemFood
    {
        public const int templateIndex = 534;

        public ItemBread() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 180;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Stale ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemBread).ToString();
            return data;
        }
    }

    //Fish
    public class ItemRawFish : AbstractItemFood
    {
        public const int templateIndex = 535;

        public ItemRawFish() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 90;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRawFish).ToString();
            return data;
        }
    }

    //Salted Fish
    public class ItemCookedFish : AbstractItemFood
    {
        public const int templateIndex = 536;

        public ItemCookedFish() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 200;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCookedFish).ToString();
            return data;
        }
    }

    //Meat
    public class ItemMeat : AbstractItemFood
    {
        public const int templateIndex = 537;

        public ItemMeat() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 240;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemMeat).ToString();
            return data;
        }
    }

    //Raw Meat
    public class ItemRawMeat : AbstractItemFood
    {
        public const int templateIndex = 538;

        public ItemRawMeat() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 100;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRawMeat).ToString();
            return data;
        }
    }

    public class ItemRations : DaggerfallUnityItem
    {
        public const int templateIndex = ClimateCalories.templateIndex_Rations;

        public ItemRations() : base(ItemGroups.UselessItems2, templateIndex)
        {
            stackCount = UnityEngine.Random.Range(2, 10);
        }

        public override bool IsStackable()
        {
            return true;
        }

        public override bool UseItem(ItemCollection collection)
        {
            DaggerfallUI.MessageBox(string.Format("When too hungry, you will eat some rations."));
            return true;
        }


        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRations).ToString();
            return data;
        }
    }

    //Cherries
    public class ItemCherries : AbstractItemFood
    {
        public const int templateIndex = 102100;

        public ItemCherries() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 30;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCherries).ToString();
            return data;
        }
    }

    //Pear
    public class ItemYellowPear : AbstractItemFood
    {
        public const int templateIndex = 102101;

        public ItemYellowPear() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemYellowPear).ToString();
            return data;
        }
    }

    //Plum
    public class ItemPlum : AbstractItemFood
    {
        public const int templateIndex = 102102;

        public ItemPlum() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemPlum).ToString();
            return data;
        }
    }

    //Peach
    public class ItemPeach : AbstractItemFood
    {
        public const int templateIndex = 102103;

        public ItemPeach() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemPeach).ToString();
            return data;
        }
    }

    //Olives
    public class ItemOlives : AbstractItemFood
    {
        public const int templateIndex = 102104;

        public ItemOlives() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 50;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemOlives).ToString();
            return data;
        }
    }

    //Cheese Wheel
    public class ItemCheeseWheel : AbstractItemFood
    {
        public const int templateIndex = 102105;

        public ItemCheeseWheel() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 640;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCheeseWheel).ToString();
            return data;
        }
    }

    //NearlyFullCheeseWheel
    public class ItemNearlyFullCheeseWheel : AbstractItemFood
    {
        public const int templateIndex = 102106;

        public ItemNearlyFullCheeseWheel() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 560;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemNearlyFullCheeseWheel).ToString();
            return data;
        }
    }

    //CheeseSlice
    public class ItemCheeseSlice : AbstractItemFood
    {
        public const int templateIndex = 102107;

        public ItemCheeseSlice() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 80;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCheeseSlice).ToString();
            return data;
        }
    }

    //SwissCheeseSlice
    public class ItemSwissCheeseSlice : AbstractItemFood
    {
        public const int templateIndex = 102108;

        public ItemSwissCheeseSlice() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 80;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemSwissCheeseSlice).ToString();
            return data;
        }
    }

    //SwissCheeseWheel
    public class ItemSwissCheeseWheel: AbstractItemFood
    {
        public const int templateIndex = 102109;

        public ItemSwissCheeseWheel() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 640;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemSwissCheeseWheel).ToString();
            return data;
        }
    }

    //Soft Cheese
    public class ItemSoftCheese : AbstractItemFood
    {
        public const int templateIndex = 102110;

        public ItemSoftCheese() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 360;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemSoftCheese).ToString();
            return data;
        }
    }

    //Pig Roast Platter
    public class ItemPigRoastPlatter : AbstractItemFood
    {
        public const int templateIndex = 102111;

        public ItemPigRoastPlatter() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 1200;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemPigRoastPlatter).ToString();
            return data;
        }
    }

    //Flour Porridge
    public class ItemFlourPorridge : AbstractItemFood
    {
        public const int templateIndex = 102112;

        public ItemFlourPorridge() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 30;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemFlourPorridge).ToString();
            return data;
        }
    }

    //Broth
    public class ItemBroth : AbstractItemFood
    {
        public const int templateIndex = 102113;

        public ItemBroth() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 15;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemBroth).ToString();
            return data;
        }
    }

    //Grapes
    public class ItemGrapes : AbstractItemFood
    {
        public const int templateIndex = 102114;

        public ItemGrapes() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemGrapes).ToString();
            return data;
        }
    }

    //White Grapes
    public class ItemWhiteGrapes : AbstractItemFood
    {
        public const int templateIndex = 102115;

        public ItemWhiteGrapes() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemWhiteGrapes).ToString();
            return data;
        }
    }

    //Cabbage Head
    public class ItemCabbageHead : AbstractItemFood
    {
        public const int templateIndex = 102116;

        public ItemCabbageHead() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 30;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCabbageHead).ToString();
            return data;
        }
    }

    //Yellow Tomato
    public class ItemYellowTomato : AbstractItemFood
    {
        public const int templateIndex = 102117;

        public ItemYellowTomato() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (FoodStatus)
            {
                case StatusStale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemYellowTomato).ToString();
            return data;
        }
    }



}

