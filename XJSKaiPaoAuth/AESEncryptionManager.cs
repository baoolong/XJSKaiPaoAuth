using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace XJSKaiPaoAuth
{
    /// <summary>
    /// AES加密解密管理器
    /// 提供AES-256加解密功能，支持密钥生成、加密和解密
    /// </summary>
    public class AESEncryptionManager : IDisposable
    {
        private Aes _aes; 
        private bool _disposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AESEncryptionManager()
        {
            _aes = Aes.Create();
            _aes.KeySize = 256; // 使用AES-256
            _aes.BlockSize = 128;
            _aes.Mode = CipherMode.CBC; // 使用CBC模式
            _aes.Padding = PaddingMode.PKCS7; // 使用PKCS7填充

            _aes.Key = Convert.FromBase64String("qTuI+RIQ323J4jZTLJMqdDDf0v8YeS1cCeiT1RvYp5U=");
            _aes.IV = Convert.FromBase64String("h8tdhVllSBUQ2UuUtTG/NA==");
        }

        /// <summary>
        /// 生成新的AES密钥和初始化向量
        /// </summary>
        /// <param name="key">输出的Base64格式密钥</param>
        /// <param name="iv">输出的Base64格式初始化向量</param>
        public void GenerateKeyAndIV(out string key, out string iv)
        {
            _aes.GenerateKey();
            _aes.GenerateIV();
            key = Convert.ToBase64String(_aes.Key);
            iv = Convert.ToBase64String(_aes.IV);
        }

        /// <summary>
        /// 设置AES密钥和初始化向量
        /// </summary>
        /// <param name="key">Base64格式的密钥</param>
        /// <param name="iv">Base64格式的初始化向量</param>
        public void SetKeyAndIV(string key, string iv)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
                throw new ArgumentNullException("密钥和初始化向量不能为空");

            try
            {
                _aes.Key = Convert.FromBase64String(key);
                _aes.IV = Convert.FromBase64String(iv);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("密钥或初始化向量格式无效", ex);
            }
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">要加密的明文</param>
        /// <returns>Base64格式的加密结果</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (ICryptoTransform encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV))
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="cipherText">Base64格式的密文</param>
        /// <returns>解密后的明文</returns>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (ICryptoTransform decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV))
                {
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("解密失败: 密文格式无效或密钥不正确", ex);
            }
        }

        /// <summary>
        /// 从文件加载密钥和初始化向量
        /// </summary>
        /// <param name="filePath">包含密钥和IV的文件路径</param>
        public void LoadKeyFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("密钥文件不存在", filePath);

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                    throw new FormatException("密钥文件格式无效");

                SetKeyAndIV(lines[0], lines[1]);
            }
            catch (Exception ex)
            {
                throw new IOException("加载密钥文件失败", ex);
            }
        }

        /// <summary>
        /// 保存密钥和初始化向量到文件
        /// </summary>
        /// <param name="filePath">保存路径</param>
        public void SaveKeyToFile(string filePath)
        {
            try
            {
                string key = Convert.ToBase64String(_aes.Key);
                string iv = Convert.ToBase64String(_aes.IV);
                File.WriteAllLines(filePath, new string[] { key, iv });
            }
            catch (Exception ex)
            {
                throw new IOException("保存密钥文件失败", ex);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    if (_aes != null)
                    {
                        _aes.Dispose();
                        _aes = null;
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~AESEncryptionManager()
        {
            Dispose(false);
        }
    }
}