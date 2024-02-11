using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.Linq;
using Unity.VisualScripting;

namespace Z3roVR.Pun
{
    public class ZPunPlayer : MonoBehaviour
    {
        [Header("Cosmetics")]
        public GameObject[] AllCosmetics;

        private List<GameObject> ActiveCosmetics = new List<GameObject>();

        [HideInInspector]
        public Player owner;

        [Header("Player Stuff")]
        public PhotonView view;

        public Transform Head;
        public Transform LHand;
        public Transform RHand; 

        /// <summary>
        /// Simple awake method.
        /// </summary>
        private void Awake()
        {
            owner = PhotonNetwork.LocalPlayer;

            if (view == null)
            {
                view = GetComponent<PhotonView>();
            }
        }

        /// <summary>
        /// Simple update method.
        /// </summary>
        private void Update()
        {
            PositionMarking();
        }

        /// <summary>
        /// Updates the pos of the head, lhand, and rhand to the local players pos.
        /// </summary>
        private void PositionMarking()
        {
            if (view.IsMine)
            {
                UpdateTransform(Head, Z3roPunManager.instance.playerhead);
                UpdateTransform(LHand, Z3roPunManager.instance.LHand);
                UpdateTransform(RHand, Z3roPunManager.instance.RHand);
            }
        }

        /// <summary>
        /// Sets the transform to a target transform
        /// </summary>
        /// <param name="current">the transform to modify</param>
        /// <param name="targ">the target transform</param>
        private void UpdateTransform(Transform current, Transform targ)
        {
            current.position = targ.position;
            current.rotation = targ.rotation;
        }   

        /// <summary>
        /// Refresh's the cosmetics for this user.
        /// </summary>
        /// <param name="OnCompleted">If it was able to get the user cosmetics and update them.</param>
        /// <returns>Nothing</returns>
        public IEnumerator RefreshUserCosmetics(Action<bool> OnCompleted = null)
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "GetUserCosmeticData",
                FunctionParameter = new
                {
                    PID = owner.UserId.ToString()
                }
            }, delegate (ExecuteCloudScriptResult obj)
            {
                Debug.Log("Got Cosmetics.");
                var jsond = (JsonObject)obj.FunctionResult;
                if (jsond.TryGetValue("ConactList", out object list))
                {
                    Debug.Log("Got List.");

                    string[] cosmeticlist = list.ToString().Split(".");
                    
                    OnReceivedCosmeticsData(cosmeticlist);
                    if (OnCompleted != null)
                    {
                        OnCompleted(true);
                    }
                    return;
                }
                else
                {
                    if (OnCompleted != null)
                    {
                        OnCompleted(false);
                    }
                }
            }, delegate (PlayFabError obj)
            {
                if (OnCompleted != null)
                {
                    OnCompleted(false);
                }
                Debug.LogError(obj.GenerateErrorReport());
            });
            yield break;
        }

        /// <summary>
        /// Updates this users cosmetics
        /// </summary>
        /// <param name="DynamicCosmeticsList">The list of cosmetics</param>
        private void OnReceivedCosmeticsData(string[] DynamicCosmeticsList)
        {
            Debug.Log("Activating Cosmetics...");

            foreach (string cosmetic in DynamicCosmeticsList)
            {
                if (cosmetic != null)
                {
                    EnableCosmetic(cosmetic);
                }
            }
        }

        /// <summary>
        /// Locally enables a specific cosmetic.
        /// </summary>
        /// <param name="name">The cosmetic name.</param>
        private void EnableCosmetic(string name)
        {
            foreach (GameObject f in ActiveCosmetics)
            {
                if (f != null)
                {
                    f.SetActive(false);
                }
            }

            foreach (GameObject f in AllCosmetics)
            {
                if (f != null)
                {
                    if (f.name == name)
                    {
                        f.SetActive(true);
                        ActiveCosmetics.Add(f);
                    }
                }              
            }
        }
    }
}