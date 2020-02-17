namespace ImageChecker.DTO
{
    public class DecisionResult
    {
        // 合否判定結果
        public int intBranchNum { get; set; }
        public int intLine { get; set; }
        public string strCloumns { get; set; }
        public string strNgReason { get; set; }
        public string strMarkingImagepath { get; set; }
        public string strOrgImagepath { get; set; }
    }
}
