using System;
using System.IO;

namespace Hichu
{
    public class LDataBlock<T> where T : LDataBlock<T>
    {
        static T s_instance;

        // Chuyển sang .es3 để Easy Save 3 quản lý
        private const string FileExtension = ".es3";
        private static string FileName => typeof(T).Name + FileExtension;

        public static T instance
        {
            get
            {
                if (s_instance == null)
                {
                    // ES3.LoadFromDevice<T> sẽ trả về null nếu file/key chưa có
                    s_instance = LDataHelper.LoadFromDevice<T>(FileName)
                                 ?? Activator.CreateInstance<T>();
                    s_instance.Init();
                }
                return s_instance;
            }
        }

        protected virtual void Init()
        {
            MonoCallback.Instance.EventApplicationPause += OnApplicationPause;
            MonoCallback.Instance.EventApplicationQuit += OnApplicationQuit;
            MonoCallback.Instance.EventApplicationFocus += OnApplicationFocus;

            LDataBlockHelper.eventDelete += () => s_instance = null;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) Save();
        }
        private void OnApplicationQuit()
        {
            Save();
        }

        /// <summary> Lưu ngay lập tức </summary>
        public static void Save()
        {
            LDataHelper.SaveToDevice(instance, FileName);
        }

        /// <summary> Xóa dữ liệu và reset instance </summary>
        public static void Delete()
        {
            LDataHelper.DeleteInDevice(FileName);
            s_instance = null;
        }
    }

    public class LDataBlockHelper
    {
        public static event Action eventDelete;

        public static void ClearDeviceData()
        {
            eventDelete?.Invoke();

            LDataHelper.DeleteAllInDevice();
        }
    }
}