using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleWeb
{
    public class LoginIdentity
    {
        internal static readonly LoginIdentity Anonymous = new LoginIdentity(0);

        private int _userID;
        private DateTime _loginDateTime = DateTime.Now;
        private string _ip;
        private string _sID;

        public LoginIdentity()
        {
            _userID = 0;
            _sID = string.Empty;
        }

        public LoginIdentity(int userId)
        {
            _userID = userId;
        }

        public LoginIdentity(string sId)
        {
            _sID = sId;
        }


        public LoginIdentity(int userId, string ip)
        {
            _userID = userId;
            _ip = ip;
        }

        public LoginIdentity(int userId,string ip,string sId)
        {
            _userID = userId;
            _ip = ip;
            _sID = sId;
        }

        #region 公共属性

        public int UserID
        {
            get
            {
                return _userID;
            }

            set
            {
                this._userID = value;
            }
        }

        public string SID
        {
            get
            {
                return _sID;
            }
        }

        #endregion
    }
}
