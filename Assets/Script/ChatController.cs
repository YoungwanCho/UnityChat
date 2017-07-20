using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using NetworkLibrary;

public class ChatController : MonoBehaviour
{
    [SerializeField]
    private ChatWindow _view = null; // 인스펙터에서 할당
    private Connection _model = null;

    public void Awake()
    {
        _model = new Connection(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP, this.ReceiveCallBack, this.SendUpdate);
        _view.Initialize(_model.OnConnectToServer, _model.OnSendData);
    }

    public void Start()
    {
        //TestPacket();
    }

    public void ReceiveCallBack(Packet packet)
    {

        //PacketUserInfo userInfo = new PacketUserInfo((int)PacketType.USER_INFO);
        ////userInfo.ToType(buff);

        SendUpdate(packet.ToString());
    }

    public void SendUpdate(string msg)
    {
        _view.AppendText("서버로부터 받음", msg);
    }

    private void TestPacket()
    {
        PacketUserInfo sendPacket = new PacketUserInfo((int)PacketType.USER_INFO);
        sendPacket.InitPacketUserInfo();

        Debug.Log(sendPacket.ToString());
        byte[] buff = sendPacket.ToBytes();

        PacketUserInfo receivePacket = new PacketUserInfo((int)PacketType.USER_INFO);
        receivePacket.ToType(buff);
        Debug.Log(receivePacket.ToString());
    }

    private void PrintByteArray(byte[] buff)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < buff.Length; i++)
        {
            sb.Append(buff[i]);
        }
        Debug.Log(sb.ToString());
    }

}
