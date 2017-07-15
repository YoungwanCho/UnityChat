using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Connection
{
    private Socket _mainSock;

    private Action<byte[]> ReceiveCallBack;
    private Action<byte[]> SendUpdate;

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

        NetworkLibrary.AsyncObject obj = new NetworkLibrary.AsyncObject(4096);
        obj.WorkingSocket = _mainSock;
        _mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
    }

    public void OnSendData(string message)
    {
        //@TODO: 연결 되도 false를 리턴한다
        //if (!_mainSock.IsBound)
        //{
        //    Debug.Log("서버가 실행되고 있지 않습니다.");
        //    return;
        //}

        if (string.IsNullOrEmpty(message))
        {
            Debug.Log("메세지가 입력 되지 않았습니다.");
            return;
        }

        IPEndPoint ip = (IPEndPoint)_mainSock.LocalEndPoint;
        string addr = ip.Address.ToString();

        byte[] bDts = Encoding.UTF8.GetBytes(addr + "\x01" + message);

        _mainSock.Send(bDts);

        if(SendUpdate != null)
        {
            SendUpdate(bDts);
        }
    }

    public void DataReceived(System.IAsyncResult ar)
    {
        NetworkLibrary.AsyncObject obj = (NetworkLibrary.AsyncObject)ar.AsyncState;

        int received = obj.WorkingSocket.EndReceive(ar);

        if (received <= 0)
        {
            obj.WorkingSocket.Close();
            return;
        }

        ReceiveCallBack(obj.Buffer);

        obj.ClearBuffer();

        obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
    }
}
