using OdinSerializer;
using System;
using System.IO;
using UnityEngine;

namespace Hichu
{
    public static class LDataHelper
    {
        /// <summary>
        /// Save an object to disk using ES3.
        /// </summary>
        /// <typeparam name="T">Type of the data block.</typeparam>
        /// <param name="data">The instance to save.</param>
        /// <param name="fileName">The .es3 file name (e.g. "DataSettings.es3").</param>
        public static void SaveToDevice<T>(T data, string fileName) where T : class
        {
            try
            {
                string key = typeof(T).Name;
                ES3.Save<T>(key, data, fileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LDataHelper] ES3 Save failed ({fileName}): {e}");
            }
        }

        /// <summary>
        /// Load an object from disk using ES3. Returns null if the file or key does not exist.
        /// </summary>
        /// <typeparam name="T">Type of the data block.</typeparam>
        /// <param name="fileName">The .es3 file name (e.g. "DataSettings.es3").</param>
        /// <returns>The deserialized instance, or null if none.</returns>
        public static T LoadFromDevice<T>(string fileName) where T : class
        {
            try
            {
                if (!ES3.FileExists(fileName))
                {
                    Debug.Log($"[LDataHelper] ES3 file '{fileName}' not found. Returning null.");
                    return null;
                }

                string key = typeof(T).Name;
                if (!ES3.KeyExists(key, fileName))
                {
                    Debug.Log($"[LDataHelper] Key '{key}' not found in '{fileName}'. Returning null.");
                    return null;
                }

                return ES3.Load<T>(key, fileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LDataHelper] ES3 Load failed ({fileName}): {e}");
                return null;
            }
        }

        /// <summary>
        /// Delete a single ES3 file from disk.
        /// </summary>
        /// <param name="fileName">The .es3 file name to delete.</param>
        public static void DeleteInDevice(string fileName)
        {
            try
            {
                if (ES3.FileExists(fileName))
                    ES3.DeleteFile(fileName);
                else
                    Debug.Log($"[LDataHelper] ES3 file '{fileName}' not found for deletion.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LDataHelper] ES3 DeleteFile failed ({fileName}): {e}");
            }
        }

        /// <summary>
        /// Delete all .es3 files in the ES3 default folder.
        /// </summary>
        public static void DeleteAllInDevice()
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, "ES3Files");
                if (!Directory.Exists(path))
                    return;

                foreach (var file in new DirectoryInfo(path).GetFiles("*.es3"))
                    file.Delete();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LDataHelper] DeleteAllInDevice failed: {e}");
            }
        }
    }
}