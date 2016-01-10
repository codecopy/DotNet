﻿using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

namespace EthanLibrary.Common
{
    /// <summary>
    /// URL的操作类
    /// </summary>
    public class URLHelper
    {
        private static System.Text.Encoding encoding = System.Text.Encoding.UTF8;

        #region URL的64位编码

        public static string Base64Encrypt(string sourthUrl)
        {
            string eurl = HttpUtility.UrlEncode(sourthUrl);
            eurl = Convert.ToBase64String(encoding.GetBytes(eurl));
            return eurl;
        }

        #endregion URL的64位编码

        #region URL的64位解码

        public static string Base64Decrypt(string eStr)
        {
            if (!IsBase64(eStr))
            {
                return eStr;
            }
            byte[] buffer = Convert.FromBase64String(eStr);
            string sourthUrl = encoding.GetString(buffer);
            sourthUrl = HttpUtility.UrlDecode(sourthUrl);
            return sourthUrl;
        }

        /// <summary>
        /// 是否是Base64字符串
        /// </summary>
        /// <param name="eStr"></param>
        /// <returns></returns>
        public static bool IsBase64(string eStr)
        {
            if ((eStr.Length % 4) != 0)
            {
                return false;
            }
            if (!Regex.IsMatch(eStr, "^[A-Z0-9/+=]*$", RegexOptions.IgnoreCase))
            {
                return false;
            }
            return true;
        }

        #endregion URL的64位解码

        #region URL编码

        /// <summary>
        /// 返回 URL 字符串的编码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>编码结果</returns>
        public static string UrlEncode(string str)
        {
            return System.Web.HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// 返回 URL 字符串的解码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>解码结果</returns>
        public static string UrlDecode(string str)
        {
            return System.Web.HttpUtility.UrlDecode(str);
        }

        #endregion URL编码

        /// <summary>
        /// 添加URL参数
        /// </summary>
        public static string AddParam(string url, string paramName, string value)
        {
            Uri uri = new Uri(url);
            if (string.IsNullOrEmpty(uri.Query))
            {
                string eval = HttpContext.Current.Server.UrlEncode(value);
                return String.Concat(url, "?" + paramName + "=" + eval);
            }
            else
            {
                string eval = HttpContext.Current.Server.UrlEncode(value);
                return String.Concat(url, "&" + paramName + "=" + eval);
            }
        }

        /// <summary>
        /// 更新URL参数
        /// </summary>
        public static string UpdateParam(string url, string paramName, string value)
        {
            string keyWord = paramName + "=";
            int index = url.IndexOf(keyWord) + keyWord.Length;
            int index1 = url.IndexOf("&", index);
            if (index1 == -1)
            {
                url = url.Remove(index, url.Length - index);
                url = string.Concat(url, value);
                return url;
            }
            url = url.Remove(index, index1 - index);
            url = url.Insert(index, value);
            return url;
        }

        #region 分析URL所属的域

        public static void GetDomain(string fromUrl, out string domain, out string subDomain)
        {
            domain = "";
            subDomain = "";
            try
            {
                if (fromUrl.IndexOf("的名片") > -1)
                {
                    subDomain = fromUrl;
                    domain = "名片";
                    return;
                }

                UriBuilder builder = new UriBuilder(fromUrl);
                fromUrl = builder.ToString();

                Uri u = new Uri(fromUrl);

                if (u.IsWellFormedOriginalString())
                {
                    if (u.IsFile)
                    {
                        subDomain = domain = "客户端本地文件路径";
                    }
                    else
                    {
                        string Authority = u.Authority;
                        string[] ss = u.Authority.Split('.');
                        if (ss.Length == 2)
                        {
                            Authority = "www." + Authority;
                        }
                        int index = Authority.IndexOf('.', 0);
                        domain = Authority.Substring(index + 1, Authority.Length - index - 1).Replace("comhttp", "com");
                        subDomain = Authority.Replace("comhttp", "com");
                        if (ss.Length < 2)
                        {
                            domain = "不明路径";
                            subDomain = "不明路径";
                        }
                    }
                }
                else
                {
                    if (u.IsFile)
                    {
                        subDomain = domain = "客户端本地文件路径";
                    }
                    else
                    {
                        subDomain = domain = "不明路径";
                    }
                }
            }
            catch
            {
                subDomain = domain = "不明路径";
            }
        }

        /// <summary>
        /// 分析 url 字符串中的参数信息
        /// </summary>
        /// <param name="url">输入的 URL</param>
        /// <param name="baseUrl">输出 URL 的基础部分</param>
        /// <param name="nvc">输出分析后得到的 (参数名,参数值) 的集合</param>
        public static void ParseUrl(string url, out string baseUrl, out NameValueCollection nvc)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            nvc = new NameValueCollection();
            baseUrl = "";

            if (url == "")
                return;

            int questionMarkIndex = url.IndexOf('?');

            if (questionMarkIndex == -1)
            {
                baseUrl = url;
                return;
            }
            baseUrl = url.Substring(0, questionMarkIndex);
            if (questionMarkIndex == url.Length - 1)
                return;
            string ps = url.Substring(questionMarkIndex + 1);

            // 开始分析参数对
            Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            MatchCollection mc = re.Matches(ps);

            foreach (Match m in mc)
            {
                nvc.Add(m.Result("$2").ToLower(), m.Result("$3"));
            }
        }

        #endregion 分析URL所属的域

        #region 自动识别文本中的URL

        /// <summary>
        /// 自动识别文本中的URL
        /// 可以识别 www.，http://， ftp://， xx@xx.xx， mms://
        /// </summary>
        /// <param name="input">输入数据</param>
        /// <returns>自动识别URL后的数据</returns>
        public static string autoConvertToURL(object input)
        {
            string str = input.ToString();

            Regex Reg;
            Reg = new Regex("([^\\]=>])(http://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "$1<a href=\"$2\" target=\"_blank\">$2</a>");
            Reg = new Regex("^(http://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "<a href=\"$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("(http://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)$");
            str = Reg.Replace(str, "<a href=\"$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("([^\\]=>])(ftp://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "$1<a href=\"$2\" target=\"_blank\">$2</a>");
            Reg = new Regex("^(ftp://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "<a href=\"$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("(ftp://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)$");
            str = Reg.Replace(str, "<a href=\"$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("([^\\]=>])(mms://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "$1<a href=\"$2\" target=\"_blank\">$2</a>");
            Reg = new Regex("^(mms://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "<a href=\"$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("(mms://[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)$");
            str = Reg.Replace(str, "<a href=\"$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("([a-z0-9_A-Z\\-\\.]{1,20})@([a-z0-9_\\-]{1,15})\\.([a-z]{2,4})");
            str = Reg.Replace(str, "<a href=\"mailto:$1@$2.$3\" target=\"_blank\">$1@$2.$3</a>");
            Reg = new Regex("([^/])(www.[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "$1<a href=\"http://$2\" target=\"_blank\">$2</a>");
            Reg = new Regex("^(www.[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)");
            str = Reg.Replace(str, "<a href=\"http://$1\" target=\"_blank\">$1</a>");
            Reg = new Regex("(www.[A-Za-z0-9\\./=\\?%\\-&_~`@':+!]+)$");
            str = Reg.Replace(str, "<a href=\"http://$1\" target=\"_blank\">$1</a>");

            return str;
        }

        #endregion 自动识别文本中的URL
    }
}