using Framework.AssetBundle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Framework.UI
{
    [LuaCallCSharp]
    public class ItemBase : MonoBehaviour
    {
        public LuaTable ScriptEnv;
        private TextAsset luaScript;
        private Action luaDestroy;

        IEnumerator Start()
        {
            while (AssetBundleManager.Instance.Progress < 1)
                yield return null;

            var op = AssetBundleManager.Instance.LoadAssetAsyn<TextAsset>("lua_ui", gameObject.name);
            yield return op;
            luaScript = op.Asset;
            if(luaScript != null)
            {
                //Debug.LogFormat("load lua ui '{0}' script", gameObject.name);
                ScriptEnv = MonoRoot.luaEnv.NewTable();
                LuaTable meta = MonoRoot.luaEnv.NewTable();
                meta.Set("__index", MonoRoot.luaEnv.Global);
                ScriptEnv.SetMetaTable(meta);
                meta.Dispose();
                ScriptEnv.Set("self", this);
                MonoRoot.luaEnv.DoString(luaScript.text, luaScript.name, ScriptEnv);
                var luaStart = ScriptEnv.Get<Action>("start");
                if (luaStart != null) luaStart();
                luaDestroy = ScriptEnv.Get<Action>("destroy");
            }
            else
                Debug.LogErrorFormat("Can not find lua script on {0}", gameObject.name);
        }

        void OnDestroy()
        {
            if (luaDestroy != null) luaDestroy();
        }
    }
}

