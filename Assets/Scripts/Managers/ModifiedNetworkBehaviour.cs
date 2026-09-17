using Unity.Netcode;
using UnityEngine;

public class ModifiedNetworkBehaviour : NetworkBehaviour
{
    [Header("Network Logging")]
    [Space(0.5f)] [SerializeField] protected Color objectNameColor;
}
