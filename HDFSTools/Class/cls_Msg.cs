using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDFSTools
{
    class cls_Msg
    {

        #region Handle
        public static IntPtr hwndFrmMain;
        #endregion

        #region Message
        public const int USER = 0x0400;
        public const int MAIN_UI_ENABLE = USER + 1;
        public const int MAIN_UI_DISABLE = MAIN_UI_ENABLE + 1;
        public const int LIST_DIRECTORIES = MAIN_UI_DISABLE + 1;
        public const int LIST_DIRECTORIES_AND_FILES = LIST_DIRECTORIES + 1;
        public const int NAVIGATE_PATH_CHANGE = LIST_DIRECTORIES_AND_FILES +1;
        public const int ASSIGN_PATH = NAVIGATE_PATH_CHANGE + 1;
        public const int SHOW_MEMORY_USED = ASSIGN_PATH + 1;
        public const int SHOW_LIST_VIEW_ITEMS = SHOW_MEMORY_USED + 1;
        public const int TREEVIEW_ENDUPDATE = SHOW_LIST_VIEW_ITEMS + 1;
        public const int CREATE_FOLDER = TREEVIEW_ENDUPDATE + 1;
        #endregion

    }
}
