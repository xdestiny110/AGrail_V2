using System.Collections;
using Framework;
using Framework.AssetBundle;
using UnityEngine;

namespace AGrail
{
    public class GameInit : MonoBehaviour
    {

        IEnumerator Start()
        {
            yield return AssetBundleManager.Instance.LoadCheckFileAsync();
            if (AssetBundleManager.Instance.IsError)
            {
                // return error msg
            }
            else
            {
                var luaScript = AssetBundleManager.Instance.LoadAsset<TextAsset>("lua_util", "GameMgr");
                MonoRoot.luaEnv.DoString(luaScript.text, luaScript.name, null);
            }
        }
    }
}

