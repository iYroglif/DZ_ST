using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace DZ_ST
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int[] g_pol = new int[5] { 1, 1, 0, 0, 1 };
        DataTable results;

        public MainWindow()
        {
            InitializeComponent();
        }

        public int[] Coding(int[] vec)
        {
            int[] c_vec = new int[15];
            for (int i = 4; i < 15; ++i)
                c_vec[i] = vec[i - 4];
            for (int i = 14; i > 3; --i)
            {
                if (c_vec[i] == 0)
                    continue;
                for (int k = 0; k < 5; ++k)
                    c_vec[i - k] = c_vec[i - k] ^ g_pol[4 - k];
            }
            for (int i = 4; i < 15; ++i)
                c_vec[i] = vec[i - 4];
            return c_vec;
        }

        public int[] Error(int[] vec, int[] err_vec)
        {
            int[] a_vec = new int[15];
            for (int i = 0; i < 15; ++i)
                a_vec[i] = vec[i] ^ err_vec[i];
            return a_vec;
        }

        public int[] Decoding(int[] vec)
        {
            int[] tmp = (int[])vec.Clone();
            for (int i = 14; i > 3; --i)
            {
                if (tmp[i] == 0)
                    continue;
                for (int k = 0; k < 5; ++k)
                    tmp[i - k] = tmp[i - k] ^ g_pol[4 - k];
            }
            int[] synd = new int[4];
            for (int i = 0; i < 4; ++i)
                synd[i] = tmp[i];
            return synd;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (textbox1.Text.Length != 11)
            {
                MessageBox.Show("Длина информационного вектора должна быть равна 11", "Ошибка");
                button3.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                textbox1.Focus();
                return;
            }

            int[] i_vec = new int[11];

            bool flg = false;
            for (int i = 0; i < 11; ++i)
            {
                if (textbox1.Text[10 - i] != '1' && textbox1.Text[10 - i] != '0')
                    flg = true;
                i_vec[i] = Convert.ToInt32(textbox1.Text[10 - i]) - 48;
            }
            if (flg)
            {
                MessageBox.Show("Проверьте правильность ввода информационного вектора (информационный вектор содержит только 1 и 0)", "Ошибка");
                button3.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                textbox1.Focus();
                return;
            }

            int[] c_vec = Coding(i_vec);

            label1.Content = null;
            for (int i = 14; i >= 0; --i)
                label1.Content += c_vec[i].ToString();

            results = new DataTable { TableName = "Результаты вычисления обнаруживающей способности циклического кода" };
            results.Columns.Add("Кратность ошибки, i", Type.GetType("System.Int32"));
            results.Columns.Add("Общее число ошибок, C\u2071\u2099", Type.GetType("System.Int32"));
            results.Columns.Add("Число обнаруженных ошибок, N\u2080", Type.GetType("System.Int32"));
            results.Columns.Add("Обнаруживающая способность кода, C\u2080", Type.GetType("System.Double"));
            results.Columns[0].ReadOnly = true;
            results.Columns[1].ReadOnly = true;
            results.Columns[2].ReadOnly = true;
            results.Columns[3].ReadOnly = true;
            for (int i = 1; i < 16; ++i)
            {
                int C = 0;
                int N = 0;
                int[] err_vec = new int[15];
                Stack<int> stack = new Stack<int>();
                stack.Push(0);
                while (stack.Count > 0)
                {
                    int value = stack.Pop();
                    int index = stack.Count;
                    if (value - 1 >= 0)
                        err_vec[value - 1] = 0;
                    while (value < 15)
                    {
                        err_vec[value] = 1;
                        index++;
                        value++;
                        stack.Push(value);

                        if (index == i)
                        {
                            int[] synd_vec = Decoding(Error(c_vec, err_vec));
                            ++C;
                            if ((synd_vec[0] != 0) || (synd_vec[1] != 0) || (synd_vec[2] != 0) || (synd_vec[3] != 0))
                                ++N;
                            break;
                        }
                    }
                }
                DataRow dr = results.NewRow();
                dr[0] = i;
                dr[1] = C;
                dr[2] = N;
                dr[3] = (double)N / C;
                results.Rows.Add(dr);
            }
            resultsGrid.ItemsSource = results.AsDataView();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = "Результаты",
                DefaultExt = ".xml",
                Filter = "XML Files|*.xml"
            };
            if (sfd.ShowDialog() == true)
                results.WriteXml(sfd.FileName);
        }

        private void textbox1_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if ((e.Text != "0") && (e.Text != "1"))
                e.Handled = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            label1.Content = null;
            resultsGrid.ItemsSource = null;
        }
    }
}
