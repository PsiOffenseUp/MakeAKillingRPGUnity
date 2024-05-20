using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace BoogalooGame
{
    #region ItemData classes
    //Class for items that can be bought and sold
    public class ItemData
    {
        public enum UsableNums : int
        {
            SELLAMATRON = 3,
            MEWMERANG = 9,
        }

        public string name { get; protected set; }
        public int id { get; protected set; }
        public int sellPrice { get; protected set; }
        public int buyPrice { get; protected set; }
        public uint weight { get; protected set; }
        public string path { get; protected set; }
        public string description { get; protected set; }
        public bool buyable { get; protected set; }
        public bool collectable { get; protected set; }
        public ItemData(string[] values, int id) 
        {
            //Set all of the attributes
            this.id = id; //Set the id
            this.name = values[0]; //Set the name read from file
            this.buyPrice = int.Parse(values[2]);
            this.sellPrice = int.Parse(values[3]);
            this.weight = uint.Parse(values[4]);
            this.path = values[5];
            this.buyable = values[6].Equals("1");
            this.collectable = values[7].Equals("1");
            this.description = values[8];
        }
    }

    //Class for items that can be used by the player (i.e. have an interaction to them)
    public class UsableItemData : ItemData  
    {
        public UsableItemData(string[] values, int id) : base(values, id) { }
    }

    //Class for Souls as an Item
    public class SoulItemData : ItemData
    {
        public SoulItemData(string[] values, int id) : base(values, id) { }
    }

    public class KeyItemData : ItemData
    {
        public bool obtained { get; private set; }
        public KeyItemData(string[] values, int id) : base(values, id) { obtained = false; }
        public void MarkObtained() { obtained = true; }
    }
    #endregion

    public static class ItemTable
    {
        const string ITEM_TABLE_PATH = "Assets/Resources/data/item_table.csv";

        private static List<ItemData> itemTable;
        public static Dictionary<int, SoulItemData> soulsDict { get; private set; }
        public static Dictionary<int, UsableItemData> usableDict { get; private set; }
        public static Dictionary<int, ItemData> normalDict { get; private set; }
        public static Dictionary<int, KeyItemData> keyDict { get; private set; }

        public static ItemData GetItemDataFromId(int id)
        {
            if(id > itemTable.Count || id < 0)
            {
                Debug.Log("Could not find item with the id: " + id);
                return null;
            }

            return itemTable[id];
        }

        /// <summary>
        /// Initializes the itemTable from 
        /// </summary>
        public static void Initialize()
        {
            StreamReader reader = new StreamReader(ITEM_TABLE_PATH);
            reader.ReadLine(); //Read the first line, which is header data

            //Populate the item table and relevant lists
            itemTable = new List<ItemData>();
            soulsDict = new Dictionary<int, SoulItemData>();
            usableDict = new Dictionary<int, UsableItemData>();
            normalDict = new Dictionary<int, ItemData>();
            keyDict = new Dictionary<int, KeyItemData>();
            while (!reader.EndOfStream)
                itemTable.Add(ReadItemDataFromFile(reader, itemTable.Count));

            reader.Close();
        }

        static ItemData ReadItemDataFromFile(StreamReader reader, int id = 0)
        {
            string[] values = reader.ReadLine().Split(',');
            ItemData data;

            //Find out what type of item it is
            switch (values[1])
            {
                case "0": //Normal item
                    data = new ItemData(values, id);   
                    normalDict[id] = data;
                    return data;
                case "1": //Usable item
                    data = new UsableItemData(values, id);
                    usableDict[id] = (UsableItemData)data;
                    return data;
                case "2": //Souls item
                    data = new SoulItemData(values, id);
                    soulsDict[id] = (SoulItemData)data;
                    return data;
                case "3": //Key item
                    data = new KeyItemData(values, id);
                    keyDict[id] = (KeyItemData)data;
                    return data;
                default:
                    return new ItemData(values, id);
            }   
        }
    }
}
