namespace ImageChecker.DTO
{
    public class HeaderData
    {
        // 検査情報ヘッダ
        public int intInspectionNum { get; set; }
        public string strProductName { get; set; }
        public string strUnitNum { get; set; }
        public string strOrderImg { get; set; }
        public string strFabricName { get; set; }
        public int intInspectionTargetLine { get; set; }
        public int intInspectionEndLine { get; set; }
        public int intInspectionStartLine { get; set; }
        public string strStartDatetime { get; set; }
        public string strEndDatetime { get; set; }
        public string strInspectionDirection { get; set; }
        public string strInspectionDate { get; set; }
        public int intOverDetectionExceptStatus { get; set; }
        public int intAcceptanceCheckStatus { get; set; }
        public string strDecisionStartDatetime { get; set; }
        public string strDecisionEndDatetime { get; set; }

        // 品種登録情報						
        public int intColumnCnt { get; set; }
        public string strAirbagImagepath { get; set; }
    }
}
