using ManagementApplication.Models;
using System.Collections.Concurrent;

namespace ManagementApplication.Common
{
    public class Global
    {
        /// <summary>
        /// 최초 요청시 생성되어 IIS죽을때까지 간직하고 있는 전역 공통 관련
        /// ConcurrentDictionary 클래스 사용 (여러개 스레드 에서 동시에 엑세스 가능하며 스레드로부터 안전)
        /// </summary>
        private static ConcurrentDictionary<string, User> _session;

        public static ConcurrentDictionary<string, User> Session
        {
            get
            {
                if (_session == null)
                    _session = new ConcurrentDictionary<string,User>(); 
                    return _session;
            }
        }

    }
}
