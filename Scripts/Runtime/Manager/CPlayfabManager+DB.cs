using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if PLAYFAB_MODULE_ENABLE
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

/** 플레이 팹 관리자 - 데이터 베이스 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	/** 아이템을 구입한다 */
	private void DoBuyItem(string a_oID, string a_oCharacterID, string a_oCurrency, int a_nPrice, EPlayfabCallback a_eCallback, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CAccess.Assert(a_oID.ExIsValid() && a_oCurrency.ExIsValid());

		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(a_eCallback, a_oCallback);

			PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest() {
				ItemId = a_oID, CharacterId = a_oCharacterID, VirtualCurrency = a_oCurrency, Price = a_nPrice
			}, (a_oResponse) => this.OnReceiveResponse(a_eCallback, a_oResponse), (a_oError) => this.OnReceiveFailResponse(a_eCallback, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	#endregion			// 조건부 함수
}

/** 플레이 팹 관리자 - 유저 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
	#region 함수
	/** 아이템을 구입한다 */
	public void BuyItem(string a_oID, string a_oCurrency, int a_nPrice, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.BuyItem: {a_oID}, {a_oCurrency}, {a_nPrice}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oID.ExIsValid() && a_oCurrency.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			this.DoBuyItem(a_oID, string.Empty, a_oCurrency, a_nPrice, EPlayfabCallback.BUY_ITEM, a_oCallback);
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 캐릭터를 구입한다 */
	public void BuyCharacter(string a_oID, string a_oCurrency, int a_nPrice, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.BuyCharacter: {a_oID}, {a_oCurrency}, {a_nPrice}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oID.ExIsValid() && a_oCurrency.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.BUY_CHARACTER, a_oCallback);
			this.BuyItem(a_oID, a_oCurrency, a_nPrice, (a_oSender, a_oResult, a_bIsSuccess) => this.OnBuyCharacter(a_oResult, a_bIsSuccess));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 데이터를 로드한다 */
	public void LoadDatas(List<string> a_oKeyList, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.LoadDatas: {a_oKeyList}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_DATAS, a_oCallback);

			PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
				PlayFabId = this.UserID, Keys = a_oKeyList
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_DATAS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_DATAS, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 아이템을 로드한다 */
	public void LoadItems(System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog("CPlayfabManager.LoadItems", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_ITEMS, a_oCallback);

			PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest() {
				// Do Something
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_ITEMS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_ITEMS, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 캐릭터를 로드한다 */
	public void LoadCharacters(System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog("CPlayfabManager.LoadCharacters", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_CHARACTERS, a_oCallback);

			PlayFabClientAPI.GetAllUsersCharacters(new ListUsersCharactersRequest() {
				PlayFabId = this.UserID
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_CHARACTERS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_CHARACTERS, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 데이터를 저장한다 */
	public void SaveDatas(Dictionary<string, string> a_oDataDict, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.SaveDatas: {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.SAVE_DATAS, a_oCallback);

			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
				Data = a_oDataDict, Permission = UserDataPermission.Private
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.SAVE_DATAS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.SAVE_DATAS, a_oError));
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
	/** 캐릭터를 구입했을 경우 */
	private void OnBuyCharacter(PlayFabResultCommon a_oResult, bool a_bIsSuccess) {
		CFunc.ShowLog($"CPlayfabManager.OnBuyCharacter: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);

		// 구입 되었을 경우
		if(this.IsInit && this.IsLogin && a_bIsSuccess) {
			PlayFabClientAPI.GrantCharacterToUser(new GrantCharacterToUserRequest() {
				ItemId = (a_oResult as PurchaseItemResult).Items[KCDefine.B_VAL_0_INT].ItemId
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.BUY_CHARACTER, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.BUY_CHARACTER, a_oError));
		} else {
			m_oCallbackDict02.GetValueOrDefault(EPlayfabCallback.BUY_CHARACTER)?.Invoke(this, null, false);
		}
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	#endregion			// 조건부 함수
}

/** 플레이 팹 관리자 - 캐릭터 */
public partial class CPlayfabManager : CSingleton<CPlayfabManager> {
	#region 함수
	/** 캐릭터 아이템을 구입한다 */
	public void BuyCharacterItem(string a_oID, string a_oCharacterID, string a_oCurrency, int a_nPrice, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.BuyCharacterItem: {a_oID}, {a_oCharacterID}, {a_oCurrency}, {a_nPrice}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oID.ExIsValid() && a_oCharacterID.ExIsValid() && a_oCurrency.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 구입 되었을 경우
		if(this.IsInit && this.IsLogin) {
			this.DoBuyItem(a_oID, a_oCharacterID, a_oCurrency, a_nPrice, EPlayfabCallback.BUY_CHARACTER_ITEM, a_oCallback);
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 캐릭터 데이터를 로드한다 */
	public void LoadCharacterDatas(string a_oCharacterID, List<string> a_oKeyList, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.LoadCharacterDatas: {a_oCharacterID}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oCharacterID.ExIsValid());

		m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_CHARACTER_DATAS, a_oCallback);

		PlayFabClientAPI.GetCharacterData(new GetCharacterDataRequest() {
			PlayFabId = this.UserID, CharacterId = a_oCharacterID, Keys = a_oKeyList
		}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_CHARACTER_DATAS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_CHARACTER_DATAS, a_oError));
	}

	/** 캐릭터 아이템을 로드한다 */
	public void LoadCharacterItems(string a_oCharacterID, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.LoadCharacterItems: {a_oCharacterID}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oCharacterID.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.LOAD_CHARACTER_ITEMS, a_oCallback);

			PlayFabClientAPI.GetCharacterInventory(new GetCharacterInventoryRequest() {
				CharacterId = a_oCharacterID
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.LOAD_CHARACTER_ITEMS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.LOAD_CHARACTER_ITEMS, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 캐릭터 데이터를 저장한다 */
	public void SaveCharacterDatas(string a_oCharacterID, Dictionary<string, string> a_oDataDict, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.SaveCharacterDatas: {a_oCharacterID}, {a_oDataDict}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oCharacterID.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.SAVE_CHARACTER_DATAS, a_oCallback);

			PlayFabClientAPI.UpdateCharacterData(new UpdateCharacterDataRequest() {
				CharacterId = a_oCharacterID, Data = a_oDataDict, Permission = UserDataPermission.Private
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.SAVE_CHARACTER_DATAS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.SAVE_CHARACTER_DATAS, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}

	/** 캐릭터 아이템 개수를 추가한다 */
	public void AddNumCharacterItems(string a_oID, string a_oCharacterID, int a_nNumItems, System.Action<CPlayfabManager, PlayFabResultCommon, bool> a_oCallback) {
		CFunc.ShowLog($"CPlayfabManager.AddNumCharacterItems: {a_oID}, {a_oCharacterID}, {a_nNumItems}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oID.ExIsValid() && a_oCharacterID.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			m_oCallbackDict02.ExReplaceVal(EPlayfabCallback.ADD_NUM_ITEMS, a_oCallback);
			
			PlayFabClientAPI.ConsumeItem(new ConsumeItemRequest() {
				ItemInstanceId = a_oID, CharacterId = a_oCharacterID, ConsumeCount = a_nNumItems
			}, (a_oResponse) => this.OnReceiveResponse(EPlayfabCallback.ADD_NUM_ITEMS, a_oResponse), (a_oError) => this.OnReceiveFailResponse(EPlayfabCallback.ADD_NUM_ITEMS, a_oError));
		} else {
			CFunc.Invoke(ref a_oCallback, this, null, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, null, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
	}
	#endregion			// 함수
}
#endif			// #endif			// #if PLAYFAB_MODULE_ENABLE
