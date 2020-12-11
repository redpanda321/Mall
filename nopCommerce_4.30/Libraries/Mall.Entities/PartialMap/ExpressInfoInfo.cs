using System;
using System.Linq;

namespace Mall.Entities
{
    public partial class ExpressInfoInfo
    {
        /// <summary>
        /// 是否为数字
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckExpressCodeIsValid(string code)
        {
            long current;
            return long.TryParse(code, out current);
        }
        /// <summary>
        /// 生成快递单号
        /// </summary>
        /// <param name="expressName"></param>
        /// <param name="currentExpressCode"></param>
        /// <returns></returns>
        public string NextExpressCode(string expressName, string currentExpressCode)
        {
            if (expressName.ToLower().Contains("ems") || expressName.Contains("邮政"))
            {
                return EMSNextCode(currentExpressCode);
            }
            else if (expressName.Contains("顺丰"))
            {
                return SFNextCode(currentExpressCode);
            }
            else if (expressName.IndexOf("宅急送") > -1)
            {
                return ZJS_NextExpressCode(currentExpressCode);
            }
            return DefaultNextCode(currentExpressCode);
        }
        /// <summary>
        /// 默认快递单号生成
        /// </summary>
        /// <param name="currentExpressCode"></param>
        /// <returns></returns>
        private string DefaultNextCode(string currentExpressCode)
        {
            long current;
            bool isValid = long.TryParse(currentExpressCode, out current);
            if (!isValid)
                throw new FormatException("快递单号格式不正确,正确的格式为数字");
            return (current + 1).ToString();
        }
        /// <summary>
        /// 宅急送
        /// </summary>
        /// <param name="currentExpressCode"></param>
        /// <returns></returns>
        public string ZJS_NextExpressCode(string currentExpressCode)
        {
            var retNo = Convert.ToInt64(currentExpressCode) + 11;
            if (retNo % 10 > 6)
            {
                retNo -= 7;
            }
            return retNo.ToString().PadLeft(currentExpressCode.Length, '0');
        }

        /// <summary>
        /// EMS快递单号生成，最少9位
        /// </summary>
        /// <param name="currentExpressCode"></param>
        /// <returns></returns>
        private string EMSNextCode(string currentExpressCode)
        {
            if (currentExpressCode.Length < 9)
            {
                throw new Mall.Core.MallException("起始快递单号格式不正确，至少9位");
            }
            var serialNo = Convert.ToInt64(currentExpressCode.Substring(2, 8));
            if (serialNo < 99999999)
                serialNo++;
            var strSerialNo = serialNo.ToString().PadLeft(8, '0');
            var temp = currentExpressCode.Substring(0, 2) + strSerialNo + currentExpressCode.Substring(10, 1);
            temp = currentExpressCode.Substring(0, 2) + strSerialNo + getEMSLastNum(temp) + currentExpressCode.Substring(11, 2);
            return temp;
        }
        private string getEMSLastNum(string emsno)
        {
            var arrems = emsno.ToList();
            var emslastno = int.Parse(arrems[2].ToString()) * 8;
            emslastno += int.Parse(arrems[3].ToString()) * 6;
            emslastno += int.Parse(arrems[4].ToString()) * 4;
            emslastno += int.Parse(arrems[5].ToString()) * 2;
            emslastno += int.Parse(arrems[6].ToString()) * 3;
            emslastno += int.Parse(arrems[7].ToString()) * 5;
            emslastno += int.Parse(arrems[8].ToString()) * 9;
            emslastno += int.Parse(arrems[9].ToString()) * 7;
            emslastno = 11 - emslastno % 11;
            if (emslastno == 10)
                emslastno = 0;
            else if (emslastno == 11)
                emslastno = 5;
            return emslastno.ToString();
        }
        /// <summary>
        /// 顺丰快递单号生成，最少12位
        /// </summary>
        /// <param name="currentExpressCode"></param>
        /// <returns></returns>
        private string SFNextCode(string currentExpressCode)
        {
            if (currentExpressCode.Length < 12)
            {
                throw new Mall.Core.MallException("起始快递单号格式不正确，至少12位");
            }
            int[] oldNum = new int[12];
            int[] newNum = new int[12];
            var sfArr = currentExpressCode.ToList();
            string fri = currentExpressCode.Substring(0, 11);
            string Nfri = string.Empty;
            //先将前11位加1，存储为新前11位
            if (currentExpressCode.Substring(0, 1) == "0")
            {
                Nfri = "0" + (Convert.ToInt64(fri) + 1).ToString();
            }
            else
            {
                Nfri = (Convert.ToInt64(fri) + 1).ToString();
            }
            ///原始前12位
            /// 
            for (int i = 0; i < 12; i++)
            {
                oldNum[i] = int.Parse(sfArr[i].ToString());
            }
            //新11位
            var NewfriArr = Nfri.ToList();
            for (int i = 0; i < 11; i++)
            {
                newNum[i] = int.Parse(Nfri[i].ToString());
            }
            if (newNum[8] - oldNum[8] == 1 && (oldNum[8] % 2 == 1))
            {
                if (oldNum[11] - 8 >= 0)
                    newNum[11] = oldNum[11] - 8;
                else
                    newNum[11] = oldNum[11] - 8 + 10;

            }
            else if (newNum[8] - oldNum[8] == 1 && (oldNum[8] % 2 == 0))
            {
                if (oldNum[11] - 7 >= 0)
                    newNum[11] = oldNum[11] - 7;
                else
                    newNum[11] = oldNum[11] - 7 + 10;
            }
            else
            {
                if ((oldNum[9] == 3 || oldNum[9] == 6) && oldNum[10] == 9)
                {
                    if (oldNum[11] - 5 >= 0)
                        newNum[11] = oldNum[11] - 5;
                    else
                        newNum[11] = oldNum[11] - 5 + 10;

                }
                else if (oldNum[10] == 9)
                {
                    if (oldNum[11] - 4 >= 0)
                        newNum[11] = oldNum[11] - 4;
                    else
                        newNum[11] = oldNum[11] - 4 + 10;
                }
                else
                {
                    if (oldNum[11] - 1 >= 0)
                        newNum[11] = oldNum[11] - 1;
                    else
                        newNum[11] = oldNum[11] - 1 + 10;
                }

            }
            return Nfri + newNum[11].ToString();
        }
    }
}
