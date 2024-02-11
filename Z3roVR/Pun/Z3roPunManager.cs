using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Object = UnityEngine.Object;
using TMPro;
using Unity.VisualScripting;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using UnityEditor.EditorTools;

namespace Z3roVR.Pun
{
    public class Z3roPunManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static Z3roPunManager instance;

        [Header("Network Settings")]
        [SerializeField]
        [Tooltip("The Version of the game")]
        private string GameVersion;
        [Tooltip("How many players can be in a lobby at once")]
        [SerializeField]
        private int MaxPlayers;

        [Tooltip("The location of the network player prefab")]
        [SerializeField]
        private string PlayerLocation = "Prefabs/NetworkPlayerPrefab";

        [Header("Player Stuff")]
        public Transform playerhead;
        public Transform LHand;
        public Transform RHand;

        private GameObject playerref;

        private List<ZPunPlayer> CosmeticLookupPlayers = new List<ZPunPlayer>();

        private int CurrentLookupIndex = 0;

        /// <summary>
        /// Simple Awake method.
        /// </summary>
        private void Awake()
        {
            if (instance == null && instance != this)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("Cannot have 2 or more instances of Z3roPunManager!");
                Object.Destroy(this);
                return;
            }
            StartCoroutine(InfrequentRuns());
        }
        
        /// <summary>
        /// Runs the code to refresh all players cosmetics every 5 minutes
        /// </summary>
        /// <returns>Nothing</returns>
        private IEnumerator InfrequentRuns()
        {
            while (PhotonNetwork.InRoom && CosmeticLookupPlayers.Count > 0)
            {
                if (CurrentLookupIndex < CosmeticLookupPlayers.Count)
                {
                    yield return StartCoroutine(CosmeticLookupPlayers[CurrentLookupIndex]?.RefreshUserCosmetics());

                    CurrentLookupIndex++;
                }
                else
                {
                    CurrentLookupIndex = 0;
                }

                yield return new WaitForSeconds(5 * 60);
            }
        }

        /// <summary>
        /// Connects to the server with authentication.
        /// </summary>
        /// <param name="AuthParams">The paramaters to send to the server</param>
        /// <param name="AuthValues">The values to use for the paramaters</param>
        public void InitialConnectionAuthenticated(object[] AuthParams, object[] AuthValues)
        {
            var auth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

            for (int i = 0; i < AuthParams.Length; i++)
            {
                if (AuthParams[i] != null)
                {
                    auth.AddAuthParameter(AuthParams[i].ToString(), AuthValues[i].ToString());
                }
            }

            PhotonNetwork.AuthValues = auth;

            Debug.Log("Set Authentication Values.");

            InitialConnection();
        }
        
        /// <summary>
        /// Simple connection to the server recommened to use authentication
        /// </summary>
        public void InitialConnection()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// Invoked when connected to the master server.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master!, ready for things.");
        }

        /// <summary>
        /// Invoked when disconnected from the server, auto reconnects to it can be used as a way to leave the room.
        /// </summary>
        /// <param name="cause">The disconnect cause</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"Disconnected {cause.ToString()}, Reconnecting...");
            DisconnectCleanup();
            InitialConnection();
        }

        /// <summary>
        /// Invoked when a user joins a room.
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined Room {PhotonNetwork.CurrentRoom.Name} {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");

            ShouldDisconnectFromRoom();

            playerref = PhotonNetwork.Instantiate(PlayerLocation, base.transform.position, base.transform.rotation);

            foreach (var ff in FindObjectsOfType<ZPunPlayer>())
            {
                if (ff != null)
                {
                   AddToCosmeticsLookup(ff);
                }
            }

            StartCoroutine(RefreshCosmeticsForAllPlayers());
        }

        /// <summary>
        /// Invoked when a new player enters the room.
        /// </summary>
        /// <param name="newPlayer">The newplayer who joined</param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateCosmeticsLookupCheck(newPlayer.ActorNumber);

            Debug.Log("New player joined room");

            ActorNumToPlayerRef(newPlayer.ActorNumber, out bool s, out ZPunPlayer player);

            if (s)
            {
                if (player != null)
                {
                    AddToCosmeticsLookup(player);
                    StartCoroutine(RefreshCosmeticsForAllPlayers());
                    return;
                }
            }

            UpdateCosmeticsLookupCheck(newPlayer.ActorNumber);
        }

        /// <summary>
        /// Invoked when the player leaves the room.
        /// </summary>
        public override void OnLeftRoom()
        {
            Debug.Log("Left Room.");
            PhotonNetwork.Destroy(playerref);
            DisconnectCleanup();
        }

        /// <summary>
        /// Checks that the player count of the room is higher than max players.
        /// </summary>
        public void ShouldDisconnectFromRoom()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > MaxPlayers)
            {
                PhotonNetwork.Disconnect();
            }
        }

        /// <summary>
        /// Cleans up values after disconnecting.
        /// </summary>
        private void DisconnectCleanup()
        {
            CosmeticLookupPlayers.Clear();
            playerref = null;
        }

        /// <summary>
        /// Refreshs all the cosmetics for the players in the room. (locally)
        /// </summary>
        /// <returns>Nothing</returns>
        IEnumerator RefreshCosmeticsForAllPlayers()
        {
            foreach (var ff in FindObjectsOfType<ZPunPlayer>())
            {
                if (ff != null)
                {
                    yield return StartCoroutine(ff.RefreshUserCosmetics());
                }
            }
        }

        /// <summary>
        /// Adds a player to the cosmetics lookup.
        /// </summary>
        /// <param name="player"></param>
        public void AddToCosmeticsLookup(ZPunPlayer player)
        {
            if (CosmeticLookupPlayers.Contains(player))
            {
                return;
            }

            if (CosmeticLookupPlayers.Count > MaxPlayers)
            {
                PhotonNetwork.Disconnect();
                return;
            }

            CosmeticLookupPlayers.Add(player);
        }

        /// <summary>
        /// Refreshs the cosmetics for a specific player.
        /// </summary>
        /// <param name="player">The player to run the refresh on</param>
        private void RefreshCosmetics(ZPunPlayer player)
        {
            if (player != null)
            {
                Debug.Log($"Starting lookup for {player.owner.NickName}");
                
                player.RefreshUserCosmetics(delegate (bool Success)
                {
                    if (Success)
                    {
                        Debug.Log("Enabled User Cosmetics!");
                        return;
                    }
                    Debug.Log("Failed to enable user cosmetics.");
                });
            }
        }

        /// <summary>
        /// Invoked when the client gets an event from the server or another player.
        /// </summary>
        /// <param name="data">The event data</param>
        public void OnEvent(EventData data)
        {
		    switch (data.Code)
            {
                case 166: // Cosmetics Refresh
                {
                    Debug.Log("Cosmetic Refresh asked by player.");
                    var playerf = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender);
                    if (playerf != null)
                    {
                        Debug.Log($"Refresh asked by: {playerf.NickName}");
                        ActorNumToPlayerRef(playerf.ActorNumber, out bool succ, out ZPunPlayer target);
                        if (succ)
                        {
                            Debug.Log("Found player for lookup.");
                            if (target != null)
                            {
                                RefreshCosmetics(target);
                            }
                            else
                            {
                                Debug.Log("Player requested refresh, but doesnt exist, what happened.");
                                UpdateCosmeticsLookupCheck(data.Sender);
                            }
                        }
                        else
                        {
                            Debug.Log("Player requested refresh, but doesnt exist, player left?");
                            UpdateCosmeticsLookupCheck(playerf.ActorNumber);
                        }
                    }
                    else
                    {
                        Debug.Log("Player requested refresh, but doesnt exist, player left?");
                        UpdateCosmeticsLookupCheck(data.Sender);
                    }
                    break;
                }
            }
	    }

        /// <summary>
        /// Gets a player ZPunPlayer class by their actor number.
        /// </summary>
        /// <param name="actornum">The players actor number</param>
        /// <param name="success">if it was a success,</param>
        /// <param name="outplayer">if success, the players ZPunPlayer class</param>
        private void ActorNumToPlayerRef(int actornum, out bool success, out ZPunPlayer outplayer)
        {
            outplayer = null;
            success = false;
            foreach (var f in FindObjectsOfType<ZPunPlayer>())
            {
                if (f != null)
                {
                    if (f.owner.ActorNumber == actornum)
                    {
                        success = true;
                        outplayer = f;
                    }
                }
            }
        }

        /// <summary>
        /// Ensures no null entries are in the cosmetics lookup
        /// </summary>
        /// <param name="num">Unused for now, but a specifc actor number to check is null</param>
        private void UpdateCosmeticsLookupCheck(int num)
        {
            foreach (var f in CosmeticLookupPlayers)
            {
                if (f != null)
                {
                    if (f.owner != null)
                    {
                        return;
                    } 
                    else
                    {
                        CosmeticLookupPlayers.Remove(f);
                    }
                }
                else
                {
                    CosmeticLookupPlayers.Remove(f);
                }
            }
        }
    }
}