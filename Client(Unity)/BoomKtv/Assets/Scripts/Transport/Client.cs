using System;
using System.Collections.Generic;
using System.Threading;
using Entity;
using ICelery.Network.Client;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Transport
{
    public class Client : TcpClient
    {
        public static readonly Client Instance = new Client();

        public Action<AuthResult> OnAuthResult;
        public Action<List<User>> OnUserListResult;
        public Action<User> OnUserEnter;
        public Action<User> OnUserExit;
        public Action<byte[]> OnGetRecord;

        private const int HeartbeatTime = 3000;
        private readonly Thread _heartbeatThread;

        private Client()
        {
            OnReceive += OnReceiveEvent;
            OnConnect += OnConnectEvent;
            OnClose += OnCloseEvent;

            _heartbeatThread = new Thread(Heartbeat)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
        }

        private void OnConnectEvent(bool result)
        {
            if (!result) return;
            
            _heartbeatThread.Start();
        }

        private void OnCloseEvent()
        {
            _heartbeatThread.Abort();
        }

        private void Heartbeat()
        {
            while (IsConnected)
            {
                Thread.Sleep(HeartbeatTime);
                DataPacket.Allocate(Opcode.CsHeartbeat).Flush();
            }
        }

        private void OnReceiveEvent(byte[] bytes)
        {
            Debug.Log($"OnReceiveEvent Length={bytes.Length}");
            HandleNetworkPacket(DataPacket.Parse(bytes));
        }

        public void RequestAuth(string uid, string key)
        {
            var packet = DataPacket.Allocate(Opcode.CsAuth);
            packet.WriteString(uid);
            packet.WriteString(key);
            packet.Flush();
        }

        public void RequestUserList()
        {
            var packet = DataPacket.Allocate(Opcode.CsUserList);
            packet.Flush();
        }

        public void SendRecord(byte[] bytes)
        {
            var packet = DataPacket.Allocate(Opcode.CsRecord);
            packet.WriteInt(bytes.Length);
            packet.WriteBytes(bytes);
            packet.Flush();
        }

        private void HandleNetworkPacket(DataPacket packet)
        {
            var opcode = packet.Opcode;
            Debug.Log($"HandleNetworkPacket opcode={opcode}");
            switch (opcode)
            {
                case Opcode.CsAuth:
                    OnAuthResult?.Invoke((AuthResult) packet.ReadShort());
                    break;
                case Opcode.CsUserList:
                    OnUserListResult?.Invoke(ParseInfoResult(packet));
                    break;
                case Opcode.SEnter:
                    OnUserEnter?.Invoke(ParseUser(packet));
                    break;
                case Opcode.SExit:
                    OnUserExit?.Invoke(ParseUser(packet));
                    break;
                case Opcode.CsRecord:
                    OnGetRecord?.Invoke(ParseGetRecord(packet));
                    break;
            }
        }

        private User ParseUser(DataPacket packet)
        {
            return new User
            {
                Uid = packet.ReadString(),
                Name = packet.ReadString()
            };
        }

        private List<User> ParseInfoResult(DataPacket packet)
        {
            var list = new List<User>();
            var len = packet.ReadUShort();
            for (var i = 0; i < len; i++)
            {
                list.Add(ParseUser(packet));
            }

            return list;
        }

        private byte[] ParseGetRecord(DataPacket packet)
        {
            var len = packet.ReadInt();
            var bytes = new byte[len];
            packet.ReadBytes(bytes, 0, len);
            return bytes;
        }
    }
}