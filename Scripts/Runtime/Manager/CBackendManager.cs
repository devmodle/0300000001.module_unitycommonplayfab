using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if BACKEND_MODULE_ENABLE
/** 백엔드 관리자 */
public partial class CBackendManager : CSingleton<CBackendManager> {
	/** 콜백 */
	public enum ECallback {
		NONE,
		INIT,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams {
		public Dictionary<ECallback, System.Action<CBackendManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private STParams m_stParams;
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;
	#endregion			// 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog("CBackendManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		} else {
			m_stParams = a_stParams;
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}
	#endregion			// 함수
}
#endif			// #if BACKEND_MODULE_ENABLE
