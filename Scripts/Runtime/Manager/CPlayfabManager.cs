using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if PLAYFAB_MODULE_ENABLE
using PlayFab;
using PlayFab.ClientModels;

/** 플레이 팹 관리자 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
	/** 콜백 */
	public enum ECallback {
		NONE,
		INIT,
		[HideInInspector] MAX_VAL
	}

	/** 플레이 팹 콜백 */
	private enum EPlayfabCallback {
		NONE = -1,
		LOGIN,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams {
		public Dictionary<ECallback, System.Action<CPlayfabManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private STParams m_stParams;
	private Dictionary<EPlayfabCallback, System.Action<CPlayfabManager, bool>> m_oCallbackDict01 = new Dictionary<EPlayfabCallback, System.Action<CPlayfabManager, bool>>();
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;
	public string UserID { get; private set; } = string.Empty;

	public bool IsLogin {
		get {
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
			return PlayFabClientAPI.IsClientLoggedIn();
#else
			return false;
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog("CPlayfabManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		} else {
			m_stParams = a_stParams;
			this.ExLateCallFunc((a_oSender) => this.OnInit());
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 로그인을 처리한다 */
	public void Login(string a_oDeviceID, System.Action<CPlayfabManager, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.Login: {a_oDeviceID}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EPlayfabCallback.LOGIN, a_oCallback);

			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest() {
				CreateAccount = true, CustomId = a_oDeviceID, TitleId = PlayFabSettings.staticSettings.TitleId
			}, this.OnLogin, this.OnLoginFail);
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
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EPlayfabCallback.LOGIN, a_oCallback);

			PlayFabClientAPI.LoginWithApple(new LoginWithAppleRequest() {
				CreateAccount = true, IdentityToken = a_oIDToken, TitleId = PlayFabSettings.staticSettings.TitleId
			}, this.OnLogin, this.OnLoginFail);
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
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict01.ExReplaceVal(EPlayfabCallback.LOGIN, a_oCallback);

			PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest() {
				CreateAccount = true, AccessToken = a_oAccessToken, TitleId = PlayFabSettings.staticSettings.TitleId
			}, this.OnLogin, this.OnLoginFail);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if (UNITY_IOS || UNITY_ANDROID) && FACEBOOK_MODULE_ENABLE
	}
	
	/** 로그아웃을 처리한다 */
	public void Logout(System.Action<CPlayfabManager> a_oCallback) {
		CFunc.ShowLog("CPlayfabManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			PlayFabSettings.staticPlayer.ClientSessionTicket = string.Empty;
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_LOGOUT_CALLBACK, () => CFunc.Invoke(ref a_oCallback, this));
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	/** 초기화 되었을 경우 */
	private void OnInit() {
		CFunc.ShowLog("CPlayfabManager.OnInit");

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_INIT_CALLBACK, () => {
			this.IsInit = true;
			m_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		});
	}

	/** 로그인 되었을 경우 */
	private void OnLogin(LoginResult a_oResult) {
		CFunc.ShowLog($"CPlayfabManager.OnLogin: {a_oResult.PlayFabId}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_LOGIN_CALLBACK, () => { this.UserID = a_oResult.PlayFabId; m_oCallbackDict01.GetValueOrDefault(EPlayfabCallback.LOGIN)?.Invoke(this, this.IsLogin); });
	}

	/** 로그인에 실패했을 경우 */
	private void OnLoginFail(PlayFabError a_oError) {
		CFunc.ShowLog($"CPlayfabManager.OnLoginFail: {a_oError.ErrorMessage}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_LOGIN_FAIL_CALLBACK, () => { this.UserID = string.Empty; m_oCallbackDict01.GetValueOrDefault(EPlayfabCallback.LOGIN)?.Invoke(this, this.IsLogin); });
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	#endregion			// 조건부 함수
}
#endif			// #if PLAYFAB_MODULE_ENABLE
