using NLog;

namespace WebForm
{
    public class BasePage : System.Web.UI.Page
    {
        #region variables

        private Logger _logger;

        #endregion

        #region Properties

        protected Logger Log
        {
            get
            {
                return _logger ??
                    (_logger = LogManager.GetCurrentClassLogger(GetType()));
            }
        }

        #endregion
    }
}