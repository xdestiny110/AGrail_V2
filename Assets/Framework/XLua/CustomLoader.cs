using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Framework
{
    public class CustomLoader : MonoBehaviour
    {
        private LuaEnv luaenv = null;

        void Awake()
        {
            luaenv = new LuaEnv();
            luaenv.AddLoader((ref string filename) => {
                if (filename == "luapbintf")
                {
                    string script = "return {ccc = 9999}";
                    return System.Text.Encoding.UTF8.GetBytes(script);
                }
                return null;
            });
        }
    }
}
