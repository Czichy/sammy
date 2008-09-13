// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Sidi.Sammy
{
    public class CryptUtil
    {
        static int keySize = 16;
        static int saltLength = 16;

        static SymmetricAlgorithm GetCrypt()
        {
            RijndaelManaged crypt = new RijndaelManaged();
            crypt.Padding = PaddingMode.Zeros;
            crypt.Mode = CipherMode.CBC;
            crypt.KeySize = keySize * 8;
            crypt.BlockSize = 256;
            return crypt;
        }

        /*
        static SymmetricAlgorithm GetCrypt()
        {
            TripleDESCryptoServiceProvider crypt = new TripleDESCryptoServiceProvider();
            crypt.Padding = PaddingMode.None;
            crypt.KeySize = 192;
            return crypt;
        }
         */

        public static CryptoStream EncryptStream(Stream s, string password)
        {
            SymmetricAlgorithm crypt = GetCrypt();
            RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
            byte[] salt = new byte[saltLength];
            r.GetBytes(salt);
            PasswordDeriveBytes keyGen = new PasswordDeriveBytes(password, salt);
            crypt.GenerateIV();
            crypt.Key = keyGen.GetBytes(keySize);
            s.Write(salt, 0, salt.Length);
            s.Write(crypt.IV, 0, crypt.IV.Length);
            return new CryptoStream(s, crypt.CreateEncryptor(), CryptoStreamMode.Write);
        }

        public static CryptoStream DecrytStream(Stream s, string password)
        {
            SymmetricAlgorithm crypt = GetCrypt();
            // crypt.Padding = PaddingMode.None;
            byte[] iv = new byte[crypt.IV.Length];
            byte[] salt = new byte[saltLength];
            s.Read(salt, 0, salt.Length);
            s.Read(iv, 0, crypt.IV.Length);
            PasswordDeriveBytes keyGen = new PasswordDeriveBytes(password, salt);
            crypt.Key = keyGen.GetBytes(keySize);
            crypt.IV = iv;
            return new CryptoStream(s, crypt.CreateDecryptor(), CryptoStreamMode.Read);
        }
    }
}
