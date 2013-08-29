using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Net.Sockets;
using Game.Base;
using Game.Server.Entities;
using Game.Server.Entities.ValueObjects;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace Game.Server
{
    class GamePeer : PeerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GamePeer));
        private readonly Application _application;


        public GamePeer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            Log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            using (var session = _application.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        var opCode = (GameOperationCode)operationRequest.OperationCode;

                        if (opCode == GameOperationCode.Register)
                        {
                            var username = (string)operationRequest.Parameters[(byte)GameOperatorCodeParameter.Username];
                            var password = (string)operationRequest.Parameters[(byte)GameOperatorCodeParameter.Password];
                            var email = (string)operationRequest.Parameters[(byte)GameOperatorCodeParameter.Email];
                            Register(session, username, password, email);
                        }
                        else if (opCode == GameOperationCode.Login)
                        {
                            var password = (string)operationRequest.Parameters[(byte)GameOperatorCodeParameter.Password];
                            var email = (string)operationRequest.Parameters[(byte)GameOperatorCodeParameter.Email];
                            Login(session, password, email);
                        }
                        else if (opCode == GameOperationCode.SendMessage)
                        {
                            var message = (string)operationRequest.Parameters[(byte)GameOperatorCodeParameter.Message];
                            SendMessage(session, message);
                        }
                        else
                        {
                            SendOperationResponse(new OperationResponse((byte)GameOperationResponse.Invalid), sendParameters);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        SendOperationResponse(new OperationResponse((byte)GameOperationResponse.FataError), sendParameters);
                        trans.Rollback();
                        Log.ErrorFormat("Error processing operation {0}:{1}", operationRequest.OperationCode, ex);
                    }
                }
            }
        }

        private void SendMessage(ISession session, string message)
        {

        }

        private void Login(ISession session, string password, string email)
        {
            var user = session.Query<User>().SingleOrDefault(s => s.Email == email);
            if (user == null || !user.Password.EqualsPlainText(password))
            {
                SendError("Email or password is incorrect!");
                return;
            }
            SendSuccess();
        }

        private void Register(ISession session, string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                SendError("All Fields are required!");
                return;
            }

            if (username.Length > 128)
            {
                SendError("Username must be less than 128 characters!");
                return;
            }

            if (email.Length > 200)
            {
                SendError("Email must be less than 200 characters");
                return;
            }

            if (session.Query<User>().Any(t => t.Username == username || t.Email == email))
            {
                SendError("Username and email must be unique!");
                return;
            }

            var user = new User
            {
                Username = username,
                CreatedAt = DateTime.Now,
                Email = email,
                Password = HashedPassword.FromPlainText(password)
            };
            session.Save(user);
            SendSuccess();
        }


        private void SendSuccess()
        {
            SendOperationResponse(new OperationResponse((byte) GameOperationResponse.Success),
                new SendParameters {Unreliable = false});
        }

        private void SendError(string message)
        {
            SendOperationResponse(
                new OperationResponse(
                    (byte)GameOperationResponse.Error,
                    new Dictionary<byte, object>
                    {
                        { (byte)GameOperationResponseParameter.ErrorMessage, message }
                    }), new SendParameters
                    {
                        Unreliable = false
                    });

        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            Log.InfoFormat("Peer disconected: {0}, {1}", reasonCode, reasonDetail);
            _application.DestroyPeer(this);
        }
    }
}
