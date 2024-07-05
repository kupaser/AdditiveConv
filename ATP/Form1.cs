using Addes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ATP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //Привязываем событие EditingControlShowing
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            this.MaximizeBox = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        #region Methods
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int columnIndex = dataGridView1.CurrentCell.ColumnIndex;
            TextBox tb = e.Control as TextBox;

            if (tb != null)
            {
                // Удаляем обработчики событий KeyPress для всех текстовых полей
                tb.KeyPress -= FirstColumn_KeyPress;
                tb.KeyPress -= OtherColumns_KeyPress;
                tb.KeyPress -= LastColumn_KeyPress;

                // Проверяем столбец и добавляем события
                if (columnIndex == 0)
                    tb.KeyPress += FirstColumn_KeyPress; // Только буквы для первого столбца
                else if (columnIndex != dataGridView1.Columns.Count - 1)
                    tb.KeyPress += OtherColumns_KeyPress; // Только цифры для остальных столбцов
                else
                    tb.KeyPress += LastColumn_KeyPress; // Блокировка для последнего столбца
            }
        }
        private void FirstColumn_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем вводить только буквы и пробелы для первого столбца
            if (!char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }
        private void OtherColumns_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем вводить только цифры для остальных столбцов
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }
        private void LastColumn_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Блокируем ввод для последнего столбца
            e.Handled = true;
        }
        private bool IsNotEmpty(DataGridView dataGridView)
        {
            for (int rowIndex = 0; rowIndex < dataGridView.Rows.Count - 1; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < dataGridView.Columns.Count - 1; columnIndex++)
                {
                    object value = dataGridView.Rows[rowIndex].Cells[columnIndex].Value;
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        // Если найдена пустая ячейка, возвращаем false
                        return false;
                    }
                }
            }
            // Если все ячейки заполнены, возвращаем true
            return true;
        }        
        //обновление полей
        public void Refresher(in List<Alternative> vals)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }

            List<string> lastColumnData = new List<string>();
            foreach (Alternative val in vals) 
            {
                lastColumnData.Add(val.K.ToString());
            }            
            if (dataGridView1.Rows.Count > 0)
            {                
                int lastColumnIndex = dataGridView1.Columns.Count - 1;
                if (lastColumnIndex >= 0)
                {
                    // Перебираем строки и устанавливаем значения в последний столбец
                    for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count-1; rowIndex++)
                    {                        
                        dataGridView1.Rows[rowIndex].Cells[lastColumnIndex].Value = lastColumnData[rowIndex];
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось найти последний столбец.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Не удалось найти строки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (IsNotEmpty(dataGridView1))
            {                
                List<Alternative> alternatives = new List<Alternative>();
                var names = new List<string>();
                var Altos = GetMatrixFromDataGridView(out names);

                for (int i = 0; i < names.Count; i++)
                {
                    alternatives.Add(new Alternative(names[i], Altos[i]));
                }

                Addconv additiveConvolution = new Addconv(alternatives);
                Alternative bestAlternative;

                additiveConvolution.Start(out bestAlternative);

                textBox1.Text = $"Оптимальный тарифный план из предложенных:\n{bestAlternative?.Name}\n\n\nПодходит на: {bestAlternative?.K * 100}%";
                Refresher(additiveConvolution.ConvolvedAlternatives);
                Recolor(bestAlternative?.Name);
            }
            else
            {
                MessageBox.Show("Имеется пустое окно!");
            }
        }
        //меняет цвет строчки
        private void Recolor(string str)
        {            
            string targetValue = str;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null && row.Cells[0].Value.ToString() == targetValue)
                {
                    row.DefaultCellStyle.BackColor = Color.Yellow;
                    break;
                }
            }
        }
        //для получения матрицы из данных DataGrid (все столбцы, кроме первого и последнего)
        public List<List<double>> GetMatrixFromDataGridView(out List<string> rowNames)
        {
            List<List<double>> matrix = new List<List<double>>();
            rowNames = new List<string>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                List<double> rowData = new List<double>();
                for (int i = 1; i < row.Cells.Count - 1; i++)
                {
                    if (row.Cells[i].Value != null && double.TryParse(row.Cells[i].Value.ToString(), out double value))
                    {
                        rowData.Add(value);
                    }
                    else
                    {
                        rowData.Add(0); // Добавляем 0 в случае ошибки
                    }
                }

                // Добавляем текущую строку матрицы в общий список
                matrix.Add(rowData);

                // Добавляем имя строки (первый столбец) в список
                if (row.Cells.Count > 0 && row.Cells[0].Value != null)
                {
                    rowNames.Add(row.Cells[0].Value.ToString());
                }
            }
            return matrix;
        }
        //для изменения значений последнего столбца на K
        public void ForK(List<double> k)
        {
            if (dataGridView1.Rows.Count != k.Count)
            {
                //при несовпадении размеров
                throw new Exception("Ошибка: Количество строк в DataGridView не соответствует размеру вектора K.");
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (double.TryParse(k[i].ToString(), out double value))
                {
                    dataGridView1.Rows[i].Cells[dataGridView1.Columns.Count - 1].Value = value;
                }
                else
                {
                    //при ошибке преобразования
                    throw new Exception($"Ошибка при преобразовании значения в векторе K, индекс {i}");
                }
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
