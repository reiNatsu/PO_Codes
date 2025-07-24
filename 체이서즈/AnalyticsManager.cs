using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif


namespace LIFULSE.Manager
{
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {

        private string _userPublicke;
        private DateTime _startTime;
        private DateTime _endTime;
        private float _gamePlayTime;
        private bool _userGaveConsent; 
        private Coroutine _coroutine;
        private bool _isInitialized = false;

        private string _gameInstalledKey = "GameInstalled";

        public bool IsInitialized
        {
            get
            {
                return _isInitialized&&ManagerLoader.Instance.AT;
            }
        }

        private void Init()
        {
            Debug.Log("AnalyticsManager Init");

#if UNITY_IOS

            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }

            //_coroutine = StartCoroutine(CheckAuthorizationTracking());
#endif

//#if LIVE_BUILD||EXTERNAL_BUILD
            //if (RestApiManager.Instance.ServerType == ServerType.Live)
            {
                lnitializeAnalytics();
            }
//#else
            Debug.Log("<color=#9efc9e>Analytict Manager 서버가 리얼이 아니라 초기화 하지 않겠음.%%%</color>");
            //_isInitialized = false;
            //_isInitialized = false;
            //if (RestApiManager.Instance.ServerType == ServerType.Test)
            //{
            //    lnitializeAnalytics();
            //    Debug.Log("<color=#9efc9e>Analytict Manager DeepLinkManager.Instance.IsDeepLinkActivated => "+DeepLinkManager.Instance.IsDeepLinkActivated+" %%%%</color>");
            //    if (DeepLinkManager.Instance.IsDeepLinkActivated)
            //        SendGameInstallEvent();
            //}
            //else
            //{
            //    Debug.Log("<color=#9efc9e>Analytict Manager 서버가 테스트 서버가 아니라 초기화 하지 않겠음.%%%</color>");
            //}
            //lnitializeAnalytics();
           
//#endif
        }

        private void OnDisable()
        {
            if(_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

#if UNITY_IOS

        private IEnumerator CheckAuthorizationTracking()
        {
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                yield return StartCoroutine(WaitAuthorizationTracking());
            }

            lnitializeAnalytics();
        }

        private IEnumerator WaitAuthorizationTracking()
        {
            Debug.Log("AnalyticsManager WaitAuthorizationTracking");
            float time = 0;

            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                time += 0.1f;
                Debug.Log("AnalyticsManager WaitAuthorizationTracking Wait " + time);

                if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                    yield break;
            }
        }
#endif

        public void SendGameInstallEvent()
        {
            Debug.Log("(1111)  DeepLinkManager.Instance.AdPlatform : "+DeepLinkManager.Instance.AdPlatform);
            // 게임 설치 완료가 되었는지 체크 >> 기기에 값 저장하고 있기.
            if (!DSPlayerPrefs.HasKey(_gameInstalledKey) )
            {
                Debug.Log("(2222)  DeepLinkManager.Instance.AdPlatform : "+DeepLinkManager.Instance.AdPlatform);
                // 게임 처음 설치 이벤트 전송
                CustomEvent customEvent = new CustomEvent("Game_Installed")
                {
                    {"ad_platform", DeepLinkManager.Instance.AdPlatform }
                };
            //    var parameters = new Dictionary<string, object>
            //{
            //    {"ad_platform", DeepLinkManager.Instance.AdPlatform }
            //};

                AnalyticsService.Instance.RecordEvent(customEvent);
                AnalyticsService.Instance.Flush(); // 이벤트 즉시 전송

                DSPlayerPrefs.SetInt(_gameInstalledKey, 1);
                DSPlayerPrefs.Save();
            }
        }


        public async void lnitializeAnalytics()
        {
            Debug.Log("<color=#9efc9e>[1]AnalyticsManager lnitializeAnalytics</color>");
            _userGaveConsent = IsAuthorized();
            if (_userGaveConsent)
            {
                // 환경 설정
                var options = new InitializationOptions();
                Debug.Log("<color=#9efc9e>[2]Unity Gaming Service Environment is  "+GetEvnironment()+"</color>");
                options.SetEnvironmentName(GetEvnironment());

                // 유니티 서비스 초기화
                await UnityServices.InitializeAsync(options);
                _isInitialized = true;
                Debug.Log("<color=#9efc9e>[3]AnalyticsManager lnitializeAnalytics End</color>");

                // 데이터 수집 시작
                AnalyticsService.Instance.StartDataCollection();
                AnalyticsService.Instance.Flush();
                Debug.Log("<color=#9efc9e>[4]Analytics service initialized and data collection started.</color>");
                //await UnityServices.InitializeAsync();
                //_isInitialized = true;
                //Debug.Log("AnalyticsManager lnitializeAnalytics End");

                //// 데이터 수집을 시작합니다.
                //AnalyticsService.Instance.StartDataCollection();
                //AnalyticsService.Instance.Flush();
                //Debug.Log("Analytics service initialized and data collection started.");

                // 초기화 할 떄 DeepLink가 ture인 상태이면 바로 클릭 이벤트 보내기
                //if (DeepLinkManager.Instance.IsDeepLinkActivated)
                //    DeepLinkManager.Instance.OnSendAdClickEvent?.Invoke();
            }
            else
            {
                Debug.Log("<color=#ff3c00>User did not give consent for data collection.</color>");
            }


            Debug.Log("<color=#9efc9e>lnitializeAnalytics isInitialized? "+_isInitialized+"</color>");
        }


        public void SendCustomEvent(CustomEvent myEvent)
        {
            AnalyticsService.Instance.RecordEvent(myEvent);
            AnalyticsService.Instance.Flush();
            // 이벤트 전송 결과를 확인합니다.
            Debug.Log("SendCustomEvent sent successfully.");
        }

        public int GetHoldGold()
        {
            return GameInfoManager.Instance.GetAmount("i_gold");
        }
        public int GetHoldCash()
        {
            return GameInfoManager.Instance.GetAmount("i_cash");
        }

        // 임시 : 컨텐츠 플레이 시간을 한국 시간 기준으로 입력
        public string SetKoreaTime()
        {
            // 한국 시간대로 변환하기 위한 TimeZoneInfo 객체 가져오기
            DateTime utcNow = DateTime.UtcNow.ToAddHours();

            // 변환된 시간을 원하는 형식의 문자열로 포맷팅
            string formattedTime = utcNow.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.Log("Set Timezone Korea :  "+ formattedTime);
            return formattedTime;
        }

        private bool IsAuthorized()
        {
#if UNITY_IOS
            // Check if user has allowed tracking
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
                return true;
            else
                return false;
#endif
            return true;
        }

        public override void InitializeDirect()
        {
            _instance = this;
           // Init();
        }

        //서버 타입에 따라 환경 설정
        private string GetEvnironment()
        {
            string environment = string.Empty;
            switch (RestApiManager.Instance.ServerType)
            {
                case ServerType.Test:
                    environment = "test";
                    break;
                case ServerType.QA:
                    environment = "qa";
                    break;
                case ServerType.Live:
                    environment = "live";
                    break;
                case ServerType.External:
                    environment = "external";
                    break;
                default:
                    environment = "production";
                    break;
            }
            return environment;
        }

        public override void SettingDirect()
        {
        }
        public override IEnumerator Initialize()
        {
            _instance = this;
            Init();
            yield break;
        }

        public override IEnumerator Setting()
        {
            yield break;
        }
    }
}
