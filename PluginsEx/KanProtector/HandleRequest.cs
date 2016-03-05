using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectronicObserver.Data;

namespace KanProtector
{
    public class HandleRequest
    {
        const string DestroyShipID = "api.ship.id";
        const string DestroyItemList = "api.slotitem.ids";
        const string PowerUpList = "api.id.items";
        const string LockShipID = "api.ship.id";
        const string LockItemID = "api.slotitem.id";
        public static string OnDestroyShip(string body)
        {
            List<string> shipList = new List<string>();
            List<string> eqList = new List<string>();
            try
            {
                Dictionary<string, string> api = GetAPI(body);
                if (api.ContainsKey(DestroyShipID))
                {
                    int shipID = int.Parse(api[DestroyShipID]);
                    ShipData ship = KCDatabase.Instance.Ships[shipID];
                    if (ProtectionData.Instance.ShipProtectionEnabled && ProtectionData.Instance.isShipProtected(shipID))
                        shipList.Add(ship.NameWithLevel);
                    if (ProtectionData.Instance.EquipmentProtectionEnabled)
                    {
                        foreach (int ItemID in ship.SlotMaster)
                        {
                            if (ProtectionData.Instance.isEquipmentProtected(ItemID))
                            {
                                eqList.Add(KCDatabase.Instance.MasterEquipments[ItemID].Name);
                            }
                        }
                    }
                }

                StringBuilder WarningText = new StringBuilder();
                if (shipList.Count > 0)
                    WarningText.Append("试图拆解被保护的舰娘[" + string.Join("][", shipList.ToArray()) + "]" + Environment.NewLine);
                if (eqList.Count > 0)
                    WarningText.Append("试图拆解的舰娘上存在被保护的装备[" + string.Join("][", eqList.ToArray()) + "]");
                if (shipList.Count > 0 || eqList.Count > 0)
                    return WarningText.ToString();
            }
            catch(Exception e)
            {
                ElectronicObserver.Utility.Logger.Add(3, string.Format("{0}", e.Message + Environment.NewLine + e.StackTrace));
            }
            return null;
        }
        public static string OnDestroyItem(string body)
        {
            List<string> eqList = new List<string>();
            try
            {
                Dictionary<string, string> api = GetAPI(body);
                if (api.ContainsKey(DestroyItemList))
                {
                    string items = api[DestroyItemList];
                    string[] itemsID = items.Split(',');
                    foreach (string Item in itemsID)
                    {
                        int ItemID = int.Parse(Item);
                        int ID = KCDatabase.Instance.Equipments[ItemID].EquipmentID;
                        if (ProtectionData.Instance.isEquipmentProtected(ID))
                        {
                            eqList.Add(KCDatabase.Instance.MasterEquipments[ID].Name);
                        }
                    }
                }

                StringBuilder WarningText = new StringBuilder();
                if (eqList.Count > 0)
                {
                    WarningText.Append("试图拆解的舰娘上存在被保护的装备[" + string.Join("][", eqList.ToArray()) + "]");
                    return WarningText.ToString();
                }
            }
            catch (Exception e)
            {
                ElectronicObserver.Utility.Logger.Add(3, string.Format("{0}", e.Message + Environment.NewLine + e.StackTrace));
            }
            return null;
        }
        public static string OnPowerUp(string body)
        {
            List<string> shipList = new List<string>();
            List<string> eqList = new List<string>();
            try
            {
                Dictionary<string, string> api = GetAPI(body);
                if (api.ContainsKey(PowerUpList))
                {
                    string ships = api[PowerUpList];
                    string[] shipsID = ships.Split(',');
                    foreach (string Ship in shipsID)
                    {
                        int shipID = int.Parse(Ship);
                        var shipdata = KCDatabase.Instance.Ships[shipID];
                        if (ProtectionData.Instance.ShipProtectionEnabled && ProtectionData.Instance.isShipProtected(shipID))
                        {
                            shipList.Add(shipdata.NameWithLevel);
                        }
                        if (ProtectionData.Instance.EquipmentProtectionEnabled)
                        {
                            foreach (int ID in shipdata.SlotMaster)
                            {
                                if (ProtectionData.Instance.isEquipmentProtected(ID))
                                {
                                    eqList.Add(KCDatabase.Instance.MasterEquipments[ID].Name);
                                }
                            }
                        }
                    }
                }
                StringBuilder WarningText = new StringBuilder();
                if (shipList.Count > 0)
                    WarningText.Append("试图拆解被保护的舰娘[" + string.Join("][", shipList.ToArray()) + "]" + Environment.NewLine);
                if (eqList.Count > 0)
                    WarningText.Append("试图拆解的舰娘上存在被保护的装备[" + string.Join("][", eqList.ToArray()) + "]");
                if (shipList.Count > 0 || eqList.Count > 0)
                    return WarningText.ToString();
            }
            catch (Exception e)
            {
                ElectronicObserver.Utility.Logger.Add(3, string.Format("{0}", e.Message + Environment.NewLine + e.StackTrace));
            }
            return null;
        }

        public static string OnLock(string body)
        {
            try
            {
                Dictionary<string, string> api = GetAPI(body);
                if (api.ContainsKey(LockShipID))
                {
                    int shipID = int.Parse(api[LockShipID]);
                    ShipData ship = KCDatabase.Instance.Ships[shipID];
                    if (ship.IsLocked)
                    {
                        if (ship.IsLocked && ProtectionData.Instance.ShipProtectionEnabled && ProtectionData.Instance.isShipProtected(shipID))
                            return "解锁了被保护的舰娘[" + ship.NameWithLevel + "]";
                        //ship.IsLocked = !ship.IsLocked;
                    }
                    if (ProtectionData.Instance.EquipmentProtectionEnabled)
                    {
                        foreach (int ItemID in ship.SlotMaster)
                        {
                            if (ProtectionData.Instance.isEquipmentProtected(ItemID))
                            {
                                return "解锁的舰娘上存在被保护的装备[" + KCDatabase.Instance.MasterEquipments[ItemID].Name + "]";
                            }
                        }
                    }
                }
                if (api.ContainsKey(LockItemID))
                {
                    string item = api[LockItemID];

                    int ItemID = int.Parse(item);
                    int ID = KCDatabase.Instance.Equipments[ItemID].EquipmentID;
                    if (KCDatabase.Instance.Equipments[ItemID].IsLocked && ProtectionData.Instance.isEquipmentProtected(ID))
                    {
                        return "解锁了被保护的装备[" + KCDatabase.Instance.MasterEquipments[ID].Name + "]";
                    }
                    
                }
            }
            catch (Exception e)
            {
                ElectronicObserver.Utility.Logger.Add(3, string.Format("{0}", e.Message + Environment.NewLine + e.StackTrace));
            }
            return null;
        }
        static Dictionary<string, string> GetAPI(string body)
        {
            string str = body.Replace("%5F", ".");
            str = str.Replace("%2C", ",");
            string[] list = str.Split('&');
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string s in list)
            {
                int index = s.IndexOf("=");
                if (index > 3)
                {
                    string key = s.Substring(0, index);
                    string value = s.Substring(Math.Min(index + 1, s.Length));
                    result[key] = value;
                }
            }
            return result;
        }
    }
}
