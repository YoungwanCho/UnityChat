using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;


public class ChatWindow : MonoBehaviour
{
    private string _textIpAddress = string.Empty;
    private string _textPort = string.Empty;
    private string _textMessage = string.Empty;
    private string _textHistroy = string.Empty;

    private System.Action<string, string> OnConnectToServer = null;
    private System.Action<string>  OnSendData = null;

    public void Initialize(System.Action<string, string> onConnectToServer, System.Action<string> onSendData)
    {
        this.OnConnectToServer = onConnectToServer;
        this.OnSendData = onSendData;
        _textIpAddress = GetDefaultIPAdress();
        _textPort = "15000"; 
    }

    public void OnGUI()
    {
        _textIpAddress = GUI.TextField(new Rect(10, 10, 300, 40), _textIpAddress);
        _textPort = GUI.TextField(new Rect(320, 10, 100, 40), _textPort);

        if (GUI.Button(new Rect(440, 10, 100, 40), "연결"))
        {
            Debug.Log("Click Connect");
            OnConnectToServer(_textIpAddress, _textPort);
        }

        _textHistroy = GUI.TextArea(new Rect(10, 60, 540, 540), _textHistroy);

        GUI.TextArea(new Rect(10, 610, 200, 40), "보낼 텍스트");
        _textMessage = GUI.TextField(new Rect(220, 610, 200, 40), _textMessage);

        if (GUI.Button(new Rect(440, 610, 100, 40), "전송"))
        {
            Debug.Log("Click Connect");
            //OnSendData(_textMessage);

            //OnSendData("");
            StartCoroutine("Test");
            _textMessage = string.Empty;
        }
    }

    IEnumerator Test()
    {
        for(;;)
        {
            OnSendData("");
            yield return null;
        }
    }

    public void AppendText(string ip, string message)
    {
        StringBuilder sb = new StringBuilder(_textHistroy);
        sb.Append(string.Format("{0} : {1}{2}", ip, message, System.Environment.NewLine));
        _textHistroy = message; //sb.ToString();
    }

    private string GetDefaultIPAdress()
    {
        IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());

        // 처음으로 발견되는 ipv4 주소를 사용한다.
        IPAddress defaultHostAddress = null;
        foreach (IPAddress addr in he.AddressList)
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                defaultHostAddress = addr;
                break;
            }
        }

        // 주소가 없다면..
        if (defaultHostAddress == null)
            // 로컬호스트 주소를 사용한다.
            defaultHostAddress = IPAddress.Loopback;

        return defaultHostAddress.ToString();
    }

}
