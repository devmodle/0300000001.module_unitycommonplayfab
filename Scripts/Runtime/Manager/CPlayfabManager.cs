using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if PLAYFAB_MODULE_ENABLE
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

/** 플레이 팹 관리자 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
	/** 식별자 */
	private enum EKey {
		NONE = -1,
		IS_INIT,
		USER_ID,
		SERVER_TIME,
		[HideInInspector] MAX_VAL
	}

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

		LOAD_DATAS,
		LOAD_NOTICES,
		LOAD_LEADERBOARD,
		LOAD_SERVER_TIME,

		LOAD_USER_DATAS,
		LOAD_USER_ITEMS,
		LOAD_USER_CHARACTERS,

		LOAD_CHARACTER_DATAS,
		LOAD_CHARACTER_ITEMS,

		SAVE_USER_DATAS,
		SAVE_CHARACTER_DATAS,

		SEND_LOG,
		SEND_USER_LOG,
		SEND_CHARACTER_LOG,

		BUY_USER_ITEM,
		BUY_USER_CHARACTER,
		BUY_CHARACTER_ITEM,

		ADD_NUM_ITEMS,
		ADD_NUM_CHARACTER_ITEMS,

		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public partial struct STParams {
		public Dictionary<ECallback, System.Action<CPlayfabManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private STParams m_stParams;

	private Dictionary<EKey, bool> m_oBoolDict = new Dictionary<EKey, bool>() {
		[EKey.IS_INIT] = false
	};

	private Dictionary<EKey, string> m_oStrDict = new Dictionary<EKey, string>() {
		[EKey.USER_ID] = string.Empty
	};

	private Dictionary<EKey, System.DateTime> m_oTimeDict = new Dictionary<EKey, System.DateTime>() {
		[EKey.SERVER_TIME] = System.DateTime.Now
	};
	
	private Dictionary<EPlayfabCallback, System.Action<CPlayfabManager, bool>> m_oCallbackDict01 = new Dictionary<EPlayfabCallback, System.Action<CPlayfabManager, bool>>();
	private Dictionary<EPlayfabCallback, System.Action<CPlayfabManager, PlayFabResultCommon, bool>> m_oCallbackDict02 = new Dictionary<EPlayfabCallback, System.Action<CPlayfabManager, PlayFabResultCommon, bool>>();
	private Dictionary<EPlayfabCallback, System.Action<PlayFabResultCommon, bool>> m_oResponseHandlerDict = new Dictionary<EPlayfabCallback, System.Action<PlayFabResultCommon, bool>>();
	#endregion			// 변수

	#region 프로퍼티
	public bool IsLogin {
		get {
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
			return PlayFabClientAPI.IsClientLoggedIn();
#else
			return false;
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		}
	}

	public bool IsInit => m_oBoolDict[EKey.IS_INIT];
	public string UserID => m_oStrDict[EKey.USER_ID];

	public System.DateTime ServerTime => m_oTimeDict[EKey.SERVER_TIME];
	public System.DateTime PSTServerTime => m_oTimeDict[EKey.SERVER_TIME].ExToPSTTime();
	public System.DateTime UTCServerTime => m_oTimeDict[EKey.SERVER_TIME].ToUniversalTime();
	#endregion			// 프로퍼티

	#region 함수
	/** 초기화 */
	public override void Awake() {
		base.Awake();

		m_oResponseHandlerDict.TryAdd(EPlayfabCallback.LOGIN, this.HandleLoginResponse);
		m_oResponseHandlerDict.TryAdd(EPlayfabCallback.SEND_LOG, this.HandleSendLogResponse);
		m_oResponseHandlerDict.TryAdd(EPlayfabCallback.SEND_USER_LOG, this.HandleSendUserLogResponse);
		m_oResponseHandlerDict.TryAdd(EPlayfabCallback.SEND_CHARACTER_LOG, this.HandleSendCharacterLogResponse);
		m_oResponseHandlerDict.TryAdd(EPlayfabCallback.LOAD_SERVER_TIME, this.HandleLoadServerTimeResponse);
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog("CPlayfabManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 초기화 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT]) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, m_oBoolDict[EKey.IS_INIT]);
		} else {
			m_stParams = a_stParams;
			this.ExLateCallFunc((a_oSender) => this.OnInit());
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 공지 사항을 로드한다 */
	public void LoadNotices(System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback, int a_nNumNotices = KCDefine.U_MAX_NUM_PLAYFAB_M_NOTICES) {
		CFunc.ShowLog($"CPlayfabManager.LoadNotices: {a_nNumNotices}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT] && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_NOTICES, a_oCallback);

			PlayFabClientAPI.GetTitleNews(new GetTitleNewsRequest() {
				Count = Mathf.Clamp(a_nNumNotices, KCDefine.B_VAL_1_INT, KCDefine.U_MAX_NUM_PLAYFAB_M_NOTICES)
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_NOTICES, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_NOTICES, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 리더보드를 로드한다 */
	public void LoadLeaderboard(string a_oStatisticsName, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback, int a_nSrcIdx = KCDefine.B_VAL_0_INT, int a_nNumStatistics = KCDefine.U_MAX_NUM_PLAYFAB_M_STATISTICS) {
		CFunc.ShowLog($"CPlayfabManager.LoadLeaderboard: {a_nSrcIdx}, {a_nNumStatistics}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT] && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_LEADERBOARD, a_oCallback);

			PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest() {
				StatisticName = a_oStatisticsName, StartPosition = a_nSrcIdx, MaxResultsCount = Mathf.Clamp(a_nNumStatistics, KCDefine.B_VAL_1_INT, KCDefine.U_MAX_NUM_PLAYFAB_M_STATISTICS)
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_LEADERBOARD, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_LEADERBOARD, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 서버 시간을 로드한다 */
	public void LoadServerTime(System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog("CPlayfabManager.LoadServerTime", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT] && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_SERVER_TIME, a_oCallback);

			PlayFabClientAPI.GetTime(new GetTimeRequest() {
				// Do Something
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_SERVER_TIME, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_SERVER_TIME, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	/** 초기화 되었을 경우 */
	private void OnInit() {
		CFunc.ShowLog("CPlayfabManager.OnInit", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_INIT_CALLBACK, () => {
			m_oBoolDict[EKey.IS_INIT] = true;
			m_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, m_oBoolDict[EKey.IS_INIT]);
		});
	}

	/** 응답을 수신했을 경우 */
	private void OnReceiveResponse(EPlayfabCallback a_eCallback, PlayFabResultCommon a_oResult) {
		CFunc.ShowLog($"CPlayfabManager.OnReceiveResponse: {a_eCallback}, {a_oResult?.ToJson()}", KCDefine.B_LOG_COLOR_PLUGIN);
		this.HandleResponse(a_eCallback, a_oResult, true);

		m_oCallbackDict01.GetValueOrDefault(a_eCallback)?.Invoke(this, true);
		m_oCallbackDict02.GetValueOrDefault(a_eCallback)?.Invoke(this, a_oResult, true);
	}

	/** 응답 수신에 실패했을 경우 */
	private void OnReceiveFailResponse(EPlayfabCallback a_eCallback, PlayFabError a_oError) {
		CFunc.ShowLog($"CPlayfabManager.OnReceiveFailResponse: {a_eCallback}, {a_oError.ErrorMessage}", KCDefine.B_LOG_COLOR_PLUGIN);
		this.HandleResponse(a_eCallback, null, false);

		m_oCallbackDict01.GetValueOrDefault(a_eCallback)?.Invoke(this, false);
		m_oCallbackDict02.GetValueOrDefault(a_eCallback)?.Invoke(this, null, false);
	}

	/** 응답을 처리한다 */
	private void HandleResponse(EPlayfabCallback a_eCallback, PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		m_oResponseHandlerDict.GetValueOrDefault(a_eCallback)?.Invoke(a_oResult, a_bIsSuccess);
	}

	/** 서버 시간 로드 응답을 처리한다 */
	private void HandleLoadServerTimeResponse(PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		CFunc.ShowLog($"CPlayfabManager.HandleLoadServerTimeResponse: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_PLAYFAB_M_LOAD_SERVER_TIME_CALLBACK, () => m_oTimeDict[EKey.SERVER_TIME] = a_bIsSuccess ? (a_oResult as GetTimeResult).Time.ToLocalTime() : System.DateTime.Now);
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	#endregion			// 조건부 함수
}
#endif			// #if PLAYFAB_MODULE_ENABLE
