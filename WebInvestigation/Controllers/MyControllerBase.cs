using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebInvestigation.Controllers
{
    public class MyControllerBase : Controller
    {
        protected string GetCookie(string key, string def = "")
        {
            var cipher = Request.Cookies[key];
            if (string.IsNullOrEmpty(cipher))
            {
                return def;
            }
            else
            {
                return Decode(cipher);
            }
        }
        protected void SetCookie(string key, string value)
        {
            Response.Cookies.Append(key, Encode(value), new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow + TimeSpan.FromDays(14),
            });
        }

        private const string TEXTSET64 = "wK0cskEpvVlBUXitL+byQIT5W89xRmdZAjJMe62HYSO/3u7raPCNhogzDfG1nq4F";
        private const string KEY = "F4iK4KH4PY9eBxWA";
        private const string MASK = "D6B7763D-43D5-492F-BAEA-0F4A1062D7AE";
        private static readonly Random RND = new Random(DateTime.Now.Ticks.GetHashCode());

        private string FusionString(string basestr, string filter)
        {
            var nums = new[] { 79, 47, 37, 2, 7, 223, 269, 229, 211, 59, 89, 263, 149, 13, 179, 83, 281, 127, 199, 227, 31, 131, 73, 157, 5, 19, 139, 197, 167, 193, 53, 151, 29, 137, 97, 107, 241, 239, 163, 113, 103, 11, 257, 109, 23, 3, 41, 61, 233, 277, };
            var ret = new StringBuilder(basestr);
            var nB = basestr.Length;
            var nF = filter.Length;
            var offset = 0;
            for (var i = Math.Max(nB, nF) - 1; i >= 0; i--)
            {
                ret[i % nB] = TEXTSET64[(TEXTSET64.IndexOf(ret[i % nB]) + (int)filter[i % nF] + nums[(i + offset) % nums.Length]) % TEXTSET64.Length];
                offset++;
            }
            return ret.ToString();
        }

        /// <summary>
        /// RIjndael Encoding
        /// </summary>
        /// <param name="planeText"></param>
        /// <returns></returns>
        private string Encode(string planeText)
        {
            var iv = new StringBuilder();
            int ivN = 0;
            for (int ivi = 0; ivi < ivN + 16; ivi++)
            {
                iv.Append(TEXTSET64[RND.Next(TEXTSET64.Length - 1)]);
            }
            using (var ri = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.ASCII.GetBytes(iv.ToString()),
                Key = Encoding.ASCII.GetBytes(FusionString(KEY, MASK)),
            })
            {
                var enc = ri.CreateEncryptor(ri.Key, ri.IV);
                byte[] buf;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(planeText);
                        }
                        buf = ms.ToArray();
                    }
                }
                return ($"{TEXTSET64[ivN]}{iv}{Convert.ToBase64String(buf)}");
            }
        }

        /// <summary>
        /// Rijndael Decoding
        /// </summary>
        /// <param name="secText"></param>
        /// <returns></returns>
        private string Decode(string secText)
        {
            int ivN = TEXTSET64.IndexOf(secText[0]);
            string iv = secText.Substring(1, ivN + 16);
            string sec = secText.Substring(ivN + iv.Length + 1);

            using (var rijndael = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.ASCII.GetBytes(iv),
                Key = Encoding.ASCII.GetBytes(FusionString(KEY, MASK)),
            })
            {
                var de = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
                using (var ms = new MemoryStream(Convert.FromBase64String(sec)))
                {
                    using (var cs = new CryptoStream(ms, de, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        protected void PersistInput<TModel>(string propertyName, TModel model, string def)
        {
            var pp = model.GetType().GetProperty(propertyName);
            var inval = pp.GetValue(model)?.ToString() ?? "";

            if (string.IsNullOrEmpty(inval.Trim()) || inval.Equals(def))
            {
                var ck = GetCookie($"{model.GetType().Name}_{propertyName}", def);
                pp.SetValue(model, ck);
            }
            else
            {
                SetCookie($"{model.GetType().Name}_{propertyName}", inval);
            }
        }
    }
}
