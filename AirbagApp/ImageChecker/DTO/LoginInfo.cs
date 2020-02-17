using System.Windows.Forms;

namespace ImageChecker.DTO
{
    class LoginInfo
    {
        // パラメータ
        public string strEmployeeNum = "";
        public string strWorkerName = "";
        public int intDispNum = -1;
        public bool bolStatus = false;

        /// <summary>
        /// ログイン処理
        /// </summary>
        /// <param name="strEmployeeNum">社員番号</param>
        /// <param name="strWorkerNm">作業者名</param>
        /// <param name="intDispNum">表示数</param>
        public void Login(string parEmployeeNum, string parWorkerNm, int parDispNum)
        {
            strEmployeeNum = parEmployeeNum;
            strWorkerName = parWorkerNm;
            intDispNum = parDispNum;
            bolStatus = true;
        }

        /// <summary>
        /// ログアウト処理
        /// </summary>
        public void Logout()
        {
            strEmployeeNum = "";
            strWorkerName = "";
            intDispNum = -1;
            bolStatus = false;

            // ログイン画面以外を閉じる
            // 起動画面ループ
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is Login)
                {
                    // ログイン画面　⇒　再表示
                    frm.Visible = true;
                }
                else
                {
                    // ログイン画面以外　⇒　閉じる
                    frm.Close();
                }
            }
        }
    }
}