using System.Collections.Generic;

namespace MafiaHostAssistant;

public static class RoleList
{
    public static readonly Dictionary<RoleRecord, int> selectedRoles = new();
    public static List<OrderEntry> orderEntries;

    public static void AddRole(RoleRecord role)
    {
        if (selectedRoles.ContainsKey(role))
        {
            selectedRoles[role]++;
        }
        else
        {
            selectedRoles.Add(role, 1);
        }
    }
    
    public static void RemoveRole(RoleRecord role)
    {
        selectedRoles[role]--;
        if (selectedRoles[role] == 0)
        {
            selectedRoles.Remove(role);
        }
    }
}

public abstract class OrderEntry {}

public class ActionOrderEntry : OrderEntry
{
    public string roleName;
    public int actionIndex;
}

public class UnionOrderEntry : OrderEntry
{
    public string unionName;
    public ConfiguredBehaviorRecord wakingAlgorythmRef;
    public List<OrderEntry> entries;
}

public class CombinationOrderEntry : OrderEntry
{
    public string combinationName;
    public List<OrderEntry> entries;
}