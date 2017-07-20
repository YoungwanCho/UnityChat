using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using NetworkLibrary;

public class Connection
{
    private Socket _mainSock;

    private Action<byte[]> ReceiveCallBack;
    private Action<byte[]> SendUpdate;

    private int _receiveSize = 0;
    private int _packetSize = 0;

    private List<byte> _byteList = new List<byte>();
    private Queue<NetworkLibrary.Packet> _packetQueue = new Queue<NetworkLibrary.Packet>();

    public Connection(AddressFamily family, SocketType type, ProtocolType proto, Action<Byte[]> receiveCallBack, Action<Byte[]> sendUpdate)
    {
        _mainSock = new Socket(family, type, proto);
        this.ReceiveCallBack = receiveCallBack;
        this.SendUpdate = sendUpdate;
    }

    public void OnConnectToServer(string ipstr, string portstr)
    {
        if (_mainSock.Connected)
        {
            Debug.Log("이미 연결 되어있습니다.");
            return;
        }

        int port;
        if (!int.TryParse(portstr, out port))
        {
            Debug.Log("포트 번호가 잘못 입력되었거나 입력 되지 않았습니다.");
            return;
        }

        try
        {
            _mainSock.Connect(ipstr, port);
        }
        catch (System.Exception ex)
        {
            Debug.Log(string.Format("연결에 실패 했습니다. 오류 내용 : {0}", ex.Message));
            return;
        }

        AsyncObject obj = new AsyncObject(1024);
        obj.WorkingSocket = _mainSock;
        _mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, StreamReceive, obj);
    }

    public void OnSendData(string message)
    {
        Debug.Log("OnSendData");
        PacketUserInfo sendPacket = new PacketUserInfo((int)PacketType.USER_INFO);
        sendPacket.InitPacketUserInfo();
        byte[] buff = sendPacket.ToBytes();
        _mainSock.Send(buff);

        if (SendUpdate != null)
        {
            //SendUpdate(buff);
        }
    }

    private void StreamReceive(IAsyncResult ar)
    {
        AsyncObject obj = (AsyncObject)ar.AsyncState;

        try
        {
            int len = obj.WorkingSocket.EndReceive(ar);

            for (int i = 0; i < len; i++)
            {
                _byteList.Add(obj.Buffer[i]);
            }
            obj.ClearBuffer();
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, StreamReceive, obj);
        }
        catch (Exception ex)
        {

        }

        ProcessStreamByte();
    }

    private void ProcessStreamByte()
    {
        if (_byteList.Count < 2) // 패킷사이즈도 알아 낼수 없는 경우
        {
            return;
        }

        if (_packetSize == 0)
        {
            byte[] sizeByte = new byte[2];
            sizeByte[0] = _byteList[0];
            sizeByte[1] = _byteList[1];
            _packetSize = Util.ByteArrToShort(sizeByte, 0);
        }

        if (_byteList.Count < _packetSize) // 필요한 만큼 다 못 받은 경우
        {
            return;
        }
        else
        {
            byte[] packetByte = new byte[_packetSize];

            for (int i = 0; i < _packetSize; i++)
            {
                packetByte[i] = _byteList[i];
            }

            _byteList.RemoveRange(0, _packetSize);
            int packetType = Util.ByteArrToInt(packetByte, 2);

            if (packetType == (int)PacketType.USER_INFO)
            {
                PacketUserInfo userInfo = new PacketUserInfo(packetType);
                userInfo.ToType(packetByte);
                _packetQueue.Enqueue(userInfo);
            }
            _packetSize = 0;
        }
        ProcessPacket();
    }

    private void ProcessPacket()
    {
        if (_packetQueue.Count <= 0)
        {
            return;
        }

        Packet packet = _packetQueue.Dequeue();

        AsyncObject obj = new AsyncObject(1024);

        if (packet.PacketType.n == (int)PacketType.USER_INFO)
        {
            PacketUserInfo userInfo = packet as PacketUserInfo;

            if (userInfo != null)
            {
                ReceiveCallBack(userInfo.ToBytes());
            }
        }
    }

    public void DataReceived(System.IAsyncResult ar)
    {


        
        //NetworkLibrary.AsyncObject obj = (NetworkLibrary.AsyncObject)ar.AsyncState;

        //int received = obj.WorkingSocket.EndReceive(ar);

        //if (received <= 0)
        //{
        //    obj.WorkingSocket.Close();
        //    return;
        //}

        //ReceiveCallBack(obj.Buffer);

        //obj.ClearBuffer();

        //obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 1024, 0, DataReceived, obj);
    }
}
