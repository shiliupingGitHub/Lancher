using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Experimental.Networking;
using System.IO;
namespace Lancher
{
   public class DownloadWeb
    {
        public UnityWebRequest mWeb;
        public Bundle mBundle;
    }

    public class Downloader
    {
        public enum STATUS
        {
            NONE,
            OBTAIN_URL,
            WAIT_URL,
            OBTAIN_LOCAL,
            COMPARE,
            DOWNLOADING,
        }
        System.Action<Dictionary<string, Bundle>,string> mOnFinish;
        STATUS mStatus = STATUS.NONE;
        UnityWebRequest mWeb;
        DownloadWeb mDownloader = null;
        string mUrl;
        string mLocal;
        DownloaderAnchor mAnchor;
        Dictionary<string, Bundle> mRemoteBundle = new Dictionary<string, Bundle>();
        Dictionary<string, Bundle> mLocalBundle = new Dictionary<string, Bundle>();
        List< Bundle> mDownderBundle = new List<Bundle>();
        BundleManifest mLocalManifest, mRemoteManifest;
        float mProgress = 0.0f;
        public float Progress
        {
            get
            {
                return mProgress;
            }
        }
        public Downloader(string url,string local, System.Action<Dictionary<string,Bundle>,string> onFinish )
        {
            mProgress = 0.0f;
            mDownderBundle.Clear();
            mRemoteBundle.Clear();
            mLocalBundle.Clear();
            mOnFinish = onFinish;
            mUrl = url;
            mLocal = local;
            GameObject dc = new GameObject("DownloaderAnchor");
            DownloaderAnchor mAnchor = dc.AddComponent<DownloaderAnchor>();
            mAnchor.mDownload = this;
            mStatus = STATUS.OBTAIN_URL;
        }
        public void Update()
        {
            switch (mStatus)
            {
                case STATUS.OBTAIN_URL:
                    ObtainUrl();
                    break;
                case STATUS.WAIT_URL:
                    WaitURL();
                    break;
                case STATUS.OBTAIN_LOCAL:
                    ObtainLocal();
                    break;
                case STATUS.COMPARE:
                    Compare();
                    break;
                case STATUS.DOWNLOADING:
                    UpdatDownload();
                    break;
               
            }

        }
        void UpdatDownload()
        {
            if (mDownloader == null)
            {
                if (mDownderBundle.Count == 0)
                {
                    mStatus = STATUS.NONE;
                    if (null != mOnFinish)
                        mOnFinish(mRemoteBundle, null);
                    GameObject.Destroy(mAnchor.gameObject);
                }
                else
                {
                    mDownloader = new DownloadWeb();
                    mDownloader.mBundle = mDownderBundle[0];
                    string path = System.IO.Path.Combine(mUrl, mDownloader.mBundle.mType == Bundle.TYPE.AB ? "bundles" : "config");
                    path = System.IO.Path.Combine(path, mDownloader.mBundle.mName);
                    mDownloader.mWeb = UnityWebRequest.Get(path + "?time=" + Time.time);
                    mDownloader.mWeb.Send();
                    mDownderBundle.RemoveAt(0);
                }
            }
            else
            {
                if(mDownloader.mWeb.isDone)
                {
                    if(mDownloader.mWeb.isError)
                    {
                        if(null != mOnFinish)
                        {
                            mDownderBundle.Clear();
                            mOnFinish(null, mDownloader.mWeb.error);

                        }
                    }
                    else
                    {
                       byte[] bs =  mDownloader.mWeb.downloadHandler.data;
                        string path = Application.persistentDataPath;
                        path = System.IO.Path.Combine(path, mDownloader.mBundle.mType == Bundle.TYPE.AB ? "bundles" : "config");
                        path = System.IO.Path.Combine(path, mDownloader.mBundle.mName);
                        File.WriteAllBytes(path, bs);
                        if(mLocalBundle.ContainsKey(mDownloader.mBundle.mName))
                        {
                            mLocalBundle[mDownloader.mBundle.mName].mVersion = mDownloader.mBundle.mVersion;
                        }
                        else
                        {
                            mLocalBundle[mDownloader.mBundle.mName] = mDownloader.mBundle;
                            mLocalManifest.mBundles.mList.Add(mDownloader.mBundle);
                        }
                           
                       
                        string pathLocal = System.IO.Path.Combine(mLocal, "bundleManifest");
                        XmlHelper.XmlSerializeToFile(mLocalManifest, pathLocal, System.Text.Encoding.UTF8);
                    }
                    mDownloader.mWeb.Dispose();
                    mDownloader = null;
                }
            }
        }
        void Compare()
        {

            foreach(var r in mRemoteBundle)
            {
                Bundle local = null;
                mLocalBundle.TryGetValue(r.Key, out local);
                if(null == local || local.mVersion != r.Value.mVersion)
                {
                    mDownderBundle.Add( r.Value);
                }
            }
            mStatus = STATUS.DOWNLOADING;
        }
        void ObtainLocal()
        {
            string path = System.IO.Path.Combine(mLocal, "bundleManifest");
            string localTxt = File.ReadAllText(path);
            BundleManifest mLocalManifest = XmlHelper.XmlDeserialize<BundleManifest>(localTxt, System.Text.Encoding.UTF8);
            foreach (var b in mLocalManifest.mBundles.mList)
            {
                mLocalBundle[b.mName] = b;
            }
            mStatus = STATUS.COMPARE;
        }
        void ObtainUrl()
        {
            mWeb = UnityWebRequest.Get(mUrl + "/bundleManifest?time="+Time.time);
            mWeb.Send();
        }
        void WaitURL()
        {
           if( mWeb.isDone)
            {
                if(mWeb.isError)
                {
                    if (null != mOnFinish)
                        mOnFinish(null, mWeb.error);
                    mStatus = STATUS.NONE;
                }
                else
                {
                    string txt = mWeb.downloadHandler.text;
                    BundleManifest mRemoteManifest = XmlHelper.XmlDeserialize<BundleManifest>(txt, System.Text.Encoding.UTF8);
                    foreach(var b in mRemoteManifest.mBundles.mList)
                    {
                        mRemoteBundle[b.mName] = b;
                    }
                    mStatus = STATUS.OBTAIN_LOCAL;
                    
                    
                }
                mWeb.Dispose();
                mWeb = null;
            }
        }
    }
    public class DownloaderAnchor : MonoBehaviour
    {
        public Downloader mDownload;
        void Update()
        {
            if(null != mDownload)
            {
                mDownload.Update();
            }
        }
    }


}


