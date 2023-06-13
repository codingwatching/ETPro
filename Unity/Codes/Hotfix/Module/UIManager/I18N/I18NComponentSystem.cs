﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class I18NComponentAwakeSystem : AwakeSystem<I18NComponent>
    {
        public override void Awake(I18NComponent self)
        {
            // self.AddSystemFonts();
            I18NComponent.Instance = self;
            self.curLangType = PlayerPrefs.GetInt(CacheKeys.CurLangType, 0);
            self.I18NEntity = new Dictionary<long, Entity>();

            var res = I18NConfigCategory.Instance.GetAll();
            self.i18nTextKeyDic = new Dictionary<string, string>();
            foreach (var item in res)
            {
                switch (self.curLangType)
                {
                    case I18NComponent.LangType.Chinese:
                        self.i18nTextKeyDic.Add(item.Value.Key, item.Value.Chinese);
                        break;
                    case I18NComponent.LangType.English:
                        self.i18nTextKeyDic.Add(item.Value.Key, item.Value.English);
                        break;
                    default:
                        self.i18nTextKeyDic.Add(item.Value.Key, item.Value.Chinese);
                        break;
                }
            }

            I18NBridge.Instance.i18nTextKeyDic = self.i18nTextKeyDic;
            I18NConfigCategory.Instance = null;
            ConfigComponent.Instance.ReleaseConfig<I18NConfigCategory>();
        }
    }
    [ObjectSystem]
    public class I18NComponentDestroySystem : DestroySystem<I18NComponent>
    {
        public override void Destroy(I18NComponent self)
        {
            I18NComponent.Instance = null;
            self.i18nTextKeyDic.Clear();
            self.i18nTextKeyDic = null;
            I18NBridge.Instance.i18nTextKeyDic = null;
        }
    }
    [FriendClass(typeof(I18NComponent))]
    public static class I18NComponentSystem
    {
        public static string I18NGetText(this I18NComponent self, string key)
        {
            if (!self.i18nTextKeyDic.TryGetValue(key, out var value))
            {
                return key;
            }
            return value;
        }

        /// <summary>
        /// 根据key取多语言取不到返回key
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static string I18NGetParamText(this I18NComponent self, string key, params object[] paras)
        {
            if (!self.i18nTextKeyDic.TryGetValue(key, out var value))
            {
                return key;
            }
            if (paras != null)
                return string.Format(value, paras);
            else
                return value;
        }
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool I18NTryGetText(this I18NComponent self, string key, out string result)
        {
            if (!self.i18nTextKeyDic.TryGetValue(key, out result))
            {
                result = key;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 切换语言,外部接口
        /// </summary>
        /// <param name="langType"></param>
        public static void SwitchLanguage(this I18NComponent self, int langType)
        {
            if (self.curLangType == langType) return;
            ConfigComponent.Instance.LoadOneConfig<I18NConfigCategory>();
            //修改当前语言
            PlayerPrefs.SetInt(CacheKeys.CurLangType, langType);
            self.curLangType = langType;
            var res = I18NConfigCategory.Instance.GetAll();
            self.i18nTextKeyDic.Clear();
            foreach (var item in res)
            {
                switch (self.curLangType)
                {
                    case I18NComponent.LangType.Chinese:
                        self.i18nTextKeyDic.Add(item.Value.Key, item.Value.Chinese);
                        break;
                    case I18NComponent.LangType.English:
                        self.i18nTextKeyDic.Add(item.Value.Key, item.Value.English);
                        break;
                    default:
                        self.i18nTextKeyDic.Add(item.Value.Key, item.Value.Chinese);
                        break;
                }
            }

            var values = self.I18NEntity.Values;
            foreach (var entity in values)
            {
                UIWatcherComponent.Instance.OnLanguageChange(entity);
            }
            I18NBridge.Instance.OnLanguageChange();
            I18NConfigCategory.Instance = null;
            ConfigComponent.Instance.ReleaseConfig<I18NConfigCategory>();
        }

        public static void RegisterI18NEntity(this I18NComponent self,Entity entity)
        {
            if(!self.I18NEntity.ContainsKey(entity.Id))
                self.I18NEntity.Add(entity.Id,entity);
        }
        
        public static void RemoveI18NEntity(this I18NComponent self,Entity entity)
        {
            self.I18NEntity.Remove(entity.Id);
        }

        public static int GetCurLanguage(this I18NComponent self)
        {
            return self.curLangType;
        }

        #region 添加系统字体

        /// <summary>
        /// 需要就添加
        /// </summary>
        public static void AddSystemFonts(this I18NComponent self)
        {
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
            string[] fonts = new[] { "STSONG" };
#elif UNITY_ANDROID
            string[] fonts = new[] {
                "NotoSansDevanagari-Regular",//天城体梵文
                "NotoSansThai-Regular",        //泰文
                "NotoSerifHebrew-Regular",     //希伯来文
                "NotoSansSymbols-Regular-Subsetted",  //符号
                "NotoSansCJK-Regular"          //中日韩
            };
#elif UNITY_IOS
            string[] fonts = new[] {
                "DevanagariSangamMN",  //天城体梵文
                "AppleSDGothicNeo",    //韩文，包含日文，部分中文
                "Thonburi",            //泰文
                "ArialHB"              //希伯来文
            };
#else
            string[] fonts = new string[0];
#endif
            TextMeshFontAssetManager.Instance.AddWithOSFont(fonts);
        }

        #endregion
    }
}
