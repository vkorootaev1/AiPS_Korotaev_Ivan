using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static int N = 128; // размерность
        double[] data = new double[N]; // массив для данных, которые считываются из файла
        double[] real = new double[N]; // массив вещественной части
        double[] imag = new double[N]; // массив мнимой части
        double[] result = new double[N]; // массив для результа
        // Коротаев Иван БАС-19
        // 6 вариант; зашифрованное слово - CARINA

        private void button1_Click(object sender, EventArgs e) // Обработчик событий нажатия на кнопку
        {
            Stopwatch watch = new Stopwatch(); // Засекаем время выполнения программы 
            if (comboBox1.SelectedIndex > -1)
            {
                // Если выбрали дискретное преобразование фурье
                try
                {
                    if (comboBox1.SelectedIndex == 0)
                    {

                        var path = File_name(); // находим путь к выбранному файлу
                        Reading(data, path, N); // считываем данные из файла в массив
                        watch.Start();
                        for (int k = 0; k < N; k++) // дискретное преобразование Фурье
                        {
                            for (int n = 0; n < N; n++)
                            {
                                real[k] += data[n] * Math.Cos(Coeff(k, n, N));
                                imag[k] += data[n] * Math.Sin(Coeff(k, n, N));
                            }
                            result[k] = Math.Round(Math.Sqrt(real[k] * real[k] + imag[k] * imag[k]) / N * 2, 0); // Получение вещественной амплитуды после преобразования 
                        }
                        watch.Stop();
                        string str = Decoding(result); // декодируем текст по ascii таблице
                        textBox1.Text = str;
                    }

                    //Если выбрали быстрое дискретное преобразование Фурье

                    else
                    {
                        var path = File_name();  // находим путь к выбранному файлу
                        Reading(data, path, N); // считываем данные из файла в массив
                        watch.Start();
                        var data_complex = Double2Complex(N, data); // переводим наши данные в комплексный вид
                        Complex[] result_complex = Fast(data_complex); // Вызов быстрого преобразование Фурье
                        double[] result = Complex2Double(result_complex, N); // Возвращаем данные из комплексного вида и получение вещественной амплитуды преобразованного сигнала
                        watch.Stop();
                        string str = Decoding(result); // декодируем текст по ascii таблице
                        textBox1.Text = str;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Не удалось выполнить!\n" +ex.Message, "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите способ", "Ошибка");
            }
            textBox2.Text = watch.ElapsedTicks.ToString() + " тактов";
        }

        private double Coeff(int k,int n,int N) // Нахождение коэффицента для стандартного дискретного преобразования Фурье
        {
            double coeff = 2 * Math.PI * n * k/N;
            return coeff;
        }

        private string File_name() // Выбор файла для считывания 
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "TXT|* .txt";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Вы не выбрали файл!");
            }
            var str = dialog.FileName;
            return str;
        }
        private  double [] Reading( double[] data,string path,int N) // Считывание данных из файла
        {
            StreamReader sr = new StreamReader(path);
            for (int i = 0; i < N; i++)
            {
                data[i] = Double.Parse(sr.ReadLine());
            }
            sr.Close();
            return data;
        }

        private string Decoding(double [] result) // Декодирование результативных данных по таблице ascii
        {
            string str ="";
            for (int i = 1; i < 7; i++)
            {
                if (result[i] != 0)
                {
                    str += (char)result[i];
                }
            }
            return str;
        }

        private Complex Coeff(int k, int N) // Получение коэффицента
        {
            Complex coeff;
            double argument = -2 * Math.PI * k / N;
            coeff = new Complex(Math.Cos(argument), Math.Sin(argument));
            return coeff;
        }

        private double[] Complex2Double(Complex [] result_complex,int N) // Функция перевода из комлексного вида в вещественный
        {
            double[] result = new double[N];
            for(int i =0; i<N;i++ )
            {
                result[i] = Math.Round(Math.Sqrt(result_complex[i].Real* result_complex[i].Real + result_complex[i].Imaginary * result_complex[i].Imaginary) / N * 2.0,0); // Получение амплитуды после преобразования 
            }
            return result;
        }

        private Complex[] Double2Complex(int N,double [] data) // Перевод из вещественного вида в комплексный
        {
            Complex [] complex = new Complex[N];
            for(int i = 0; i<N;i++)
            {
                complex[i] = new Complex (data[i], 0);
            }
            return complex;
        }


        private Complex[] Fast(Complex [] data_complex) // Рекурсивная функция быстрого преобразование Фурье (каждый раз вдвое уменьшаем размерность)
        {
            int N = data_complex.Length; // Текущая размерность
            Complex[] result;
            if (N == 2) // Если размерность равна 2 (т.е. больше уменьшить размерность нельзя)
            {
                result = new Complex[2];
                result[0] = data_complex[0] + data_complex[1]; // значение с четным номером
                result[1] = data_complex[0] - data_complex[1]; // значение с нечетным номером
            }
            else // Разбиение сигнала на 2
            {
                Complex[] data_complex_chet = new Complex[N / 2]; // Создаем массив с размерностью в два раза меньше предыдущей (четные номера)
                Complex[] data_complex_nechet = new Complex[N / 2]; // Создаем массив с размерностью в два раза меньше предыдущей (нечетные номера)
                for (int i = 0; i < N / 2; i++)
                {
                    data_complex_chet[i] = data_complex[2 * i]; 
                    data_complex_nechet[i] = data_complex[2 * i + 1];
                }
                Complex[] result_chet = Fast(data_complex_chet); // повторение БПФ (четные номера)
                Complex[] result_nechet = Fast(data_complex_nechet); // повторение БПФ (нечетные номера)
                result = new Complex[N];
                for (int i = 0; i < N / 2; i++) //"Сборка" преобразованного сигнала
                {
                    result[i] = result_chet[i] + Coeff(i, N) * result_nechet[i];
                    result[i + N / 2] = result_chet[i] - Coeff(i, N) * result_nechet[i];
                }
            }
            return result;
        }
    }
}
