namespace ImageChecker.DTO
{
    public class DecisionResult
    {
        // 合否判定結果
        public int intBranchNum { get; set; }
        public int intLine { get; set; }
        public string strCloumns { get; set; }
        public string strNgFace { get; set; }
        public string strNgReason { get; set; }
        public string strMarkingImagepath { get; set; }
        public string strOrgImagepath { get; set; }
        public int intAcceptanceCheckResult { get; set; }
        public string strAcceptanceCheckDatetime { get; set; }
        public string strAcceptanceCheckWorker { get; set; }
        public int intBeforeAcceptanceCheckResult { get; set; }
        public string strBeforeAcceptanceCheckUpdDatetime { get; set; }
        public string strBeforeAcceptanceCheckWorker { get; set; }
        public string strResultUpdateDatetime { get; set; }
        public string strResultUpdateWorker { get; set; }
        public string strBeforeNgReason { get; set; }
    }
}
