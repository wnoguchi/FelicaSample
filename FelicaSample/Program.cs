using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FelicaLib;

namespace FelicaSample
{
    class Program
    {
        /// <summary>
        /// Entry Point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                using (Felica f = new Felica())
                {
                    PrintIDm(f);
                    PrintEdyNo(f);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void PrintIDm(Felica f)
        {
            // 共通ポーリング実行
            f.Polling((int)SystemCode.Common);

            // IDm を取得する
            var idm = f.IDm();

            // バイト数を念のため確認する
            Console.WriteLine(string.Format("Bytes: {0}", idm.Length));

            // IDm をプリントする
            Console.Write("IDm: ");
            foreach (var b in idm)
            {
                Console.Write(string.Format("{0:X2}", b));
            }
            Console.Write("\r\n");
        }

        /// <summary>
        /// Print Edy No.
        /// </summary>
        /// <param name="f"></param>
        private static void PrintEdyNo(Felica f)
        {
            f.Polling((int)SystemCode.Edy);
            byte[] edyNoByteArray = f.ReadWithoutEncryption(0x110B, 0)
                .Skip(2)
                .Take(8)
                .ToArray();

            Console.Write("Edy No.: ");
            foreach (var b in edyNoByteArray)
            {
                Console.Write(string.Format("{0:X2}", b));
            }
            Console.Write("\r\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        private static void ReadNanaco(Felica f)
        {
            f.Polling((int)SystemCode.Common);
            byte[] data = f.ReadWithoutEncryption(0x558b, 0);
            if (data == null)
            {
                throw new Exception("nanaco ID が読み取れません");
            }
            Console.Write("Nanaco ID = ");
            for (int i = 0; i < 8; i++) {
                Console.Write(data[i].ToString("X2"));
            }
            Console.Write("\n");

            for (int i = 0; ; i++)
            {
                data = f.ReadWithoutEncryption(0x564f, i);
                if (data == null) break;

                switch (data[0])
                {
                    case 0x47:
                    default:
                        Console.Write("支払     ");
                        break;
                    case 0x6f:
                        Console.Write("チャージ ");
                        break;
                }

                int value = (data[9] << 24) + (data[10] << 16) + (data[11] << 8) + data[12];
                int year = (value >> 21) + 2000;
                int month = (value >> 17) & 0xf;
                int date = (value >> 12) & 0x1f;
                int hour = (value >> 6) & 0x3f;
                int min = value & 0x3f;

                Console.Write("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}", year, month, date, hour, min);

                value = (data[1] << 24) + (data[2] << 16) + (data[3] << 8) + data[4];
                Console.Write("  金額 {0,6}円", value);

                value = (data[5] << 24) + (data[6] << 16) + (data[7] << 8) + data[8];
                Console.Write("  残高 {0,6}円", value);

                value = (data[13] << 8) + data[14];
                Console.WriteLine("  連番 {0}", value);
            }
        }
    }
}
