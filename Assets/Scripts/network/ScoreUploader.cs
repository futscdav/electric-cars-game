using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class ScoreUploader {

	private const string Server = "k333stu1.felk.cvut.cz";
	private const int Port = 4639;
	private TcpClient tcp = null;

	public ScoreUploader() {
		
	}

	public bool SendScore(string user, int score) {
		if (!OpenSocket()) {
			return false;
		}
		NetworkStream stream = tcp.GetStream();
		string dataToSend = string.Format("Score for: \"{0}\" = \"{1}\"", user, score);
		Debug.Log("Sending: \"" + dataToSend + "\"");
		byte[] raw = Translate(dataToSend);
		stream.Write(raw, 0, raw.Length);

		//receive response
		tcp.ReceiveTimeout = 3000;
		byte[] rawResponse = new byte[256];
		bool success = false;
		try {
			stream.Read(rawResponse, 0, rawResponse.Length);
			string response = System.Text.Encoding.ASCII.GetString(rawResponse);
			Debug.Log("Received: \"" + response + "\"" + "(length = " + rawResponse.Length + ")");
			success = true;
		} catch (System.IO.IOException except) {
			Debug.Log(except.Message);
			//read timeout
			Debug.Log("Receive timeout.");
		}
		CloseSocket();
		return success;
	}

	private bool OpenSocket() {
		if (tcp == null) {
			try {
				tcp = new TcpClient(Server, Port);
			} catch (System.Net.Sockets.SocketException except) {
				Debug.Log(except.Message);
				return false;
			}
		}
		return tcp.Connected;
	}

	private void CloseSocket() {
		tcp.Close();
		tcp = null;
	}

	private byte[] Translate(string message) {
		byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
		return data;
	}

}
