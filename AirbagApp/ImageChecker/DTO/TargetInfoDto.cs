using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ImageChecker.DTO
{
    public class TargetInfoDto
    {
        DataTable dtTargetInfo = new DataTable();

        public TargetInfoDto()
        {
            dtTargetInfo.Columns.Add("№", typeof(Int32));
            dtTargetInfo.Columns.Add("RollInfo", typeof(string));
            dtTargetInfo.Columns.Add("CheckStatus", typeof(string));
            dtTargetInfo.Columns.Add("CheckProcess", typeof(string));
            dtTargetInfo.Columns.Add("Process", typeof(string));
            dtTargetInfo.Columns.Add("Status", typeof(Int32));
            dtTargetInfo.Columns.Add("CheckFlg", typeof(Int16));
        }


        public DataTable getTargetInfoDTO()
        {
            return dtTargetInfo;
        }

        public void setTargetInfoDTO(DataTable dtInfo)
        {
            dtTargetInfo = dtInfo;
        }

    }


}
