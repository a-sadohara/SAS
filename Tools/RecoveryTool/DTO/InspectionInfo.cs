using System;

namespace RecoveryTool.DTO
{
    public class InspectionInfo
    {
        /// <summary>
        /// 検査日付を取得または設定する
        /// </summary>
        /// <value>検査日付</value>
        public DateTime dtInspectionDate { get; set; }

        /// <summary>
        /// 検査番号を取得または設定する
        /// </summary>
        /// <value>検査番号</value>
        public int intInspectionNum { get; set; }

        /// <summary>
        /// 反番を取得または設定する
        /// </summary>
        /// <value>反番</value>
        public string strFabricName { get; set; }
    }
}