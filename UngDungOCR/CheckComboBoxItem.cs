using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UngDungOCR
{
    class CheckComboBoxItem
    {
        public string Text { get; set; }
        public bool Checked { get; set; }

        public CheckComboBoxItem(string text, bool isChecked = false)
        {
            Text = text;
            Checked = isChecked;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
