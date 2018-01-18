using System.Collections;
using System;
using UnityEngine;

namespace Utility {

	public class WebUtility : SingleClass<WebUtility> {

        const string m_addressDefaultUrl = "http://baidu.com";
        float m_waitTime;
        const int m_waitMaxTime = 5;

        public IEnumerator Post(string page, string method, Hashtable data, Action<WWW> callback) {
            WWWForm form = new WWWForm();
            foreach (string key in data.Keys) {
                form.AddField(key, data[key].ToString());
            }
            string url = m_addressDefaultUrl + (String.IsNullOrEmpty(page) ? "" : "/" + page) + (String.IsNullOrEmpty(method) ? "" : "/" + method);
            yield return PostWWW(url, form, callback);
        }

        public IEnumerator PostFile(string page, string method, Hashtable data, byte[] file, Action<WWW> callback) {
            WWWForm form = new WWWForm();
            foreach (string key in data.Keys) {
                form.AddField(key, data[key].ToString());
            }
            form.AddBinaryData("file", file);
            string url = m_addressDefaultUrl + (String.IsNullOrEmpty(page) ? "" : "/" + page) + (String.IsNullOrEmpty(method) ? "" : "/" + method);
            yield return PostWWW(url, form, callback);
        }

        IEnumerator PostWWW(string url, WWWForm form, Action<WWW> callback) {
            while(Application.internetReachability == NetworkReachability.NotReachable) {
                yield return null;
            }
            var www = new WWW(url, form);
            yield return www;
            if(callback != null) {
                callback(www);
            }
        }

        public IEnumerator GetData(string _path, Action<WWW> callback) {
            m_waitTime = 0;
            string url = m_addressDefaultUrl + _path;
            yield return GetWWW(url, callback);
        }

        IEnumerator GetWWW(string url, Action<WWW> callback) {
            while(Application.internetReachability == NetworkReachability.NotReachable) {
                //若一直无网络，等待几秒退出
                m_waitTime += Time.deltaTime;
                if(m_waitTime > m_waitMaxTime) {
                    if(callback != null) {
                        callback(null);
                    }
                    yield break;
                }
                yield return null;
            }
            var www = new WWW(url);
            yield return www;
            if(string.IsNullOrEmpty(www.error)) {
                if(callback != null) {
                    callback(www);
                }
            } else {
                if(callback != null) {
                    callback(null);
                }
            }
        }
    }
}