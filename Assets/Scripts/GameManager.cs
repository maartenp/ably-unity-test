using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IO.Ably;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class GameManager : MonoBehaviour {
	public string API_KEY;
	public Text output;
	public AblyRest restClient;
	public AblyRealtime realtimeClient;

	// Use this for initialization
	void Start () {
		UnitySystemConsoleRedirector.Redirect();
		Connect();
	}

	void Connect() {
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
		restClient = new AblyRest(new ClientOptions(API_KEY) { ClientId = "Host", LogLevel = LogLevel.Debug });
		var token = restClient.Auth.RequestToken(new TokenParams { ClientId = "Host" });

		realtimeClient = new AblyRealtime(new ClientOptions { TokenDetails = token, Tls = false, ClientId = "Host", LogLevel = LogLevel.Debug });

		realtimeClient.Connection.On((change) => ConnectionStateChange(change));
		
	}
	
	void ConnectionStateChange (IO.Ably.Realtime.ConnectionStateChange change) {
		output.text = output.text + "<size=15><b>["+DateTime.Now.ToString("HH:mm:ss")+"] <Connection></b> " +change.Previous+ " -> " + change.Current;
		if (change.Reason != null)
			output.text = output.text + " (Reason: "+change.Reason+")";
		if (change.HasError)
		 	output.text = output.text + " (Error)";

		output.text = output.text + "\n</size>";
	}

	public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
		// Source: https://answers.unity.com/questions/792342/how-to-validate-ssl-certificates-when-using-httpwe.html
		bool isOk = true;
		// If there are errors in the certificate chain, look at each error to determine the cause.
		if (sslPolicyErrors != SslPolicyErrors.None) {
			for(int i=0; i<chain.ChainStatus.Length; i++) {
				if(chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
					bool chainIsValid = chain.Build((X509Certificate2)certificate);
					if(!chainIsValid) {
						isOk = false;
					}
				}
			}
		}
		return isOk;
	}	

	public void OnDestroy() {
		realtimeClient.Close();
	}
}
