using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public static class NameRetriever {

	public static string GetName() {
		string accountName;
		#if UNITY_ANDROID
		AndroidJavaClass jc_unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo_Activity = jc_unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaClass jc_AccountManager = new AndroidJavaClass("android.accounts.AccountManager");
		AndroidJavaObject jo_AccountManager = jc_AccountManager.CallStatic<AndroidJavaObject>("get", jo_Activity);
		AndroidJavaObject jo_Accounts = jo_AccountManager.Call<AndroidJavaObject>("getAccountsByType", "com.google");
		// convert java accounts into array
		AndroidJavaObject[] jo_AccountsArr = AndroidJNIHelper.ConvertFromJNIArray<AndroidJavaObject[]>(jo_Accounts.GetRawObject());
		if (jo_AccountsArr.Length > 0)
			accountName = jo_AccountsArr[0].Get<string>("name");
		else
			accountName = "UnknownAccount";
		return accountName;
		#endif
		return "TEST"+(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
	}

}