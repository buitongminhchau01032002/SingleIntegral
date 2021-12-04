﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace SingleIntegral
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void CalBtn_Click(object sender, EventArgs e)
        {
            // tính giá trị các cận
            bool check_a = false;
            List<string> expresson_a = separateExpresson(aTxb.Text);
            decimal a = calExpresson(expresson_a, out check_a);
            if (!check_a)
            {
                resultLable.Text = "Error";
                return;
            }
            bool check_b = false;
            List<string> expresson_b = separateExpresson(bTxb.Text);
            decimal b = calExpresson(expresson_b, out check_b);
            if (!check_b)
            {
                resultLable.Text = "Error";
                return;
            }

            // Lấy cận trên và dưới
            decimal soLon = a > b ? a : b;
            decimal soBe = a < b ? a : b;

            // Khởi tạo các giá trị để tính toán
            decimal result = (decimal)0;
            decimal dx = (decimal)0.0001;
            decimal process = soBe;
            List<string> expresson;
            decimal f_x;
            bool check = false;

            // Tính từng đoạn dx
            while (process + dx < soLon)
            {
                expresson = separateExpresson(funcInput.Text.Replace("x", "(" + (process + dx / 2).ToString("F10") + ")"));
                f_x = calExpresson(expresson, out check);

                if (!check)
                {
                    resultLable.Text = "Error";
                    return;
                }
                else
                {
                    result += (f_x * dx);
                    process += dx;
                }

            }
            // Tính đoạn còn lại
            expresson = separateExpresson(funcInput.Text.Replace("x", "(" + (process + (soLon - process) / 2).ToString("F10") + ")"));
            f_x = calExpresson(expresson, out check);

            if (!check)
            {
                resultLable.Text = "Error";
                return;
            }
            else
            {
                result += (f_x * (soLon - process));
            }

            // Xét dấu của tích phân
            if (a > b)
                result = -result;

            // Hiển thị kết quả
            resultLable.Text = result.ToString("F5");
        }

        private decimal calExpresson(List<string> expresson, out bool check)
        {
            check = false; // true --> biểu thực hợp lệ, false --> không hợp lệ
            while (expresson.Count > 1)
            {
                int vtNgoacMo = -1;
                int vtNgoacDong = -1;

                // Xác định vị trí dấu ngoặc
                for (int i = 0; i < expresson.Count; i++)
                {
                    if (expresson[i] == "(")
                    {
                        vtNgoacMo = i;
                    }
                    if (expresson[i] == ")")
                    {
                        vtNgoacDong = i;
                        break;
                    }
                }
                if (vtNgoacMo == -1 || vtNgoacDong == -1)
                    return 0;

                // Tính toán biểu thức cho đến khi chỉ còn 1 phần tử giữa cặp ngoặc
                while (vtNgoacDong - vtNgoacMo > 2)
                {
                    // Xác định vị trí toán tử sẽ được tính toán
                    int vtToanTu = -1;
                    for (int i = vtNgoacMo + 1; i <= vtNgoacDong - 1; i++)
                    {
                        if (ktPhanTu(expresson[i]) == 1)
                        {
                            if (vtToanTu == -1 ||
                                doUuTien(expresson[vtToanTu]) < doUuTien(expresson[i]) ||
                                vtToanTu == i - 1 // Phía sau toán tử là một toán tử khác --> toán tử phía sau là 1 ngôi
                            )
                                vtToanTu = i;
                        }
                    }

                    if (vtToanTu == -1) // Không tìm thấy toán tử
                    {
                        if (vtNgoacDong - vtNgoacMo != 2 ||
                            ktPhanTu(expresson[vtNgoacMo+1]) != 0
                        )
                            return 0;
                    }
                    else
                    {
                        // Phía trước và phía sau toán tử là số --> toán tử 2 ngôi
                        if (ktPhanTu(expresson[vtToanTu - 1]) == 0 && ktPhanTu(expresson[vtToanTu + 1]) == 0)
                        {
                            decimal toanHang1 = toNum(expresson[vtToanTu - 1]);
                            decimal toanHang2 = toNum(expresson[vtToanTu + 1]);
                            switch (expresson[vtToanTu])
                            {
                                case "+":
                                    expresson[vtToanTu - 1] = (toanHang1 + toanHang2).ToString("F10");
                                    break;
                                case "-":
                                    expresson[vtToanTu - 1] = (toanHang1 - toanHang2).ToString("F10");
                                    break;
                                case "*":
                                    expresson[vtToanTu - 1] = (toanHang1 * toanHang2).ToString("F10");
                                    break;
                                case "/":
                                    expresson[vtToanTu - 1] = (toanHang1 / toanHang2).ToString("F10");
                                    break;
                                case "^":
                                    expresson[vtToanTu - 1] = Math.Pow((double)toanHang1, (double)toanHang2).ToString("F10");
                                    break;
                                default:
                                    return 0;
                            }
                            expresson.RemoveAt(vtToanTu+1);
                            expresson.RemoveAt(vtToanTu);
                            vtNgoacDong -= 2;
                        }
                        // Toán tử 1 ngôi
                        else if (ktPhanTu(expresson[vtToanTu + 1]) == 0)
                        {
                            decimal toanHang = toNum(expresson[vtToanTu + 1]);
                            switch (expresson[vtToanTu])
                            {
                                case "+":
                                    expresson[vtToanTu] = toanHang.ToString("F10");
                                    break;
                                case "-":
                                    expresson[vtToanTu] = (-toanHang).ToString("F10");
                                    break;
                                case "sqrt":
                                    expresson[vtToanTu] = Math.Sqrt((double)toanHang).ToString("F10");
                                    break;
                                case "sin":
                                    expresson[vtToanTu] = Math.Sin((double)toanHang).ToString("F10");
                                    break;
                                case "cos":
                                    expresson[vtToanTu] = Math.Cos((double)toanHang).ToString("F10");
                                    break;
                                case "tan":
                                    expresson[vtToanTu] = Math.Tan((double)toanHang).ToString("F10");
                                    break;
                                case "cot":
                                    expresson[vtToanTu] = ((decimal)1 / (decimal)Math.Tan((double)toanHang)).ToString("F10");
                                    break;
                                case "ln":
                                    expresson[vtToanTu] = Math.Log((double)toanHang).ToString("F10");
                                    break;
                                default:
                                    return 0;
                            }
                            expresson.RemoveAt(vtToanTu + 1);
                            vtNgoacDong--;
                        }
                        else
                            return 0;
                    }
                }

                // Xoá ngoặc
                expresson.RemoveAt(vtNgoacDong);
                expresson.RemoveAt(vtNgoacMo);

            }
            if (expresson.Count == 0 || ktPhanTu(expresson[0]) != 0)
                return 0;
            check = true;
            return toNum(expresson[0]);
        }

        private decimal toNum(string str)
        {
            if (str == "pi")
                return (decimal)Math.PI;
            if (str == "e")
                return (decimal)Math.E;
            return Decimal.Parse(str);
        }

        private int ktPhanTu(string str) //0: số, 1: toán tử, -1: không hợp lệ
        {
            if (str == "pi" || str == "e")
                return 0;
            if ((str[0] >= '0' && str[0] <= '9') || (str.Length >= 2 && str[1] >= '0' && str[1] <= '9'))
            {
                try
                {
                    Decimal.Parse(str);
                    return 0;
                }
                catch (FormatException)
                {
                    return -1;
                }
            }
            if (str == "+" ||
                str == "-" ||
                str == "*" ||
                str == "/" ||
                str == "^" ||
                str == "sqrt" ||
                str == "sin" ||
                str == "cos" ||
                str == "tan" ||
                str == "cot" ||
                str == "ln"
            )
                return 1;
            return -1;
        }

        private int doUuTien(string operatorStr)
        {
            if (operatorStr == "+" || operatorStr == "-")
                return 1;
            if (operatorStr == "*" || operatorStr == "/")
                return 2;
            if (operatorStr == "^")
                return 3;
            return 4;
        }

        private List<string> separateExpresson(string str)
        {
            List<string> listExpresson = new List<string>();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ')
                    continue;
                if (str[i] == '+' ||
                    str[i] == '-' ||
                    str[i] == '*' ||
                    str[i] == '/' ||
                    str[i] == '^' ||
                    str[i] == '(' ||
                    str[i] == ')')
                {
                    listExpresson.Add(str[i].ToString());
                }
                else if ((str[i] >= '0' && str[i] <= '9') || str[i] == '.')
                {
                    string num = "";
                    while (i < str.Length)
                    {
                        if ((str[i] >= '0' && str[i] <= '9') || str[i] == '.')
                        {
                            num += str[i];
                            i++;
                        }
                        else
                        {
                            i--;
                            break;
                        }
                    }
                    listExpresson.Add(num);
                }
                else
                {
                    string otherStr = "";
                    while (i < str.Length)
                    {
                        if (str[i] == ' ' ||
                            (str[i] >= '0' && str[i] <= '9') ||
                            str[i] == '.' ||
                            str[i] == '+' ||
                            str[i] == '-' ||
                            str[i] == '*' ||
                            str[i] == '/' ||
                            str[i] == '^' ||
                            str[i] == '(' ||
                            str[i] == ')')
                        {
                            i--;
                            break;
                        }
                        else
                        {
                            otherStr += str[i];
                            i++;
                            
                        }
                    }
                    listExpresson.Add(otherStr);
                }

            }

            // Thêm cặp ngoặc bao cả biểu thức
            listExpresson.Insert(0, "(");
            listExpresson.Add(")");
            return listExpresson;
        }


    }
}
