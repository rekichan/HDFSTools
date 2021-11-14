using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Hadoop.WebHDFS;

namespace HDFSTools
{
    class cls_HDFS
    {

        #region Properties
        public static cls_HDFS HDFS;
        public WebHDFSClient client;
        #endregion

        #region Constructor
        private cls_HDFS() { }
        #endregion

        #region Function
        /// <summary>
        /// 单例模式获取实例
        /// </summary>
        /// <returns></returns>
        public static cls_HDFS getInstance()
        {
            if (HDFS == null)
                HDFS = new cls_HDFS();

            return HDFS;
        }
        #endregion

    }
}
