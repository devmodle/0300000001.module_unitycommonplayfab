using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if PLAYFAB_MODULE_ENABLE
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

/** 플레이 팹 관리자 - 인증 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
	#region 함수
	/** 로그인을 처리한다 */
	public void Login(string a_oDeviceID, System.Action<CPlayfabManager, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.Login: {a_oDeviceID}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oDeviceID.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 초기화 되었을 경우
		if(!m_oBoolDict.GetValueOrDefault(EKey.IS_INIT) || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EPlayfabCallback.LOGIN, a_oCallback);
			
#if !UNITY_EDITOR && UNITY_IOS
			PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest() {
				CreateAccount = true, DeviceId = a_oDeviceID, DeviceModel = SystemInfo.deviceModel, OS = SystemInfo.operatingSystem, TitleId = PlayFabSettings.staticSettings.TitleId
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOGIN, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOGIN, a_oError));
#elif !UNITY_EDITOR && UNITY_ANDROID
			PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest() {
				CreateAccount = true, AndroidDeviceId = a_oDeviceID, AndroidDevice = SystemInfo.deviceModel, OS = SystemInfo.operatingSystem, TitleId = PlayFabSettings.staticSettings.TitleId
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOGIN, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOGIN, a_oError));
#else
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest() {
				CreateAccount = true, CustomId = a_oDeviceID, TitleId = PlayFabSettings.staticSettings.TitleId
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOGIN, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOGIN, a_oError));
#endif			// #if !UNITY_EDITOR && UNITY_IOS
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 애플 로그인을 처리한다 */
	public void LoginWithApple(string a_oUserID, string a_oIDToken, System.Action<CPlayfabManager, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.LoginWithApple: {a_oUserID}, {a_oIDToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oUserID.ExIsValid() && a_oIDToken.ExIsValid());

#if UNITY_IOS && APPLE_LOGIN_ENABLE
		// 초기화 되었을 경우
		if(!m_oBoolDict.GetValueOrDefault(EKey.IS_INIT) || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EPlayfabCallback.LOGIN, a_oCallback);

			PlayFabClientAPI.LoginWithApple(new LoginWithAppleRequest() {
				CreateAccount = true, IdentityToken = a_oIDToken, TitleId = PlayFabSettings.staticSettings.TitleId
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOGIN, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOGIN, a_oError));
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if UNITY_IOS && APPLE_LOGIN_ENABLE
	}

	/** 페이스 북 로그인을 처리한다 */
	public void LoginWithFacebook(string a_oAccessToken, System.Action<CPlayfabManager, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.LoginWithFacebook: {a_oAccessToken}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oAccessToken.ExIsValid());

#if (UNITY_IOS || UNITY_ANDROID) && FACEBOOK_MODULE_ENABLE
		// 초기화 되었을 경우
		if(!m_oBoolDict.GetValueOrDefault(EKey.IS_INIT) || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EPlayfabCallback.LOGIN, a_oCallback);

			PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest() {
				CreateAccount = true, AccessToken = a_oAccessToken, TitleId = PlayFabSettings.staticSettings.TitleId
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOGIN, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOGIN, a_oError));
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FACEBOOK_MODULE_ENABLE
	}
	
	/** 로그아웃을 처리한다 */
	public void Logout(System.Action<CPlayfabManager> a_oCallback) {
		CFunc.ShowLog("CPlayfabManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

		try {
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
			// 로그인 되었을 경우
			if(m_oBoolDict.GetValueOrDefault(EKey.IS_INIT) && this.IsLogin) {
				PlayFabSettings.staticPlayer.ClientSessionTicket = string.Empty;
			}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		} finally {
			CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_LOGOUT_CALLBACK, () => CFunc.Invoke(ref a_oCallback, this));
		}
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	/** 로그인 응답을 처리한다 */
	private void HandleLoginResponse(PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		CFunc.ShowLog($"CPlayfabManager.HandleLoginResponse: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_LOGIN_CALLBACK, () => m_oStrDict.ExReplaceVal(EKey.USER_ID, a_bIsSuccess ? (a_oResult as LoginResult).PlayFabId : string.Empty));
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	#endregion			// 조건부 함수
}
#endif			// #if PLAYFAB_MODULE_ENABLE
