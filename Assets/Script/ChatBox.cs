using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    private Socket _mainSock;

    private string _textIpAddress = string.Empty;
    private string _textPort = string.Empty;
    private string _textMessage = string.Empty;
    private string _textHistroy = string.Empty;

    public void Awake()
    {
        _mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
    }

    public void OnGUI()
    {
        _textIpAddress = GUI.TextField(new Rect(10, 10, 300, 40), _textIpAddress);
        _textPort = GUI.TextField(new Rect(320, 10, 100, 40), _textPort);

        if (GUI.Button(new Rect(440, 10, 100, 40), "연결"))
        {
            Debug.Log("Click Connect");
            OnConnectToServer();
        }

        GUI.TextArea(new Rect(10, 60, 540, 540), string.Empty);

        GUI.TextArea(new Rect(10, 610, 200, 40), "보낼 텍스트");
        _textMessage = GUI.TextField(new Rect(220, 610, 200, 40), _textMessage);

        if (GUI.Button(new Rect(440, 610, 100, 40), "전송"))
        {
            Debug.Log("Click Connect");
            OnSendData();
        }
    }

    public void OnConnectToServer()
    {
        if (_mainSock.Connected)
        {
            Debug.Log("이미 연결 되어있습니다.");
            return;
        }

        int port;
        if(!int.TryParse(_textPort, out port))
        {
            Debug.Log("포트 번호가 잘못 입력되었거나 입력 되지 않았습니다.");
            return;
        }

        try
        {
            _mainSock.Connect(_textIpAddress, port);
        }
        catch(System.Exception ex)
        {
            Debug.Log(string.Format("연결에 실패 했습니다. 오류 내용 : {0}", ex.Message));
            return;
        }

        MultiChatClient.AsyncObject obj = new MultiChatClient.AsyncObject(4096);
        obj.WorkingSocket = _mainSock;
        _mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
    }

    private void OnSendData()
    {
        if (!_mainSock.IsBound)
        {
            Debug.Log("서버가 실행되고 있지 않습니다.");
            return;
        }

        if (string.IsNullOrEmpty(_textMessage))
        {
            Debug.Log("메세지가 입력 되지 않았습니다.");
            return;


        }

        IPEndPoint ip = (IPEndPoint)_mainSock.LocalEndPoint;
        string addr = ip.Address.ToString();

        byte[] bDts = Encoding.UTF8.GetBytes(addr + "\x01" + _textMessage);

        _mainSock.Send(bDts);

        _textMessage = string.Empty;
    }

    public void DataReceived(System.IAsyncResult ar)
    {
        MultiChatClient.AsyncObject obj = (MultiChatClient.AsyncObject) ar.AsyncState;

        int received = obj.WorkingSocket.EndReceive(ar);

        if(received <= 0)
        {
            obj.WorkingSocket.Close();
            return;
        }

        string text = Encoding.UTF8.GetString(obj.Buffer);

        string[] tokens = text.Split('\x01');
        string ip = tokens[0];
        string msg = tokens[1];

        obj.ClearBuffer();

        obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);

    }

}
