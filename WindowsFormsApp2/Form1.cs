using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        // Число поколений
        const int Np = 1000;

        // Число особей в поколении
        const int No = 100;

        // Порог отсечения селекции
        const double l = 0.7;

        // Точность 
        const double eps = 0.01;

        double[] solution;
        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            solution = genetic(-1000.0, 1000);
            DrawGraph(zedGraphControl1);
        }

        double func(double x)
        {
            return 1 / x;
        }

        // Ставим ограничение [-4;0)
        double limit(double pop)
        {
            if ((pop >= -4) && (pop < 0))
            {
                return pop;
            }
            else if (pop < -4)
            {
                return -4.0;
            }
            else
            {
                return 0;
            }
        }

        // Мутация генерация случайной величины
        double mutation(double x0, double x1)
        {
            const int NUM = 10000;
            double x = rnd.NextDouble()*(-4);
            return x;
        }

        // Инверсия: поиск в окрестностях точки
        double inversion(double x, double eps)
        {
            int sign = 0;
            sign++;
            sign %= 2;
            if (sign == 0) return limit(x - eps);
            else return limit(x + eps);
        }

        // Арифметический кроссинговер λ = 0.5
        void crossover(double[] x, double x0, double x1)
        {
            int k = No-1;
            int ln = Convert.ToInt32(Math.Sqrt(No * l));
            for (int i = 0; i < ln; i++)
                for (int j = i + 1; j < ln; j++)
                {
                    x[k] = (x[i] + x[j]) / 2;
                    k--;
                }
            for (int i = 0; i < ln; i++)
            {
                x[k] = inversion(x[i], eps); k--;
            }
            for (int i = ln; i < k; i++)
                x[i] = mutation(x0, x1);
        }

        // Cортировка популяции по приспособленности
        void sort(double[] x, double[] y)
        {
            for (int i = 0; i < No; i++)
                for (int j = i + 1; j < No; j++)
                    if (Math.Abs(y[j]) < Math.Abs(y[i]))
                    {
                        double temp = y[i];
                        y[i] = y[j];
                        y[j] = temp;
                        temp = x[i];
                        x[i] = x[j];
                        x[j] = temp;
                    }
        }

        // Поиск решения
        double[] genetic(double x0, double x1)
        {
            double[] population = new double [No];
            double[] f = new double [No];
            double[] solution = new double[Np];
            int iter = 0;
        
            // Формирование начальной популяции
            for (int i = 0; i < No; i++)
            {
                population[i] = mutation(x0, x1);
                f[i] = func(population[i]);
            }
            sort(population, f);

            // Формирование новых популяций
            do
            {
                solution[iter] = population[0];
                iter++;
                crossover(population, x0, x1);
                for (int i = 0; i < No; i++)
                {
                    f[i] = func(population[i]);
                }
                sort(population, f);
            } while (Math.Abs(f[0]) > eps && iter < Np);
            return solution;
        }

        private void DrawGraph(ZedGraphControl zgc)
        {
            // Получим панель для рисования
            GraphPane pane = zgc.GraphPane;
            
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            // Создадим список точек
            PointPairList list = new PointPairList();

            int xmin = 0;
            int xmax = Np;

            // Заполняем список точек
            for (int x = xmin; x < xmax; x += 1)
            {
                // Добавим в список точку
                list.Add(x, solution[x]);
            }

            LineItem myCurve = pane.AddCurve("Water", list, Color.Blue, SymbolType.None);

            pane.AxisChange();
        }
    }
}
