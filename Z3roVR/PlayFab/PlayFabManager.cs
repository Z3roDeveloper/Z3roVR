using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Z3roVR.Security;
using Photon.Pun;
using System.Runtime.CompilerServices;
using Z3roVR.Pun;

namespace Z3roVR.PlayFab
{
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager instance;

        public string PlayFabId;

        /// <summary>
        /// Simple awake method.
        /// </summary>
        private void Awake()
        {
            if (instance == null && instance != this)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("Cannot have 2 instances of PlayFabManager!");
                Object.Destroy(this);
                return;
            }
            _AuthenticateWithPlayFab();
        }

        /// <summary>
        /// Authenticates the user with playfab, logs them in using custom id.
        /// </summary>
        private void _AuthenticateWithPlayFab()
        {
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier, // don't ever actually use the dui
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            }, OnUserLoggedIn, OnErrorGlobal);
        }

        /// <summary>
        /// Invoked when the user is logged in.
        /// </summary>
        /// <param name="obj">The login result</param>
        private void OnUserLoggedIn(LoginResult obj)
        {
            Debug.Log($"Login success, pfid: {obj.PlayFabId}");
            PlayFabId = obj.PlayFabId;

            if (obj.NewlyCreated)
            {
                SetDisplayName($"MONKE" + Random.Range(1111, 4444));
                Debug.Log("Attempted to update the display name");
            }

            PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest
            {
                PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
            }, AuthenticateWithPhoton, OnErrorGlobal);
        }

        /// <summary>
        /// Authenticates the user with photon.
        /// </summary>
        /// <param name="obj">The GetPhotonAuthenticationTokenResult</param>
        private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
        {
            Debug.Log("Got photon authentication token, Authenticating..");

            object[] AuthParams = new object[2];
            object[] AuthValues = new object[2];

            AuthParams[0] = "username";
            AuthValues [0] = PlayFabId;

            AuthParams[1] = "token";
            AuthValues[1] = obj.PhotonCustomAuthenticationToken;

            Z3roPunManager.instance.InitialConnectionAuthenticated(AuthParams, AuthValues);
        }

        /// <summary>
        /// On PlayFab Error stuff
        /// </summary>
        /// <param name="obj">The PlayFabError</param>
        public void OnErrorGlobal(PlayFabError obj)
        {
            Debug.LogError(obj.GenerateErrorReport());
        }

        /// <summary>
        /// Updates the user displayname
        /// </summary>
        /// <param name="newName">Their new displayname</param>
        public void SetDisplayName(string newName)
        {
            Debug.Log("Setting display name");

            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = newName
            }, delegate (UpdateUserTitleDisplayNameResult obj)
            {
                Debug.Log("Successfully updated user display name!");
            }, null);
        }
    }
}