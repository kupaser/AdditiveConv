using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Addes
{
    public class Alternative
    {
        public string Name { get; set; }
        public List<double> Properties { get; set; }
        public double? K { get; set; }

        public Alternative(string name, List<double> properties)
        {
            Name = name;
            Properties = properties;
        }
    }

    public class Addconv
    {
        private List<Alternative> alternatives;
        public List<Alternative> ConvolvedAlternatives { get; private set; }

        public Addconv(List<Alternative> alternatives)
        {
            this.alternatives = alternatives;
            ConvolvedAlternatives = new List<Alternative>();
        }

        private void Initw()
        {
            try
            {
                int numProperties = alternatives.First().Properties.Count;

                // Найти максимальные значения в каждом столбце
                var maxValues = new double[numProperties];
                for (int i = 0; i < numProperties; i++)
                {
                    maxValues[i] = alternatives.Max(a => a.Properties[i]);
                }

                foreach (var alternative in alternatives)
                {
                    if (alternative.Properties.Count != numProperties)
                    {
                        throw new ArgumentException("разное количество свойств у альтернатив");
                    }

                    // Нормализовать свойства альтернативы, деля на максимальное значение
                    alternative.Properties = alternative.Properties.Select((p, i) => p / maxValues[i]).ToList();

                    // Суммировать нормализованные свойства для вычисления K
                    double sum = alternative.Properties.Sum();
                    alternative.K = sum;

                    // Добавить альтернативу в список ConvolvedAlternatives
                    ConvolvedAlternatives.Add(new Alternative(alternative.Name, alternative.Properties) { K = alternative.K });
                }
                Softmax();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Softmax()
        {
            var expValues = ConvolvedAlternatives.Select(a => Math.Exp(a.K.Value)).ToList();
            double sumExpValues = expValues.Sum();

            for (int i = 0; i < ConvolvedAlternatives.Count; i++)
            {
                ConvolvedAlternatives[i].K = Math.Round(expValues[i] / sumExpValues, 2);
            }
        }

        public void Start(out Alternative bestAlt)
        {
            try
            {
                if (alternatives == null || alternatives.Count == 0)
                {
                    throw new ArgumentException("Пустые альтернативы!");
                }

                Initw();
                bestAlt = ConvolvedAlternatives.OrderByDescending(a => a.K).FirstOrDefault();
            }
            catch (Exception e)
            {
                bestAlt = null;
                MessageBox.Show(e.Message);
            }
        }
    }
}