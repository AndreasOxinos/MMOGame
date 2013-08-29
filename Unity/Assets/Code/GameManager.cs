using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon;
using Game.Base;
using UnityEngine;

public class GameManager : MonoBehaviour, IPhotonPeerListener
{

    enum GameManagerState
    {
        Form,
        Sending,
        Error,
        Success
    }

    private PhotonPeer _photonPeer;

    private string _registerEmail;
    private string _registerPassword;
    private string _loginEmail;
    private string _loginPassword;
    private string _username;
    private string _error;
    private GameManagerState _state;

    public void Start()
    {
        _photonPeer = new PhotonPeer(this, ConnectionProtocol.Udp);
        if (!_photonPeer.Connect("127.0.0.1:5055", "Game1Photon"))
        {
            Debug.LogError("Could not connect!");
        }


        _registerEmail = "";
        _registerPassword = "";
        _username = "";
        _error = "";
        _loginEmail = "";
        _loginPassword = "";
        _state = GameManagerState.Form;
    }

    public void Update()
    {
        _photonPeer.Service();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Width(800), GUILayout.Height(600));

        if (_state == GameManagerState.Form || _state == GameManagerState.Error)
        {

            GUILayout.Label("Game registration!");

            if (_state == GameManagerState.Error)
            {
                GUILayout.Label(String.Format("Error: {0}", _error));
            }

            GUILayout.Label("Username:");
            _username = GUILayout.TextField(_username);

            GUILayout.Label("Email:");
            _registerEmail = GUILayout.TextField(_registerEmail);

            GUILayout.Label("Password:");
            _registerPassword = GUILayout.TextField(_registerPassword);

            if (GUILayout.Button("Register"))
            {
                Register(_username, _registerPassword, _registerEmail);
            }

            GUILayout.Label("Game Login form:");
            _loginEmail = GUILayout.TextField(_loginEmail);
            _loginPassword = GUILayout.TextField(_loginPassword);
            if (GUILayout.Button("Login"))
            {
                Login(_loginEmail, _loginPassword);
            }
        }
        else if (_state == GameManagerState.Sending)
        {
            GUILayout.Label("Sending...");
        }
        else if (_state == GameManagerState.Success)
        {
            GUILayout.Label("Success!!");
        }
        GUILayout.EndHorizontal();
    }

    private void Login(string loginEmail, string loginPassword)
    {
        _state = GameManagerState.Sending;
        _photonPeer.OpCustom( (byte) GameOperationCode.Login,
            new Dictionary<byte, object>
            {
                {(byte)GameOperatorCodeParameter.Email, loginEmail},
                {(byte)GameOperatorCodeParameter.Password, loginPassword}
                
            }, true);
    }

    private void Register(string username, string password, string email)
    {
        _state = GameManagerState.Sending;
        _photonPeer.OpCustom((byte)GameOperationCode.Register,
            new Dictionary<byte, object>
            {
                {(byte)GameOperatorCodeParameter.Email, email},
                {(byte)GameOperatorCodeParameter.Password, password},
                {(byte)GameOperatorCodeParameter.Username, username}

            }, true);
    }

    private void SendServer(string message)
    {
        _photonPeer.OpCustom(1,
                            new Dictionary<byte, object>()
                            {
                                {0, message}
                            },
                            true);
    }

    public void OnApplicationQuit()
    {
        _photonPeer.Disconnect();
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        var response = (GameOperationResponse)operationResponse.OperationCode;

        if (response == GameOperationResponse.Error)
        {
            _state = GameManagerState.Error;
            _error = (string)operationResponse.Parameters[(byte)GameOperationResponseParameter.ErrorMessage];
        }
        else if (response == GameOperationResponse.FataError || response == GameOperationResponse.Invalid)
        {
            _state = GameManagerState.Error;
            _error = "You broke the server";
        }
        else if (response == GameOperationResponse.Success)
        {
            _state = GameManagerState.Success;
        }

    }

    public void OnStatusChanged(StatusCode statusCode)
    {
    }

    public void OnEvent(EventData eventData)
    {

    }
}
