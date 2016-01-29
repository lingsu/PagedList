using System;

namespace PagedList
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BasePageableModel : IPageableModel
    {
        #region Methods

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public int PageIndex
        {
            get
            {
                if (PageNumber > 0)
                    return PageNumber - 1;

                return 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public int PageSize { get; set; }

        #endregion
    }
}