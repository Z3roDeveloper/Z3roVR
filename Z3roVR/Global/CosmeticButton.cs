using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Z3roVR.PlayFab;

namespace Z3roVR.Global
{
    public class CosmeticButton : MonoBehaviour
    {
        public string CosmeticName;
        public bool IsEnableButton;

        public string TagToCompare = "HandTag"; // The tags most of these fangames use

        /// <summary>
        /// Simple OnTriggerEnter
        /// </summary>
        /// <param name="other">the collider</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(TagToCompare))
            {
                UpdateUserData(IsEnableButton);
            }
        }

        /// <summary>
        /// Updates the users cosmetic data of their currently active cosmetics.
        /// </summary>
        /// <param name="enabled">If the cosmetic is enabled</param>
        private void UpdateUserData(bool enabled)
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "UpdateUserCosmeticData",
                FunctionParameter = new
                {
                    ItemToUpdate = CosmeticName,
                    Enabled = enabled,
                    PID = PlayFabManager.instance.PlayFabId
                }
            }, delegate (ExecuteCloudScriptResult obj)
            {
                Debug.Log("Updated my data.");
                RequestServerUpdate();
            }, delegate (PlayFabError obj)
            {
                Debug.LogError(obj.GenerateErrorReport());
            });
        }

        /// <summary>
        /// Sends the event for all the clients to update this users cosmetics
        /// </summary>
        private void RequestServerUpdate()
        {
            object[] eventcontext = new object[1];
            eventcontext[0] = "non";
            PhotonNetwork.RaiseEvent(166, eventcontext, RaiseEventOptions.Default, SendOptions.SendReliable);
            Debug.Log("Send cosmetic update request");
        }
    }
}