using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if PLAYFAB_MODULE_ENABLE
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

/** 플레이 팹 관리자 - 분석 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
#region 함수
	/** 로그를 전송한다 */
	public void SendLog(string a_oName, Dictionary<string, object> a_oDataDict) {
		CFunc.ShowLog($"CPlayfabManager.SendLog: {a_oName}, {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);

#if((UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE) && PLAYFAB_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
		// 로그인 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT] && this.IsLogin) {
			PlayFabClientAPI.WriteTitleEvent(new WriteTitleEventRequest() {
				EventName = a_oName, Body = a_oDataDict
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.SEND_LOG, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.SEND_LOG, a_oError));
		}
#endif // #if ((UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE) && PLAYFAB_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
	}

	/** 유저 로그를 전송한다 */
	public void SendUserLog(string a_oName, Dictionary<string, object> a_oDataDict) {
		CFunc.ShowLog($"CPlayfabManager.SendUserLog: {a_oName}, {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);

#if((UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE) && PLAYFAB_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
		// 로그인 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT] && this.IsLogin) {
			PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest() {
				EventName = a_oName, Body = a_oDataDict
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.SEND_USER_LOG, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.SEND_USER_LOG, a_oError));
		}
#endif // #if((UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE) && PLAYFAB_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
	}

	/** 캐릭터 로그를 전송한다 */
	public void SendCharacterLog(string a_oName, string a_oCharacterID, Dictionary<string, object> a_oDataDict) {
		CFunc.ShowLog($"CPlayfabManager.SendCharacterLog: {a_oName}, {a_oCharacterID}, {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);

#if((UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE) && PLAYFAB_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
		// 로그인 되었을 경우
		if(m_oBoolDict[EKey.IS_INIT] && this.IsLogin) {
			PlayFabClientAPI.WriteCharacterEvent(new WriteClientCharacterEventRequest() {
				EventName = a_oName, CharacterId = a_oCharacterID, Body = a_oDataDict
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.SEND_CHARACTER_LOG, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.SEND_CHARACTER_LOG, a_oError));
		}
#endif // #if ((UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE) && PLAYFAB_ANALYTICS_ENABLE) && (ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD)
	}
#endregion // 함수

#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	/** 로그 전송 응답을 처리한다 */
	private void HandleSendLogResponse(PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		CFunc.ShowLog($"CPlayfabManager.HandleSendLogResponse: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
	}

	/** 유저 로그 전송 응답을 처리한다 */
	private void HandleSendUserLogResponse(PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		CFunc.ShowLog($"CPlayfabManager.HandleSendUserLogResponse: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
	}

	/** 캐릭터 로그 전송 응답을 처리한다 */
	private void HandleSendCharacterLogResponse(PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		CFunc.ShowLog($"CPlayfabManager.HandleSendCharacterLogResponse: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
	}
#endif // #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
#endregion // 조건부 함수
}
#endif // #if PLAYFAB_MODULE_ENABLE
